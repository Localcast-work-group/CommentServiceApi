using CategoryService.Api.Exceptions.BusinessRuleValidation;
using CategoryService.Api.Interfaces;
using CategoryService.Api.Interfaces.Models;
using CategoryService.Api.Interfaces.Repositories;
using CategoryService.Api.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace CategoryService.Api.Services
{
    public class BaseService<T> : IBaseService<T> where T : class, IModelWithNameAndId
    {
        private readonly IBaseRepository<T> _baseRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly Serilog.ILogger _logger;

        public BaseService( Serilog.ILogger logger,IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _baseRepository = _unitOfWork.Repository<T>();
            _logger = logger;

        }
        public async Task<Guid> Add(T model)
        {
            if (!await _baseRepository.IsNameUniqueAsync(model.Name))
            {
                _logger.Warning("Attempted to add a {ModelType} with a duplicate name: {ModelName}", typeof(T).Name, model.Name);
                throw new DuplicateFieldException($"Name '{model.Name}' is already taken.", "Name");
            }
            await _baseRepository.AddAsync(model);
            await _unitOfWork.SaveChangesAsync();
            return model.Id;
        }

        public async Task Delete(Guid id)
        {
            await _baseRepository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }
        public async Task<List<T>> GetAll()
        {
            IQueryable<T> query = await _baseRepository.GetAllAsync();
            return await query.ToListAsync();
        }

        public async Task<T?> GetById(Guid id)
        {
            return await _baseRepository.GetByIdAsync(id);
        }
        public async Task<T?> GetByName(string name)
        {
            return await _baseRepository.GetByNameAsync(name);
        }
        public async Task<Guid> Update(T model)
        {
            await _baseRepository.UpdateAsync(model);
            await _unitOfWork.SaveChangesAsync();
            return model.Id;
        }
        public bool IsNameUnique(string name)
        {
            return _baseRepository.IsNameUnique(name);
        }
        public async Task<bool> IsNameUniqueAsync(string name)
        {
            return await _baseRepository.IsNameUniqueAsync(name);
        }
    }
}
