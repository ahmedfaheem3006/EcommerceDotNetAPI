using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ECommerce.DAL.Context;
using ECommerce.DAL.Repositories;
using ECommerce.DAL.Repositories.Interfaces;
using ECommerce.DAL.UnitOfWork;

namespace ECommerce.DAL.Repositories
{
    /// <summary>
    /// UnitOfWork Implementation
    /// Why: ensures all database operations within a single request share the same
    /// DbContext instance and are committed/rolled back atomically
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;

        // Lazy initialization for non-generic repositories
        private IProductRepository? _products;
        private ICartRepository? _carts;
        private IOrderRepository? _orders;
        private ICategoryRepository? _categories;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IProductRepository Products =>
            _products ??= new ProductRepository(_context);

        public ICartRepository Carts =>
            _carts ??= new CartRepository(_context);

        public IOrderRepository Orders =>
            _orders ??= new OrderRepository(_context);

        public ICategoryRepository Categories =>
            _categories ??= new CategoryRepository(_context);

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }
}
