using System.IdentityModel.Tokens.Jwt; // Import for handling JWT token creation and management.
using System.Security.Claims; // Import for dealing with claims-based identity.
using System.Text; // Import for encoding utilities.
using api.Models; // Assuming your application's data transfer objects (DTOs) are located here.
using Microsoft.IdentityModel.Tokens; // Import for handling JWT token security keys and signing.

public class AuthService
{
  private readonly IConfiguration _configuration; // Configuration interface to access app settings.

  // Constructor that injects the configuration settings into the service.
  public AuthService(IConfiguration configuration)
  {
    _configuration = configuration; // Store the injected configuration in a private field.

    // Optional debugging lines to print out the JWT configuration values to ensure they are loaded correctly.
    // Console.WriteLine($"JWT Key Length: {_configuration["Jwt:Key"].Length}");
    // Console.WriteLine($"JWT Issuer: {_configuration["Jwt:Issuer"]}");
    // Console.WriteLine($"JWT Audience: {_configuration["Jwt:Audience"]}");
  }

  // Method to generate a JWT token for a given user.
  public string GenerateJwtToken(UserDto user)
  {
    var tokenHandler = new JwtSecurityTokenHandler(); // Handler responsible for creating and validating JWTs. It handles the token's lifecycle.

    // Convert the secret key from the configuration into a byte array.
    var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is missing in configuration.");

    var key = Encoding.ASCII.GetBytes(jwtKey);

    // settings for token: Describe the token using SecurityTokenDescriptor.
    var tokenDescriptor = new SecurityTokenDescriptor
    {
      // ClaimsIdentity represents claims (attributes) associated with the user.
      Subject = new ClaimsIdentity(new[]
        {
                // new Claim(JwtRegisteredClaimNames.Sub, user.Email), // sub (Subject) claim: Typically the unique identifier of the user (e.g., email or user ID). It’s crucial as it tells the server who the token represents.
                // new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Helps in maintaining token uniqueness and is useful for tracking or revoking tokens if necessary.

                // optional claims
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()), // Often used to store a user ID, which is critical for identifying the user within your system.
                new Claim(ClaimTypes.Name, user.Name), // User's name.
                new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User") // User's role, determining access level.
        }),
      Expires = DateTime.UtcNow.AddHours(36), // Set the token to expire in 1 minute from creation.
      // Expires = DateTime.UtcNow.AddDays(2), // Set the token to expire in 2 hours from creation.
      // Expires = DateTime.UtcNow.AddHours(2),

      // The signing credentials contain the security key and the algorithm used for signature validation.
      SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),

      // optional
      Issuer = _configuration["Jwt:Issuer"], // "iss" (issuer) claim: The issuer of the token.
      Audience = _configuration["Jwt:Audience"] // "aud" (audience) claim: Intended recipient of the token.
    };

    // Create the token based on the descriptor.
    var token = tokenHandler.CreateToken(tokenDescriptor);

    // Serialize the token to a JWT string.
    return tokenHandler.WriteToken(token);
  }
}
