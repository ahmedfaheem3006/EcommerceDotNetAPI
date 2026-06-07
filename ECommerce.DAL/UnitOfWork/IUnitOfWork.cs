using System.Threading.Tasks;
using ECommerce.DAL.Repositories.Interfaces;

namespace ECommerce.DAL.UnitOfWork
{
    /// <summary>
    /// Unit of Work Pattern
    /// لماذا: نضمن إن كل العمليات اللي بتحصل في الـ Request الواحد تتنفذ كـ Transaction واحدة
    /// لو أي حاجة فشلت، كل الـ Changes ترجع زي ما كانت (Rollback)
    /// وبنضمن إن الـ DbContext واحد لكل الـ Repositories في نفس الـ Request
    /// </summary>
    public interface IUnitOfWork
    {
        // Non-Generic Repositories
        IProductRepository Products { get; }
        ICartRepository Carts { get; }
        IOrderRepository Orders { get; }
        ICategoryRepository Categories { get; }

        // Save all changes in one transaction
        Task<int> SaveChangesAsync();

        // Begin a transaction manually (if needed)
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
