using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class UserDto
    {

        public Guid UserId { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Name must be between 2 and 100 characters.", MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; } = string.Empty;

        // [Required]
        // [StringLength(255, ErrorMessage = "Password must be between 6 and 255 characters.", MinimumLength = 6)]
        // public string Password { get; set; } = string.Empty; // Note: Include cautiously

        [StringLength(255)]
        public string Address { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public bool IsAdmin { get; set; } = false;// Include only if necessary for the context
        public bool IsBanned { get; set; } = false;// Include only if necessary for the context
        public DateTime CreatedAt { get; set; }

        public List<OrderModel> Orders { get; set; } = new List<OrderModel>();  // One-to-many with Order

    }
}