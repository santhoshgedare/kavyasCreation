using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using Web.Services;

namespace Web.Areas.Admin.Pages.Products
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBlobStorageService _blobStorageService;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(IUnitOfWork unitOfWork, IBlobStorageService blobStorageService, ILogger<CreateModel> logger)
        {
            _unitOfWork = unitOfWork;
            _blobStorageService = blobStorageService;
            _logger = logger;
        }

        [BindProperty]
        public Product Product { get; set; } = new();
        [BindProperty]
        public List<IFormFile> ImageUploads { get; set; } = [];
        [BindProperty]
        public string? SpecKey { get; set; }
        [BindProperty]
        public string? SpecValue { get; set; }
        [BindProperty]
        public string? VariantsJson { get; set; }
        public IReadOnlyList<Category> Categories { get; private set; } = [];
        public string CategoriesVariantTypesJson { get; private set; } = "{}";

        public async Task OnGetAsync()
        {
            await LoadCategoriesAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadCategoriesAsync();
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (Product.CategoryId == Guid.Empty)
            {
                ModelState.AddModelError("Product.CategoryId", "Select a category.");
                return Page();
            }

            Product.Id = Guid.NewGuid();
            var uploads = GetUploads();

            try
            {
                if (uploads.Any())
                {
                    var images = await SaveImagesToAzureAsync(uploads);
                    await _unitOfWork.Products.AddAsync(Product);
                    await _unitOfWork.Products.AddImagesAsync(Product.Id, images);
                }
                else
                {
                    await _unitOfWork.Products.AddAsync(Product);
                }

                if (!string.IsNullOrWhiteSpace(SpecKey) && !string.IsNullOrWhiteSpace(SpecValue))
                {
                    await _unitOfWork.Products.AddSpecificationAsync(Product.Id, new ProductSpecification
                    {
                        Id = Guid.NewGuid(),
                        Key = SpecKey,
                        Value = SpecValue
                    });
                }

                // Save variants
                if (!string.IsNullOrWhiteSpace(VariantsJson))
                {
                    await SaveVariantsAsync(Product.Id, Product.CategoryId);
                }

                TempData["SuccessMessage"] = "Product created successfully!";
                return RedirectToPage("/Products/Index", new { area = "Admin" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                ModelState.AddModelError("", "Error creating product. Please try again.");
                return Page();
            }
        }

        private async Task SaveVariantsAsync(Guid productId, Guid categoryId)
        {
            if (string.IsNullOrWhiteSpace(VariantsJson)) return;

            var variantInputs = JsonSerializer.Deserialize<List<VariantInput>>(VariantsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (variantInputs is null || variantInputs.Count == 0) return;

            var category = await _unitOfWork.Categories.GetByIdWithVariantTypesAsync(categoryId);
            if (category is null) return;

            var variantTypeMap = category.VariantTypes.ToDictionary(v => v.Name, v => v.Id, StringComparer.OrdinalIgnoreCase);

            for (var i = 0; i < variantInputs.Count; i++)
            {
                var input = variantInputs[i];
                var variant = new ProductVariant
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId,
                    SKU = input.SKU,
                    Price = input.Price,
                    Stock = input.Stock,
                    IsActive = true
                };

                if (input.Options != null)
                {
                    foreach (var opt in input.Options)
                    {
                        if (variantTypeMap.TryGetValue(opt.Key, out var typeId))
                        {
                            variant.Options.Add(new ProductVariantOption
                            {
                                Id = Guid.NewGuid(),
                                ProductVariantId = variant.Id,
                                CategoryVariantTypeId = typeId,
                                Value = opt.Value
                            });
                        }
                    }
                }

                // Upload variant-specific images
                var variantFiles = Request.Form.Files.Where(f => f.Name == $"variant-images-{i}").ToList();
                var sortOrder = 0;
                foreach (var file in variantFiles)
                {
                    if (file.Length == 0) continue;
                    var url = await _blobStorageService.UploadAsync(file, "product-images");
                    variant.Images.Add(new ProductImage
                    {
                        Id = Guid.NewGuid(),
                        ProductId = productId,
                        ProductVariantId = variant.Id,
                        Url = url,
                        SortOrder = sortOrder++
                    });
                }

                Product.Variants.Add(variant);
            }

            await _unitOfWork.Products.UpdateAsync(Product);
        }

        private async Task LoadCategoriesAsync()
        {
            Categories = await _unitOfWork.Categories.ListAsync();
            var dict = Categories.ToDictionary(
                c => c.Id.ToString(),
                c => c.VariantTypes.OrderBy(v => v.SortOrder).Select(v => v.Name).ToList());
            CategoriesVariantTypesJson = JsonSerializer.Serialize(dict);
        }

        private List<IFormFile> GetUploads()
        {
            if (ImageUploads.Any())
            {
                return ImageUploads;
            }

            // Only return general product image uploads, not variant-images-* files
            return Request.Form.Files
                .Where(f => !f.Name.StartsWith("variant-images-", StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        private async Task<List<ProductImage>> SaveImagesToAzureAsync(IEnumerable<IFormFile> files)
        {
            var images = new List<ProductImage>();

            foreach (var file in files)
            {
                if (file.Length == 0)
                {
                    continue;
                }

                try
                {
                    var imageUrl = await _blobStorageService.UploadAsync(file, "product-images");
                    _logger.LogInformation("Image uploaded to Azure: {ImageUrl}", imageUrl);

                    images.Add(new ProductImage
                    {
                        Id = Guid.NewGuid(),
                        Url = imageUrl
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error uploading image {FileName} to Azure Blob Storage", file.FileName);
                    throw;
                }
            }

            return images;
        }

        public class VariantInput
        {
            public string? SKU { get; set; }
            public decimal Price { get; set; }
            public int Stock { get; set; }
            public Dictionary<string, string> Options { get; set; } = new();
            public List<Guid>? KeepImageIds { get; set; }
        }
    }
}
