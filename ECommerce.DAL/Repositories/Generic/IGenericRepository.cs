using ECommerce.Common.Result;
using System.Linq.Expressions;

namespace ECommerce.DAL.Repositories.Generic
{
    /// <summary>
    /// IGenericRepository: contract for common CRUD operations
    /// Why: we don't want to repeat GetById, GetAll, Add, Update, Delete for every entity
    /// </summary>
    public interface IGenericRepository<T> where T : class
    {
        Task<Result<T>> GetByIdAsync(int id);
        Task<Result<IReadOnlyList<T>>> GetAllAsync();
        Task<Result<IReadOnlyList<T>>> FindAsync(Expression<Func<T, bool>> criteria, string[]? includes = null);
        Task<Result<T>> AddAsync(T entity);
        Task<Result<T>> UpdateAsync(T entity);
        Task<Result> DeleteAsync(T entity);
    }
}
