using Core.Entities;
using Core.Interfaces;
using Infra.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Web.Services;

namespace Web.Areas.Admin.Pages.Products
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBlobStorageService _blobStorageService;
        private readonly ILogger<EditModel> _logger;
        private readonly AppDbContext _db;

        public EditModel(IUnitOfWork unitOfWork, IBlobStorageService blobStorageService, ILogger<EditModel> logger, AppDbContext db)
        {
            _unitOfWork = unitOfWork;
            _blobStorageService = blobStorageService;
            _logger = logger;
            _db = db;
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
        public string? ImageOrder { get; set; }
        [BindProperty]
        public string? VariantsJson { get; set; }
        public IReadOnlyList<Category> Categories { get; private set; } = [];
        public string CategoriesVariantTypesJson { get; private set; } = "{}";
        public string ExistingVariantsJson { get; private set; } = "[]";

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            await LoadCategoriesAsync();
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product is null)
            {
                return RedirectToPage("/Products/Index", new { area = "Admin" });
            }

            Product = product;
            BuildExistingVariantsJson();
            return Page();
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

            try
            {
                // Load the tracked entity so we only update scalar properties
                var existing = await _db.Products.FindAsync(Product.Id);
                if (existing is null)
                {
                    ModelState.AddModelError("", "Product not found.");
                    return Page();
                }

                // Copy scalar fields from the form-bound model
                existing.Name = Product.Name;
                existing.Description = Product.Description;
                existing.CategoryId = Product.CategoryId;
                existing.Price = Product.Price;
                existing.Stock = Product.Stock;
                existing.ReorderLevel = Product.ReorderLevel;
                existing.ReorderQuantity = Product.ReorderQuantity;
                existing.IsFeatured = Product.IsFeatured;

                var uploads = GetUploads();
                var (orderMap, newPositions) = ParseImageOrder(ImageOrder);

                if (uploads.Any())
                {
                    var images = await SaveImagesToAzureAsync(uploads, newPositions);
                    foreach (var img in images) { img.ProductId = existing.Id; }
                    _db.ProductImages.AddRange(images);
                }

                if (orderMap.Count > 0)
                {
                    await _unitOfWork.Products.UpdateImageOrderAsync(existing.Id, orderMap);
                }

                // Update variants
                await SaveVariantsAsync(existing.Id, existing.CategoryId);

                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Product updated successfully!";
                return RedirectToPage("/Products/Index", new { area = "Admin" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product");
                ModelState.AddModelError("", "Error updating product. Please try again.");
                return Page();
            }
        }

        private async Task SaveVariantsAsync(Guid productId, Guid categoryId)
        {
            // Load existing variants with their images
            var existingVariants = await _db.ProductVariants
                .Include(v => v.Options)
                .Include(v => v.Images)
                .Where(v => v.ProductId == productId)
                .ToListAsync();

            // Collect all keep-image IDs from the incoming JSON so we know what to preserve
            var allKeepIds = new HashSet<Guid>();
            List<CreateModel.VariantInput>? variantInputs = null;
            if (!string.IsNullOrWhiteSpace(VariantsJson))
            {
                variantInputs = JsonSerializer.Deserialize<List<CreateModel.VariantInput>>(VariantsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (variantInputs != null)
                {
                    foreach (var vi in variantInputs)
                    {
                        if (vi.KeepImageIds != null)
                            foreach (var kid in vi.KeepImageIds) allKeepIds.Add(kid);
                    }
                }
            }

            // Handle existing variant images before deleting variants
            var variantImages = existingVariants.SelectMany(v => v.Images).ToList();
            foreach (var img in variantImages)
            {
                if (allKeepIds.Contains(img.Id))
                {
                    // Detach from the variant so the variant can be deleted
                    img.ProductVariantId = null;
                }
                else
                {
                    // Delete from blob storage and soft-delete the image record
                    try { await _blobStorageService.DeleteAsync(img.Url, "product-images"); } catch { }
                    _db.ProductImages.Remove(img);
                }
            }
            await _db.SaveChangesAsync();

            // Now safe to remove variants (no FK references remain)
            _db.ProductVariants.RemoveRange(existingVariants);
            await _db.SaveChangesAsync();

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

                // Re-assign kept images to the new variant
                if (input.KeepImageIds != null)
                {
                    foreach (var imgId in input.KeepImageIds)
                    {
                        var existingImg = await _db.ProductImages.FirstOrDefaultAsync(im => im.Id == imgId);
                        if (existingImg != null)
                        {
                            existingImg.ProductVariantId = variant.Id;
                            existingImg.SortOrder = sortOrder++;
                        }
                    }
                }

                _db.ProductVariants.Add(variant);
            }

            await _db.SaveChangesAsync();
        }

        private void BuildExistingVariantsJson()
        {
            var variants = Product.Variants
                .Where(v => v.IsActive)
                .Select(v => new
                {
                    sku = v.SKU,
                    price = v.Price,
                    stock = v.Stock,
                    options = v.Options.ToDictionary(
                        o => o.CategoryVariantType?.Name ?? "",
                        o => o.Value),
                    images = v.Images.OrderBy(i => i.SortOrder).Select(i => new { id = i.Id, url = i.Url }).ToList()
                })
                .ToList();
            ExistingVariantsJson = JsonSerializer.Serialize(variants);
        }

        private async Task LoadCategoriesAsync()
        {
            Categories = await _unitOfWork.Categories.ListAsync();
            var dict = Categories.ToDictionary(
                c => c.Id.ToString(),
                c => c.VariantTypes.OrderBy(v => v.SortOrder).Select(v => v.Name).ToList());
            CategoriesVariantTypesJson = JsonSerializer.Serialize(dict);
        }

        public async Task<IActionResult> OnPostDeleteImageAsync(Guid id, Guid imageId)
        {
            try
            {
                var image = await _unitOfWork.Products.GetImageByIdAsync(imageId);
                if (image is not null)
                {
                    await _blobStorageService.DeleteAsync(image.Url, "product-images");
                    await _unitOfWork.Products.RemoveImageAsync(imageId);
                    TempData["SuccessMessage"] = "Image deleted successfully!";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image");
                TempData["ErrorMessage"] = "Error deleting image. Please try again.";
            }

            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostAddSpecificationAsync(Guid id)
        {
            if (!string.IsNullOrWhiteSpace(SpecKey) && !string.IsNullOrWhiteSpace(SpecValue))
            {
                await _unitOfWork.Products.AddSpecificationAsync(id, new ProductSpecification
                {
                    Id = Guid.NewGuid(),
                    Key = SpecKey,
                    Value = SpecValue
                });
            }

            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostRemoveSpecificationAsync(Guid id, Guid specId)
        {
            await _unitOfWork.Products.RemoveSpecificationAsync(specId);
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostMoveImageAsync(Guid id, Guid imageId, int direction)
        {
            await _unitOfWork.Products.MoveImageAsync(imageId, direction);
            return RedirectToPage(new { id });
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

        /// <summary>
        /// Uploads product images to Azure Blob Storage
        /// </summary>
        private async Task<List<ProductImage>> SaveImagesToAzureAsync(IEnumerable<IFormFile> files, IReadOnlyList<int>? sortOrders = null)
        {
            var images = new List<ProductImage>();
            var index = 0;

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
                        Url = imageUrl,
                        SortOrder = sortOrders is not null && index < sortOrders.Count ? sortOrders[index] : -1
                    });
                    index++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error uploading image {FileName} to Azure Blob Storage", file.FileName);
                    throw;
                }
            }

            return images;
        }

        private static (Dictionary<Guid, int> orderMap, List<int> newPositions) ParseImageOrder(string? order)
        {
            var orderMap = new Dictionary<Guid, int>();
            var newPositions = new List<int>();

            if (string.IsNullOrWhiteSpace(order))
            {
                return (orderMap, newPositions);
            }

            var tokens = order.Split(',', StringSplitOptions.RemoveEmptyEntries);
            for (var index = 0; index < tokens.Length; index++)
            {
                var token = tokens[index];
                if (token.StartsWith("e:", StringComparison.OrdinalIgnoreCase))
                {
                    var raw = token.Substring(2);
                    if (Guid.TryParse(raw, out var id))
                    {
                        orderMap[id] = index;
                    }
                }
                else if (token == "n")
                {
                    newPositions.Add(index);
                }
            }

            return (orderMap, newPositions);
        }
    }
}
