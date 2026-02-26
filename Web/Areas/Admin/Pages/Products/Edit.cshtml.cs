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
    public class EditModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBlobStorageService _blobStorageService;
        private readonly ILogger<EditModel> _logger;

        public EditModel(IUnitOfWork unitOfWork, IBlobStorageService blobStorageService, ILogger<EditModel> logger)
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
        public string? ImageOrder { get; set; }
        public IReadOnlyList<Category> Categories { get; private set; } = [];

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            Categories = await _unitOfWork.Categories.ListAsync();
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product is null)
            {
                return RedirectToPage("/Products/Index", new { area = "Admin" });
            }

            Product = product;
            return Page();
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

            try
            {
                var uploads = GetUploads();
                var (orderMap, newPositions) = ParseImageOrder(ImageOrder);

                if (uploads.Any())
                {
                    var images = await SaveImagesToAzureAsync(uploads, newPositions);
                    await _unitOfWork.Products.AddImagesAsync(Product.Id, images);
                }

                if (orderMap.Count > 0)
                {
                    await _unitOfWork.Products.UpdateImageOrderAsync(Product.Id, orderMap);
                }

                await _unitOfWork.Products.UpdateAsync(Product);
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

        public async Task<IActionResult> OnPostDeleteImageAsync(Guid id, Guid imageId)
        {
            try
            {
                var image = await _unitOfWork.Products.GetImageByIdAsync(imageId);
                if (image is not null)
                {
                    await _blobStorageService.DeleteAsync(image.Url, "products");
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

            return Request.Form.Files.ToList();
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
                    var imageUrl = await _blobStorageService.UploadAsync(file, "products");
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
