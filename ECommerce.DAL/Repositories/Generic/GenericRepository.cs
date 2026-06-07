using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ECommerce.DAL.Context;
using ECommerce.Common.Result;
using ECommerce.DAL.Repositories.Generic;

namespace ECommerce.DAL.Repositories
{
    /// <summary>
    /// GenericRepository: Implements IGenericRepository<T>
    /// Why: provides default CRUD implementations for any entity type T
    /// </summary>
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<Result<T>> GetByIdAsync(int id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity == null)
                return Result<T>.Failure("Entity not found.");

            return Result<T>.Success(entity);
        }

        public async Task<Result<IReadOnlyList<T>>> GetAllAsync()
        {
            var entities = await _dbSet.ToListAsync();
            return Result<IReadOnlyList<T>>.Success(entities);
        }

        public async Task<Result<IReadOnlyList<T>>> FindAsync(Expression<Func<T, bool>> criteria, string[]? includes = null)
        {
            IQueryable<T> query = _dbSet;

            if (includes != null)
                foreach (var include in includes)
                    query = query.Include(include);

            var result = await query.Where(criteria).ToListAsync();
            return Result<IReadOnlyList<T>>.Success(result);
        }

        public async Task<Result<T>> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            return Result<T>.Success(entity);
        }

        public async Task<Result<T>> UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            return await Task.FromResult(Result<T>.Success(entity));
        }

        public async Task<Result> DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            return await Task.FromResult(Result.Success("Deleted successfully."));
        }

        // Pagination helper
        public async Task<Result<PaginatedResult<T>>> GetAllPaginatedAsync(int pageNumber, int pageSize, Expression<Func<T, bool>>? filter = null)
        {
            IQueryable<T> query = _dbSet;

            if (filter != null)
                query = query.Where(filter);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new PaginatedResult<T>
            {
                Items = items,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return Result<PaginatedResult<T>>.Success(result);
        }
    }
}
