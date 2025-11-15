using CategoryService.Api.Interfaces;
using CategoryService.Api.Interfaces.Models;
using CategoryService.Api.Interfaces.Repositories;
using CategoryService.Api.Repositories;
using CourseService.Api.Services;
using System.Collections;

namespace CategoryService.Api.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _dbContext;
        private Hashtable _repositories;
        private ICategoryRepository _categoryRepository;

        public UnitOfWork(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public ICategoryRepository Categories
        {
            get
            {
                return _categoryRepository ??= new CategoryRepository(_dbContext);
            }
        }

        // for BaseService
        public IBaseRepository<T> Repository<T>() where T : class, IModelWithNameAndId
        {
            if (_repositories == null)
                _repositories = new Hashtable();

            var type = typeof(T).Name;

            if (!_repositories.ContainsKey(type))
            {
                var repositoryType = typeof(BaseRepository<>);
                var repositoryInstance = Activator.CreateInstance(
                    repositoryType.MakeGenericType(typeof(T)), _dbContext);

                _repositories.Add(type, repositoryInstance);
            }

            return (IBaseRepository<T>)_repositories[type];
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _dbContext.SaveChangesAsync(cancellationToken);
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }
    }
}
