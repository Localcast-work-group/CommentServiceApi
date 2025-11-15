using CategoryService.Api.Interfaces.Repositories;
using CategoryService.Api.Models;
using CategoryService.Api.Data;
using CategoryService.Api.Repositories;

namespace CourseService.Api.Services
{
    public class CategoryRepository : BaseRepository<Category>, ICategoryRepository
    {
        private readonly ApplicationDbContext ApplicationDbContext;
        public CategoryRepository(ApplicationDbContext applicationDbContext) : base(applicationDbContext) 
        {
            ApplicationDbContext = applicationDbContext;
        }

        
    }
}
