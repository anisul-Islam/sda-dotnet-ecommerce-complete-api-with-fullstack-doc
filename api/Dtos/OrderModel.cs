using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{


    public class OrderModel
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;  // Default status
        public int UserId { get; set; }  // Foreign key
        public UserDto User { get; set; } = new UserDto();  // Navigation property
        public List<ProductDto> Products { get; set; } = new List<ProductDto>();  // One-to-many with Product
    }
}