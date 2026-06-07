using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ECommerce.API.Helpers;
using ECommerce.BLL.Interfaces;
using ECommerce.Common.Constants;
using ECommerce.Common.Result;

namespace ECommerce.API.Controllers
{
    /// <summary>
    /// CategoriesController: CRUD Operations for Categories
    /// 
    /// Endpoints:
    /// GET    /api/categories       - Get all categories
    /// GET    /api/categories/{id}  - Get category by ID
    /// POST   /api/categories       - Create category (Admin only)
    /// PUT    /api/categories/{id}  - Update category (Admin only)
    /// DELETE /api/categories/{id}  - Delete category (Admin only)
    /// POST   /api/categories/{id}/image - Upload category image (Admin only)
    /// </summary>
    [Authorize]
    public class CategoriesController : BaseController
    {
        private readonly ICategoryService _categoryService;
        private readonly IImageService _imageService;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(
            ICategoryService categoryService,
            IImageService imageService,
            ILogger<CategoriesController> logger)
        {
            _categoryService = categoryService;
            _imageService = imageService;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _categoryService.GetAllAsync();
            return HandleResult(result);
        }

        [AllowAnonymous]
        [HttpGet("{id}", Name = "GetCategoryById")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _categoryService.GetByIdAsync(id);
            return HandleResult(result);
        }

        [Authorize(Policy = PolicyNames.AdminOnly)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CategoryDto dto)
        {
            var result = await _categoryService.CreateAsync(dto);
            return HandleCreated(result, "GetCategoryById", new { id = result.Data?.Id });
        }

        [Authorize(Policy = PolicyNames.AdminOnly)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CategoryDto dto)
        {
            var result = await _categoryService.UpdateAsync(id, dto);
            return HandleResult(result);
        }

        [Authorize(Policy = PolicyNames.AdminOnly)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _categoryService.DeleteAsync(id);
            return HandleResult(result);
        }

        [Authorize(Policy = PolicyNames.AdminOnly)]
        [HttpPost("{id}/image")]
        public async Task<IActionResult> UploadImage(int id, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new Result { IsSuccess = false, Message = "No file uploaded." });

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return BadRequest(new Result { IsSuccess = false, Message = "Invalid file type. Allowed: .jpg, .jpeg, .png, .gif" });

            using var stream = file.OpenReadStream();
            var uploadResult = await _imageService.UploadImageAsync(stream, file.FileName);

            if (uploadResult.IsFailure)
                return BadRequest(uploadResult);

            // Update category with new image URL
            var categoryResult = await _categoryService.GetByIdAsync(id);
            if (categoryResult.IsFailure)
                return HandleNotFound("Category not found.");

            categoryResult.Data!.ImageUrl = uploadResult.Data;
            var updateResult = await _categoryService.UpdateAsync(id, new CategoryDto
            {
                Name = categoryResult.Data.Name,
                Description = categoryResult.Data.Description,
                ImageUrl = uploadResult.Data
            });

            return HandleResult(updateResult);
        }
    }
}
