using System.ComponentModel.DataAnnotations.Schema;

namespace api.EntityFrameworkCore
{
    [Table("Products")]
    public class Product
    {
        public Guid ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; } = 0;
        public int Sold { get; set; } = 0;
        public decimal Shipping { get; set; } = 0;
        // public int OrderId { get; set; }  // Foreign key
        // public Order Order { get; set; } = new Order();  // Navigation property
        public List<Category> Categories { get; set; } = new List<Category>();  // Many-to-many with Category

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}