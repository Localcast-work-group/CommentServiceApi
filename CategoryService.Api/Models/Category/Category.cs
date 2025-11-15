using CategoryService.Api.Interfaces.Models;
using System.ComponentModel.DataAnnotations;

namespace CategoryService.Api.Models
{
    public class Category : IModelWithNameAndId
    {
        public Guid Id { get; set; }
        [MaxLength(150)]
        public string Name { get; set; }
        [MaxLength(150)]
        public string ImagePath { get; set; }   
        public int VideoCount { get; set; }
    }
}
