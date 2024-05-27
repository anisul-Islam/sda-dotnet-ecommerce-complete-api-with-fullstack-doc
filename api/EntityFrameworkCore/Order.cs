using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using ECommerceAPI.Models;

namespace api.EntityFrameworkCore
{
    [Table("Orders")]
    public class Order
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;  // Default status
        public int UserId { get; set; }  // Foreign key
        public User User { get; set; } = new User();  // Navigation property
        public List<Product> Products { get; set; } = new List<Product>();  // One-to-many with Product
    }
}
