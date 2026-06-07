using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ECommerce.Common.Result;
using ECommerce.DAL.Entities;

using ECommerce.DAL.Repositories.Generic;

namespace ECommerce.DAL.Repositories.Interfaces
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<Result<PaginatedResult<Product>>> GetAllFilteredAsync(
            int? categoryId,
            string? name,
            int pageNumber,
            int pageSize);
        Task<Result<Product>> GetWithCategoryAsync(int id);
    }

    public interface ICartRepository : IGenericRepository<Cart>
    {
        Task<Result<IReadOnlyList<Cart>>> GetUserCartAsync(string userId);
        Task<Result<Cart>> GetByUserIdAndProductIdAsync(string userId, int productId);
        Task<Result> ClearUserCartAsync(string userId);
    }

    public interface IOrderRepository : IGenericRepository<Order>
    {
        Task<Result<IReadOnlyList<Order>>> GetUserOrdersAsync(string userId);
        Task<Result<Order>> GetOrderWithDetailsAsync(string userId, int orderId);
    }

    public interface ICategoryRepository : IGenericRepository<Category>
    {
        Task<Result<Category>> GetWithProductsAsync(int id);
    }
}
