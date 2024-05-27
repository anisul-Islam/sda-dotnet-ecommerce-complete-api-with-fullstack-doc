
using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Users
{
    public class ResetPasswordDto
    {
        [Required]
        public string NewPassword { get; set; } = string.Empty;
    }
}