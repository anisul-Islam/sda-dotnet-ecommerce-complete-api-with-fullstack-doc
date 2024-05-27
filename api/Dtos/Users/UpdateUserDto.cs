using System.ComponentModel.DataAnnotations;

public class UpdateUserDto
{
    // Removed UserId and CreatedAt; these should not be modified or should be handled separately.

    [StringLength(100, ErrorMessage = "Name must be between 2 and 100 characters.", MinimumLength = 2)]
    public string? Name { get; set; }

    [StringLength(255, ErrorMessage = "Address length must not exceed 255 characters.")]
    public string? Address { get; set; }

    // [Url(ErrorMessage = "Image must be a valid URL.")]
    public string? Image { get; set; }

    // Include these only if updating through an admin interface or with proper authorization checks:
    public bool? IsAdmin { get; set; }
    public bool? IsBanned { get; set; }
}
