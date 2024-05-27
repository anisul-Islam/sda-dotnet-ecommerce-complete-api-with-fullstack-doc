namespace api.Models
{
    public class CategoryDto
    {
        public Guid CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<ProductDto> Products { get; set; } = new List<ProductDto>();  // Many-to-many with Product
        public DateTime CreatedAt { get; set; }
    }
}