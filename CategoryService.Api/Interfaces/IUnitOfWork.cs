using CategoryService.Api.Interfaces.Models;
using CategoryService.Api.Interfaces.Repositories;

namespace CategoryService.Api.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        ICategoryRepository Categories { get; }
        IBaseRepository<T> Repository<T>() where T : class, IModelWithNameAndId;
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
