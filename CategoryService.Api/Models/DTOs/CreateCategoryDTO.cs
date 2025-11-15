namespace CategoryService.Api.Models.DTOs
{
    public class CreateCategoryDTO
    {
        public string Name { get; set; }
        public IFormFile Image { get; set; }
    }
}
