using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace kavyasCreation.Areas.Admin.Pages.Products
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _environment;

        public EditModel(IUnitOfWork unitOfWork, IWebHostEnvironment environment)
        {
            _unitOfWork = unitOfWork;
            _environment = environment;
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

            var uploads = GetUploads();
            var (orderMap, newPositions) = ParseImageOrder(ImageOrder);

            if (uploads.Any())
            {
                var images = await SaveImagesAsync(uploads, newPositions);
                await _unitOfWork.Products.AddImagesAsync(Product.Id, images);
            }

            if (orderMap.Count > 0)
            {
                await _unitOfWork.Products.UpdateImageOrderAsync(Product.Id, orderMap);
            }

            await _unitOfWork.Products.UpdateAsync(Product);
            return RedirectToPage("/Products/Index", new { area = "Admin" });
        }

        public async Task<IActionResult> OnPostDeleteImageAsync(Guid id, Guid imageId)
        {
            var image = await _unitOfWork.Products.GetImageByIdAsync(imageId);
            if (image is not null)
            {
                DeleteFile(image.Url);
                await _unitOfWork.Products.RemoveImageAsync(imageId);
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

        private async Task<List<ProductImage>> SaveImagesAsync(IEnumerable<IFormFile> files, IReadOnlyList<int>? sortOrders = null)
        {
            var uploadsRoot = Path.Combine(_environment.WebRootPath, "uploads", "products");
            Directory.CreateDirectory(uploadsRoot);
            var images = new List<ProductImage>();

            var index = 0;

            foreach (var file in files)
            {
                if (file.Length == 0)
                {
                    continue;
                }

                var extension = Path.GetExtension(file.FileName);
                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsRoot, fileName);

                await using var stream = System.IO.File.Create(filePath);
                await file.CopyToAsync(stream);

                images.Add(new ProductImage
                {
                    Id = Guid.NewGuid(),
                    Url = $"/uploads/products/{fileName}",
                    SortOrder = sortOrders is not null && index < sortOrders.Count ? sortOrders[index] : -1
                });
                index++;
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

        private void DeleteFile(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return;
            }

            if (url.StartsWith("/uploads/products", StringComparison.OrdinalIgnoreCase))
            {
                var relative = url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                var path = Path.Combine(_environment.WebRootPath, relative);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
            }
        }
    }
}
