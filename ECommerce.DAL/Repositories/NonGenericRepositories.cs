using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ECommerce.DAL.Context;
using ECommerce.DAL.Repositories.Generic;
using ECommerce.DAL.Repositories.Interfaces;
using ECommerce.Common.Result;
using ECommerce.DAL.Entities;

namespace ECommerce.DAL.Repositories
{
    /// <summary>
    /// Product Repository (Non-Generic)
    /// Why: contains product-specific queries that the GenericRepository can't handle
    /// (like filtering, searching, pagination with includes)
    /// </summary>
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Result<PaginatedResult<Product>>> GetAllFilteredAsync(
            int? categoryId,
            string? name,
            int pageNumber,
            int pageSize)
        {
            IQueryable<Product> query = _context.Products
                .Include(p => p.Category);

            // Filter by CategoryId if provided
            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            // Search by name if provided
            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(p => p.Name.Contains(name));

            // Pagination
            var totalCount = await query.CountAsync();
            var items = await query
                .OrderBy(p => p.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new PaginatedResult<Product>
            {
                Items = items,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return Result<PaginatedResult<Product>>.Success(result);
        }

        public async Task<Result<Product>> GetWithCategoryAsync(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return Result<Product>.Failure("Product not found.");

            return Result<Product>.Success(product);
        }
    }

    /// <summary>
    /// Cart Repository (Non-Generic)
    /// </summary>
    public class CartRepository : GenericRepository<Cart>, ICartRepository
    {
        private readonly ApplicationDbContext _context;

        public CartRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Result<IReadOnlyList<Cart>>> GetUserCartAsync(string userId)
        {
            var cartItems = await _context.Carts
                .Include(c => c.Product)
                    .ThenInclude(p => p.Category)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            return Result<IReadOnlyList<Cart>>.Success(cartItems);
        }

        public async Task<Result<Cart>> GetByUserIdAndProductIdAsync(string userId, int productId)
        {
            var cartItem = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);

            if (cartItem == null)
                return Result<Cart>.Failure("Cart item not found.");

            return Result<Cart>.Success(cartItem);
        }

        public async Task<Result> ClearUserCartAsync(string userId)
        {
            var cartItems = await _context.Carts
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (cartItems.Any())
            {
                _context.Carts.RemoveRange(cartItems);
            }

            return await Task.FromResult(Result.Success("Cart cleared successfully."));
        }
    }

    /// <summary>
    /// Order Repository (Non-Generic)
    /// </summary>
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        private readonly ApplicationDbContext _context;

        public OrderRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Result<IReadOnlyList<Order>>> GetUserOrdersAsync(string userId)
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return Result<IReadOnlyList<Order>>.Success(orders);
        }

        public async Task<Result<Order>> GetOrderWithDetailsAsync(string userId, int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null)
                return Result<Order>.Failure("Order not found.");

            return Result<Order>.Success(order);
        }
    }

    /// <summary>
    /// Category Repository (Non-Generic)
    /// </summary>
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Result<Category>> GetWithProductsAsync(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return Result<Category>.Failure("Category not found.");

            return Result<Category>.Success(category);
        }
    }
}
