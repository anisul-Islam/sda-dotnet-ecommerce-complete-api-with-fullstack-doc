using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace api.EntityFrameworkCore
{
    public class ProductWithDataAnnotation
    {
        [Table("products")]
        public class Product
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int ProductId { get; set; }

            [Required]
            [StringLength(100)]
            public string Name { get; set; } = string.Empty;

            [Required]
            [StringLength(100)]
            public string Slug { get; set; } = string.Empty;

            [StringLength(100)]
            public string Image { get; set; } = string.Empty;

            public string Description { get; set; } = string.Empty;

            [Required]
            [Column(TypeName = "decimal(10,2)")]
            public decimal Price { get; set; }

            [Range(0, int.MaxValue)]
            public int Quantity { get; set; } = 0;

            [Range(0, int.MaxValue)]
            public int Sold { get; set; } = 0;

            [Column(TypeName = "decimal(10,2)")]
            public decimal Shipping { get; set; } = 0;

            public int OrderId { get; set; }  // Foreign key
            public Order Order { get; set; } = new Order(); // Navigation property
            public List<Category> Categories { get; set; } = new List<Category>();  // Many-to-many with Category

            [Column("createdAt", TypeName = "timestamp with time zone")]
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        }


    }
}