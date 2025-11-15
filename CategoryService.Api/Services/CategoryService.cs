using CategoryService.Api.Data;
using CategoryService.Api.Interfaces;
using CategoryService.Api.Interfaces.Services;
using CategoryService.Api.Models;
using CategoryService.Api.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CategoryService.Api.Services
{
    public class CategoryService : BaseService<Category>, ICategoryService
    {
        Serilog.ILogger Logger { get; set; }
        private readonly IUnitOfWork _unitOfWork;


        public CategoryService(IUnitOfWork unitOfWork, Serilog.ILogger logger) : base(logger, unitOfWork)
        {
            Logger = logger;
            _unitOfWork = unitOfWork;
        }
        public  async Task<List<GetCategoryDTO>> GetCategories()
        {
            IQueryable<Category> query = await _unitOfWork.Categories.GetAllAsync();

            return await query.Select(x => new GetCategoryDTO
            {
                CategoryName = x.Name,
                Id = x.Id,
                VideoCount = x.VideoCount
            }).ToListAsync();
        }
        public async Task<GetCategoryDTO?> GetCategory(Guid id)
        {
            Category? category = await _unitOfWork.Categories.GetByIdAsync(id);
            if(category == null)
            {
                return null;
            }

            return new GetCategoryDTO
            {
                CategoryName = category.Name,
                Id = category.Id,
                VideoCount = category.VideoCount
            };
        }
        public async Task Seed()
        {
            IQueryable<Category> categories = await _unitOfWork.Categories.GetAllAsync();
            if (await categories.AnyAsync()) 
            {
                return;
            }
            Category category = new Category
            {
                Name = "Others",
                VideoCount = 0,
                ImagePath = ""
            };
            await Add(category);
        }

        public async Task IncrementVideoCount(Guid id)
        {
            Category category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category == null)
            {
                throw new KeyNotFoundException($"Category with id {id} not found.");
            }
            Logger.Information("Incrementing video count for category {CategoryId}", id);
            category.VideoCount += 1;
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
