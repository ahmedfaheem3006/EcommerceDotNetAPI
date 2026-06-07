using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AutoMapper;
using ECommerce.BLL.Interfaces;
using ECommerce.Common.Result;
using ECommerce.DAL.Entities;

namespace ECommerce.BLL.Services
{
    /// <summary>
    /// Product Service: Business Logic for Product CRUD + Search + Pagination
    /// 
    /// لماذا: نحتوي الـ Business Logic الخاصة بالمنتجات هنا:
    /// - Filtering by Category
    /// - Searching by Name
    /// - Pagination
    /// - Image upload handling
    /// - Stock quantity validation
    /// </summary>
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IImageService _imageService;

        public ProductService(IUnitOfWork unitOfWork, IMapper mapper, IImageService imageService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _imageService = imageService;
        }

        public async Task<Result<PaginatedResult<Product>>> GetAllAsync(
            int? categoryId, string? name, int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            return await _unitOfWork.Products.GetAllFilteredAsync(categoryId, name, pageNumber, pageSize);
        }

        public async Task<Result<Product>> GetByIdAsync(int id)
        {
            return await _unitOfWork.Products.GetWithCategoryAsync(id);
        }

        public async Task<Result<Product>> CreateAsync(ProductDto dto)
        {
            // Validate category exists
            var categoryResult = await _unitOfWork.Categories.GetByIdAsync(dto.CategoryId);
            if (categoryResult.IsFailure)
                return Result<Product>.Failure("Category not found");

            // Validate price
            if (dto.Price <= 0)
                return Result<Product>.Failure("Price must be greater than zero");

            // Validate stock
            if (dto.StockQuantity < 0)
                return Result<Product>.Failure("Stock quantity cannot be negative");

            var product = _mapper.Map<Product>(dto);
            var result = await _unitOfWork.Products.AddAsync(product);

            if (result.IsFailure)
                return Result<Product>.Failure("Failed to create product");

            await _unitOfWork.SaveChangesAsync();
            return Result<Product>.Success(result.Data!, "Product created successfully");
        }

        public async Task<Result<Product>> UpdateAsync(int id, ProductDto dto)
        {
            var existingResult = await _unitOfWork.Products.GetByIdAsync(id);
            if (existingResult.IsFailure)
                return Result<Product>.Failure("Product not found");

            var existing = existingResult.Data!;

            // Validate category exists
            var categoryResult = await _unitOfWork.Categories.GetByIdAsync(dto.CategoryId);
            if (categoryResult.IsFailure)
                return Result<Product>.Failure("Category not found");

            existing.Name = dto.Name;
            existing.Description = dto.Description;
            existing.Price = dto.Price;
            existing.StockQuantity = dto.StockQuantity;
            existing.CategoryId = dto.CategoryId;
            existing.ImageUrl = dto.ImageUrl;
            existing.UpdatedAt = DateTime.UtcNow;

            var updateResult = await _unitOfWork.Products.UpdateAsync(existing);
            if (updateResult.IsFailure)
                return Result<Product>.Failure("Failed to update product");

            await _unitOfWork.SaveChangesAsync();
            return Result<Product>.Success(updateResult.Data!, "Product updated successfully");
        }

        public async Task<Result> DeleteAsync(int id)
        {
            var existingResult = await _unitOfWork.Products.GetByIdAsync(id);
            if (existingResult.IsFailure)
                return Result.Failure("Product not found");

            // Check if product is in any cart
            var cartResult = await _unitOfWork.Carts.FindAsync(c => c.ProductId == id);
            if (cartResult.IsSuccess && cartResult.Data!.Count > 0)
                return Result.Failure("Cannot delete product that exists in a cart");

            var deleteResult = await _unitOfWork.Products.DeleteAsync(existingResult.Data!);
            if (deleteResult.IsFailure)
                return deleteResult;

            await _unitOfWork.SaveChangesAsync();
            return Result.Success("Product deleted successfully");
        }

        public async Task<Result> UploadImageAsync(int productId, string imageUrl)
        {
            var productResult = await _unitOfWork.Products.GetByIdAsync(productId);
            if (productResult.IsFailure)
                return Result.Failure("Product not found");

            var product = productResult.Data!;
            product.ImageUrl = imageUrl;
            product.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Products.UpdateAsync(product);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success("Product image uploaded successfully");
        }
    }
}
