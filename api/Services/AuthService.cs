using System.IdentityModel.Tokens.Jwt; // Import for handling JWT token creation and management.
using System.Security.Claims; // Import for dealing with claims-based identity.
using System.Text; // Import for encoding utilities.
using api.Models; // Assuming your application's data transfer objects (DTOs) are located here.
using Microsoft.IdentityModel.Tokens; // Import for handling JWT token security keys and signing.

public class AuthService
{
  // Constructor
  public AuthService()
  {
    // Optional debugging lines to print out the JWT configuration values to ensure they are loaded correctly.
    Console.WriteLine($"JWT Key Length: {Environment.GetEnvironmentVariable("Jwt__Key")?.Length}");
    Console.WriteLine($"JWT Issuer: {Environment.GetEnvironmentVariable("Jwt__Issuer")}");
    Console.WriteLine($"JWT Audience: {Environment.GetEnvironmentVariable("Jwt__Audience")}");
  }

  // Method to generate a JWT token for a given user.
  public string GenerateJwtToken(UserDto user)
  {
    var tokenHandler = new JwtSecurityTokenHandler(); // Handler responsible for creating and validating JWTs. It handles the token's lifecycle.

    // Convert the secret key from the environment variable into a byte array.
    var jwtKey = Environment.GetEnvironmentVariable("Jwt__Key") ?? throw new InvalidOperationException("JWT Key is missing in environment variables.");
    var key = Encoding.ASCII.GetBytes(jwtKey);

    // Settings for token: Describe the token using SecurityTokenDescriptor.
    var tokenDescriptor = new SecurityTokenDescriptor
    {
      // ClaimsIdentity represents claims (attributes) associated with the user.
      Subject = new ClaimsIdentity(new[]
        {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()), // Often used to store a user ID, which is critical for identifying the user within your system.
                new Claim(ClaimTypes.Name, user.Name), // User's name.
                new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User") // User's role, determining access level.
            }),
      Expires = DateTime.UtcNow.AddHours(36), // Set the token to expire in 36 hours from creation.

      // The signing credentials contain the security key and the algorithm used for signature validation.
      SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),

      // Optional
      Issuer = Environment.GetEnvironmentVariable("Jwt__Issuer"),
      Audience = Environment.GetEnvironmentVariable("Jwt__Audience")
    };

    var token = tokenHandler.CreateToken(tokenDescriptor);

    // Serialize the token to a JWT string.
    return tokenHandler.WriteToken(token);
  }
}
