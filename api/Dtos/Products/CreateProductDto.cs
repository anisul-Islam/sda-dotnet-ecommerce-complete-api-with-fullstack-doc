using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace api.Models
{
    public class CreateProductDto
    {
        [Required]
        [StringLength(100, ErrorMessage = "Name must be between 2 and 100 characters.", MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        // [Url(ErrorMessage = "Image must be a valid URL.")]
        public string Image { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description can't be longer than 1000 characters.")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 10000.00, ErrorMessage = "Price must be between 0.01 and 10000.00.")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be a non-negative number.")]
        public int Quantity { get; set; } = 0;

        [Range(0, int.MaxValue, ErrorMessage = "Sold must be a non-negative number.")]
        public int Sold { get; set; } = 0;

        [Range(0, 1000.00, ErrorMessage = "Shipping must be between 0 and 1000.00.")]
        public decimal Shipping { get; set; } = 0;

        // public int OrderId { get; set; }  // Foreign key

        // public OrderModel Order { get; set; } = new OrderModel();  // Navigation property

        [Required]
        [MinLength(1, ErrorMessage = "CategoryIds must contain at least one category.")]
        public List<Guid> CategoryIds { get; set; } = new List<Guid>();  // Many-to-many with CategoryMany-to-many with Category


    }
}
