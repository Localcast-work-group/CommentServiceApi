using CategoryService.Api.Models;
using CategoryService.Api.Models.DTOs;

namespace CategoryService.Api.Interfaces.Services
{
    public interface ICategoryService : IBaseService<Category>
    {
        Task<List<GetCategoryDTO>> GetCategories();
        Task<GetCategoryDTO?> GetCategory(Guid id);
        Task IncrementVideoCount(Guid id);
        Task Seed();
    }
}
