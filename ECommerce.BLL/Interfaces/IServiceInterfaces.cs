using System.Collections.Generic;
using System.Threading.Tasks;
using ECommerce.Common.Result;
using ECommerce.DAL.Entities;

namespace ECommerce.BLL.Interfaces
{
    /// <summary>
    /// IAuthService: Authentication & Authorization service contract
    /// </summary>
    public interface IAuthService
    {
        Task<Result<string>> RegisterAsync(RegisterRequest request);
        Task<Result<string>> LoginAsync(LoginRequest request);
        Task<Result<string>> GetRoleByEmailAsync(string email);
    }

    public class RegisterRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Address { get; set; }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// ICategoryService: CRUD for categories
    /// </summary>
    public interface ICategoryService
    {
        Task<Result<IReadOnlyList<Category>>> GetAllAsync();
        Task<Result<Category>> GetByIdAsync(int id);
        Task<Result<Category>> CreateAsync(CategoryDto dto);
        Task<Result<Category>> UpdateAsync(int id, CategoryDto dto);
        Task<Result> DeleteAsync(int id);
    }

    public class CategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
    }

    /// <summary>
    /// IProductService: CRUD + Filtering + Pagination for products
    /// </summary>
    public interface IProductService
    {
        Task<Result<PaginatedResult<Product>>> GetAllAsync(
            int? categoryId, string? name, int pageNumber, int pageSize);
        Task<Result<Product>> GetByIdAsync(int id);
        Task<Result<Product>> CreateAsync(ProductDto dto);
        Task<Result<Product>> UpdateAsync(int id, ProductDto dto);
        Task<Result> DeleteAsync(int id);
        Task<Result> UploadImageAsync(int productId, string imageUrl);
    }

    public class ProductDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int CategoryId { get; set; }
        public string? ImageUrl { get; set; }
    }

    /// <summary>
    /// ICartService: Cart management (User-specific)
    /// </summary>
    public interface ICartService
    {
        Task<Result<IReadOnlyList<CartDto>>> GetUserCartAsync(string userId);
        Task<Result<CartDto>> AddToCartAsync(string userId, CartAddRequest request);
        Task<Result> RemoveFromCartAsync(string userId, int productId);
        Task<Result<CartDto>> UpdateQuantityAsync(string userId, int productId, int quantity);
        Task<Result> ClearCartAsync(string userId);
    }

    public class CartDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Total => Price * Quantity;
        public string? ImageUrl { get; set; }
    }

    public class CartAddRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; } = 1;
    }

    /// <summary>
    /// IOrderService: Order processing
    /// </summary>
    public interface IOrderService
    {
        Task<Result<OrderDto>> PlaceOrderAsync(string userId, PlaceOrderRequest request);
        Task<Result<IReadOnlyList<OrderSummaryDto>>> GetUserOrdersAsync(string userId);
        Task<Result<OrderDetailsDto>> GetOrderDetailsAsync(string userId, int orderId);
    }

    public class PlaceOrderRequest
    {
        public string? ShippingAddress { get; set; }
        public string? PaymentMethod { get; set; }
    }

    public class OrderDto
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; }
    }

    public class OrderSummaryDto
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int ItemsCount { get; set; }
    }

    public class OrderDetailsDto
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ShippingAddress { get; set; }
        public string? PaymentMethod { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
    }

    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Subtotal => Quantity * Price;
    }

    /// <summary>
    /// IImageService: File upload management
    /// </summary>
    public interface IImageService
    {
        Task<Result<string>> UploadImageAsync(Stream fileStream, string fileName);
    }
}
