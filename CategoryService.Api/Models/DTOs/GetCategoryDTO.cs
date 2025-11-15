namespace CategoryService.Api.Models.DTOs
{
    public class GetCategoryDTO
    {
        public Guid Id { get; set; }
        public string CategoryName { get; set; }
        public int VideoCount { get; set; }
    }
}
