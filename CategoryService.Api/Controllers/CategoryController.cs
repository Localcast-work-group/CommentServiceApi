using CategoryService.Api.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CategoryService.Api.Models;
using CategoryService.Api.Extensions;
using CategoryService.Api.Models.DTOs;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Attributes;

namespace CategoryService.Api.Controllers
{
    [ApiController]
    [AutoValidation]
    [Route("api/[controller]")]


    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly IWebHostEnvironment _environment;

        public CategoryController(ICategoryService categoryService, IWebHostEnvironment environment)
        {
            _categoryService = categoryService;
            _environment = environment;
        }
        [HttpGet("{id}", Name = "GetCategory")]
        public async Task<IActionResult> Get([FromRoute] Guid Id)
        {
            GetCategoryDTO? category = await _categoryService.GetCategory(Id);
            if (category == null)
            {
                return NotFound("Category is null");
            }

            return Ok(category);
        }
        [HttpGet(Name = "GetCategories")]
        public async Task<IActionResult> GetCategories()
        {
            List<GetCategoryDTO> categories = await _categoryService.GetCategories();

            return Ok(categories);
        }
        [HttpGet("GetImage/{id}", Name = "GetCategoryImage")]
        public async Task<IActionResult> GetImage(Guid Id)
        {

            Category Category = await _categoryService.GetById(Id);
            if (Category == null)
            {
                return NotFound("Category is null");
            }
            var fileInfo = new FileInfo(Category.ImagePath);
            return File(
                new FileStream(Category.ImagePath, FileMode.Open, FileAccess.Read, FileShare.Read),
                 MimeType.GetMimeType(fileInfo.Extension),
                fileInfo.Name
            );
        }
        [HttpPost]
        [Authorize(Roles ="Admin") ]
        public async Task<IActionResult> Post(CreateCategoryDTO category)
        {
            if (category == null || category.Image == null)
            {
                return BadRequest("Invalid category data");
            }

            Guid categoryId = Guid.NewGuid();

            string uploadsFolder = Path.Combine("CategoriesCovers");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }
            
            string fileName = $"{categoryId}{Path.GetExtension(category.Image.FileName)}";
            string filePath = Path.Combine(uploadsFolder, fileName);
            using (FileStream stream = new FileStream(filePath, FileMode.Create))
            {
                category.Image.CopyTo(stream);
            }
            Category newCategory= new Category
            {
                Id = categoryId,
                Name = category.Name,
                ImagePath = filePath,
            };

            await _categoryService.Add(newCategory);
            return Created(
                );
        }

    }
}
