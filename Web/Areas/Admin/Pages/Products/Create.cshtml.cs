using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
        public IReadOnlyList<Category> Categories { get; private set; } = [];

        public async Task OnGetAsync()
        {
            Categories = await _unitOfWork.Categories.ListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Categories = await _unitOfWork.Categories.ListAsync();
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
                    
                    if (!string.IsNullOrWhiteSpace(SpecKey) && !string.IsNullOrWhiteSpace(SpecValue))
                    {
                        await _unitOfWork.Products.AddSpecificationAsync(Product.Id, new ProductSpecification
                        {
                            Id = Guid.NewGuid(),
                            Key = SpecKey,
                            Value = SpecValue
                        });
                    }
                    
                    TempData["SuccessMessage"] = "Product created successfully with images!";
                    return RedirectToPage("/Products/Index", new { area = "Admin" });
                }

                await _unitOfWork.Products.AddAsync(Product);
                if (!string.IsNullOrWhiteSpace(SpecKey) && !string.IsNullOrWhiteSpace(SpecValue))
                {
                    await _unitOfWork.Products.AddSpecificationAsync(Product.Id, new ProductSpecification
                    {
                        Id = Guid.NewGuid(),
                        Key = SpecKey,
                        Value = SpecValue
                    });
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

        private List<IFormFile> GetUploads()
        {
            if (ImageUploads.Any())
            {
                return ImageUploads;
            }

            return Request.Form.Files.ToList();
        }

        /// <summary>
        /// Uploads product images to Azure Blob Storage
        /// </summary>
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
    }
}
