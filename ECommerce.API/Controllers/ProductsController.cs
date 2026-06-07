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
    /// ProductsController: CRUD + Search + Filtering + Pagination for Products
    /// 
    /// Endpoints:
    /// GET    /api/products                         - Get all products (with filtering, search, pagination)
    /// GET    /api/products/{id}                    - Get product details
    /// POST   /api/products                         - Create product (Admin only)
    /// PUT    /api/products/{id}                    - Update product (Admin only)
    /// DELETE /api/products/{id}                    - Delete product (Admin only)
    /// POST   /api/products/{id}/image              - Upload product image (Admin only)
    /// 
    /// Query Parameters for GET:
    /// categoryId  - Filter by category (optional)
    /// name        - Search by name (optional)
    /// pageNumber  - Page number (default: 1)
    /// pageSize    - Items per page (default: 20, max: 100)
    /// </summary>
    [Authorize]
    public class ProductsController : BaseController
    {
        private readonly IProductService _productService;
        private readonly IImageService _imageService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(
            IProductService productService,
            IImageService imageService,
            ILogger<ProductsController> logger)
        {
            _productService = productService;
            _imageService = imageService;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? categoryId,
            [FromQuery] string? name,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _productService.GetAllAsync(categoryId, name, pageNumber, pageSize);
            return HandleResult(result);
        }

        [AllowAnonymous]
        [HttpGet("{id}", Name = "GetProductById")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _productService.GetByIdAsync(id);
            return HandleResult(result);
        }

        [Authorize(Policy = PolicyNames.AdminOnly)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductDto dto)
        {
            var result = await _productService.CreateAsync(dto);
            return HandleCreated(result, "GetProductById", new { id = result.Data?.Id });
        }

        [Authorize(Policy = PolicyNames.AdminOnly)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductDto dto)
        {
            var result = await _productService.UpdateAsync(id, dto);
            return HandleResult(result);
        }

        [Authorize(Policy = PolicyNames.AdminOnly)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _productService.DeleteAsync(id);
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
                return BadRequest(new Result { IsSuccess = false, Message = "Invalid file type." });

            using var stream = file.OpenReadStream();
            var uploadResult = await _imageService.UploadImageAsync(stream, file.FileName);

            if (uploadResult.IsFailure)
                return BadRequest(uploadResult);

            var result = await _productService.UploadImageAsync(id, uploadResult.Data!);
            return HandleResult(result);
        }
    }
}
