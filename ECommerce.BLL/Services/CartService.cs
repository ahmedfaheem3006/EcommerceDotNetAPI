using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ECommerce.BLL.Interfaces;
using ECommerce.DAL.Entities;

namespace ECommerce.BLL.Services
{
    /// <summary>
    /// Cart Service: Business Logic for Cart Management
    /// 
    /// لماذا: إدارة السلة تتضمن:
    /// - إضافة منتج للسلة (أو زيادة الكمية لو موجود بالفعل)
    /// - حذف منتج من السلة
    /// - تحديث الكمية
    /// - عرض محتوى السلة
    /// - التحقق من المخزون قبل الإضافة
    /// </summary>
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CartService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<IReadOnlyList<CartDto>>> GetUserCartAsync(string userId)
        {
            var cartResult = await _unitOfWork.Carts.GetUserCartAsync(userId);
            if (cartResult.IsFailure)
                return Result<IReadOnlyList<CartDto>>.Failure(cartResult.Message!);

            var cartDtos = cartResult.Data!.Select(item => new CartDto
            {
                ProductId = item.ProductId,
                ProductName = item.Product.Name,
                CategoryName = item.Product.Category.Name,
                Price = item.Product.Price,
                Quantity = item.Quantity,
                ImageUrl = item.Product.ImageUrl
            }).ToList();

            return Result<IReadOnlyList<CartDto>>.Success(cartDtos);
        }

        public async Task<Result<CartDto>> AddToCartAsync(string userId, CartAddRequest request)
        {
            // Validate product exists and has enough stock
            var productResult = await _unitOfWork.Products.GetByIdAsync(request.ProductId);
            if (productResult.IsFailure)
                return Result<CartDto>.Failure("Product not found");

            var product = productResult.Data!;

            if (product.StockQuantity < request.Quantity)
                return Result<CartDto>.Failure($"Only {product.StockQuantity} items available in stock");

            // Check if item already exists in cart
            var existingCartResult = await _unitOfWork.Carts.GetByUserIdAndProductIdAsync(userId, request.ProductId);

            if (existingCartResult.IsSuccess)
            {
                // Update quantity
                var existingCart = existingCartResult.Data!;
                existingCart.Quantity += request.Quantity;
                await _unitOfWork.Carts.UpdateAsync(existingCart);
                await _unitOfWork.SaveChangesAsync();

                return await GetUserCartItemDto(existingCart, product);
            }

            // Add new cart item
            var cartItem = new Cart
            {
                UserId = userId,
                ProductId = request.ProductId,
                Quantity = request.Quantity
            };

            await _unitOfWork.Carts.AddAsync(cartItem);
            await _unitOfWork.SaveChangesAsync();

            return await GetUserCartItemDto(cartItem, product);
        }

        public async Task<Result> RemoveFromCartAsync(string userId, int productId)
        {
            var cartItemResult = await _unitOfWork.Carts.GetByUserIdAndProductIdAsync(userId, productId);
            if (cartItemResult.IsFailure)
                return Result.Failure("Item not found in cart");

            await _unitOfWork.Carts.DeleteAsync(cartItemResult.Data!);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success("Item removed from cart");
        }

        public async Task<Result<CartDto>> UpdateQuantityAsync(string userId, int productId, int quantity)
        {
            if (quantity < 1)
                return Result<CartDto>.Failure("Quantity must be at least 1");

            var cartItemResult = await _unitOfWork.Carts.GetByUserIdAndProductIdAsync(userId, productId);
            if (cartItemResult.IsFailure)
                return Result<CartDto>.Failure("Item not found in cart");

            var productResult = await _unitOfWork.Products.GetByIdAsync(productId);
            if (productResult.IsFailure)
                return Result<CartDto>.Failure("Product not found");

            var product = productResult.Data!;
            if (product.StockQuantity < quantity)
                return Result<CartDto>.Failure($"Only {product.StockQuantity} items available");

            var cartItem = cartItemResult.Data!;
            cartItem.Quantity = quantity;

            await _unitOfWork.Carts.UpdateAsync(cartItem);
            await _unitOfWork.SaveChangesAsync();

            return await GetUserCartItemDto(cartItem, product);
        }

        public async Task<Result> ClearCartAsync(string userId)
        {
            return await _unitOfWork.Carts.ClearUserCartAsync(userId);
        }

        private async Task<Result<CartDto>> GetUserCartItemDto(Cart cartItem, Product product)
        {
            var categoryResult = await _unitOfWork.Categories.GetByIdAsync(product.CategoryId);
            var categoryName = categoryResult.IsSuccess ? categoryResult.Data!.Name : "N/A";

            var dto = new CartDto
            {
                ProductId = cartItem.ProductId,
                ProductName = product.Name,
                CategoryName = categoryName,
                Price = product.Price,
                Quantity = cartItem.Quantity,
                ImageUrl = product.ImageUrl
            };

            return Result<CartDto>.Success(dto, "Cart updated successfully");
        }
    }
}
