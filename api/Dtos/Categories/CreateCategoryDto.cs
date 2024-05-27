using System.ComponentModel.DataAnnotations;


namespace api.Models
{
    public class CreateCategoryDto
    {

        [Required]
        [StringLength(100, ErrorMessage = "Name must be between 2 and 200 characters.", MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

}