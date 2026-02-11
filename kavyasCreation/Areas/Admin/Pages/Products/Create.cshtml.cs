using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace kavyasCreation.Areas.Admin.Pages.Products
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _environment;

        public CreateModel(IUnitOfWork unitOfWork, IWebHostEnvironment environment)
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
            if (uploads.Any())
            {
                var images = await SaveImagesAsync(uploads);
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
            return RedirectToPage("/Products/Index", new { area = "Admin" });
        }

        private List<IFormFile> GetUploads()
        {
            if (ImageUploads.Any())
            {
                return ImageUploads;
            }

            return Request.Form.Files.ToList();
        }

        private async Task<List<ProductImage>> SaveImagesAsync(IEnumerable<IFormFile> files)
        {
            var uploadsRoot = Path.Combine(_environment.WebRootPath, "uploads", "products");
            Directory.CreateDirectory(uploadsRoot);
            var images = new List<ProductImage>();

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
                    Url = $"/uploads/products/{fileName}"
                });
            }

            return images;
        }
    }
}
