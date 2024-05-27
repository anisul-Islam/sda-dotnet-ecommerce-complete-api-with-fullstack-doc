# Web API

## Da

### Data Annotations

- **Simplicity and Readability**: Data annotations allow you to configure the model directly within the class definitions. This can be more straightforward and intuitive, especially for simpler configurations like setting up primary keys, required fields, and relationships.
- **Quick Setup**: Annotations are quick to implement and require less code, making them ideal for smaller projects or where the model configurations are not too complex.
- **Limitations**: Data annotations have limitations in their capabilities. For example, some more complex relationships or database features (like complex keys, filters, or advanced database functions) cannot be fully configured using annotations alone.

### Fluent API

- **Power and Flexibility**: The Fluent API provides more options and greater flexibility than data annotations. It can configure aspects of the model that annotations cannot, such as configuring cascade delete behaviors in a more granular way, setting up complex types, and table splitting.
- **Centralized Configuration**: Fluent API configurations are typically placed in the `OnModelCreating` method in your `DbContext` class. This centralization can make it easier to manage and review model configurations as your project grows.
- **No Dependency on System.ComponentModel.DataAnnotations**: Fluent API keeps your entity classes clean of persistence-related configuration, reducing the coupling between your domain model and the database technology.

### Choosing Between Fluent API and Data Annotations

Here are some considerations to help you decide:

- **Complexity and Size of the Project**: For larger projects with complex data models, Fluent API is often more suitable because it handles complicated configurations more gracefully.
- **Team Preferences and Standards**: Sometimes the choice depends on team standards or developer preferences. Some teams prefer keeping entity classes clean and putting all configuration in the context class.
- **Interchangeability**: You can mix both Fluent API and data annotations in the same project, but it’s best practice to stick with one method for similar types of configuration to maintain consistency in your codebase.

### Example

For instance, if you're setting a column to be required and unique, here’s how you might do it with both approaches:

**Data Annotations:**

```csharp
public class User
{
    [Required]
    [EmailAddress]
    [StringLength(100)]
    [Column(TypeName = "varchar(100)")]
    public string Email { get; set; }
}
```

**Fluent API:**

```csharp
modelBuilder.Entity<User>().Property(u => u.Email)
    .IsRequired()
    .HasMaxLength(100)
    .HasColumnType("varchar(100)");
modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
```

### Conclusion

If you find that data annotations are sufficient for your needs and you prefer the simplicity they offer, it’s perfectly fine to use them. However, for more advanced configurations, or to keep your domain models free from infrastructure concerns, the Fluent API is the better choice. Many experienced developers prefer Fluent API for large or complex projects due to its scalability and flexibility.

## Complete Ecommerce API

### 0. Bootstrap the project

- Install the packages

```csharp
  dotnet add package Microsoft.AspNetCore.OData --version 8.0.0-preview3
  dotnet add package Microsoft.AspNetCore.Mvc.NewtonsoftJson --version 6.0.0-preview.6.21355.2
  dotnet add package Swashbuckle.AspNetCore --version 6.2.3
  dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
  dotnet add package Microsoft.EntityFrameworkCore.Design
  dotnet tool install --global dotnet-ef
```

```csharp
// Program.cs
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });
// ReferenceHandler.IgnoreCycles => This option prevents the serializer from generating circular references by ignoring them, eliminating the $id and $ref properties.
// JsonIgnoreCondition.WhenWritingNull => This option ensures that null properties are not included in the JSON output.

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/", () =>
{

    return "Welcome to the api";
})
.WithOpenApi();


app.MapControllers();
app.Run();
```

- create few folders -> Controllers, Services, Dtos, Helpers

### 1. Create the UserDto or UserModel (Data Transfer Object)

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class UserDto
    {

        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        // public string Password { get; set; } = string.Empty; // Note: Include cautiously
        public string Address { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public bool IsAdmin { get; set; } = false;// Include only if necessary for the context
        public bool IsBanned { get; set; } = false;// Include only if necessary for the context
        public DateTime CreatedAt { get; set; }

        public List<OrderModel> Orders { get; set; } = new List<OrderModel>();  // One-to-many with Order

    }
}
```

### 2. Create the User Entity

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using api.EntityFrameworkCore;


namespace ECommerceAPI.Models
{
    [Table("Users")]
    public class User
    {

        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public bool IsAdmin { get; set; } = false;
        public bool IsBanned { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<Order> Orders { get; set; } = new List<Order>();  // One-to-many with Order
    }
}

```

### 3. Setup the Context

```csharp
using api.EntityFrameworkCore;
using api.Models;
using ECommerceAPI.Models;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.UserId); // Primary Key configuration
            entity.Property(u => u.UserId).HasDefaultValueSql("uuid_generate_v4()"); // generate UUID for new records
            entity.Property(u => u.Name).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(100);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Password).IsRequired();
            entity.Property(u => u.Address).HasMaxLength(255);
            entity.Property(u => u.IsAdmin).HasDefaultValue(false);
            entity.Property(u => u.IsBanned).HasDefaultValue(false);
            entity.Property(u => u.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }
}

```

### 4. Configure the Database

```json
// appsettings.json
 "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=5432;Database=full-ecommerce-db;Username=postgres;Password=new_password;"
  }

// inside the Startup file
builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
```

### 5. Add the migration scripts

- Install Entity Framework Core Tools: Ensure that you have installed the Entity Framework Core Tools globally. You can do this by running the following command: dotnet tool install --global dotnet-ef

```csharp
dotnet ef migrations add InitialCreate
dotnet ef database update
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
```

### 6. Create the Controllers

```csharp
// UserController.cs
using Microsoft.AspNetCore.Mvc;
using ECommerceAPI.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using api.Controllers;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }
}

```

### 7. Create the Services

```csharp
// UserService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ECommerceAPI.Models;
using Microsoft.EntityFrameworkCore;

public class UserService
{
    private readonly AppDbContext _appDbcontext;

    public UserService(AppDbContext context)
    {
        _appDbcontext = context;
    }
}
```

### 8. Register the Service and Dtos

```csharp
// Program.cs
builder.Services.AddScoped<UserService>();
// Program.cs
builder.Services.AddControllers();
app.MapControllers();
```

### 9. GET /api/users -> Get All Users

```csharp
GET http://localhost:5267/api/users

// UserController
[HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        try
        {
            var users = await _userService.GetAllUsersAsync();
            return ApiResponse.Success(users, "All users retrieved successfully.");
        }
        catch (ApplicationException ex)
        {
            return ApiResponse.ServerError(ex.Message);
        }
        catch (Exception ex)
        {
            // Log the exception details here to debug or trace issues
            Console.WriteLine($"Exception : {ex.Message}");
            return ApiResponse.ServerError("An unexpected error occurred.");
        }
    }

// UserService
 public async Task<List<UserDto>> GetAllUsersAsync()
    {
        try
        {
            return await _appDbcontext.Users.ToListAsync();
        }
        catch (Exception ex)
        {
            // Optionally log the exception
            throw new ApplicationException("An error occurred while retrieving users.", ex);
        }
    }
```

### 10. Create the Centralized API Response

```csharp
using Microsoft.AspNetCore.Mvc;
using System;

namespace api.Controllers
{
  public static class ApiResponse
  {
    // Central method to handle the creation of ApiResponseTemplate with ObjectResult
    private static IActionResult CreateApiResponse<T>(T? data, string message, int statusCode, bool success)
    {
      var response = new ApiResponseTemplate<T>(success, data, message, statusCode);
      return new ObjectResult(response)
      {
        StatusCode = statusCode
      };
    }

    public static IActionResult Success<T>(T data, string message = "Success")
    {
      return CreateApiResponse(data, message, StatusCodes.Status200OK, true);
    }

    public static IActionResult Created<T>(T data, string message = "Resource Created")
    {
      return CreateApiResponse(data, message, StatusCodes.Status201Created, true);
    }

    public static IActionResult NotFound(string message = "Resource not found")
    {
      return CreateApiResponse<object>(null, message, StatusCodes.Status404NotFound, false);
    }

    public static IActionResult Conflict(string message = "Conflict Detected")
    {
      return CreateApiResponse<object>(null, message, StatusCodes.Status409Conflict, false);
    }

    public static IActionResult BadRequest(string message = "Bad request")
    {
      return CreateApiResponse<object>(null, message, StatusCodes.Status400BadRequest, false);
    }

    public static IActionResult Unauthorized(string message = "Unauthorized access")
    {
      return CreateApiResponse<object>(null, message, StatusCodes.Status401Unauthorized, false);
    }

    public static IActionResult Forbidden(string message = "Forbidden access")
    {
      return CreateApiResponse<object>(null, message, StatusCodes.Status403Forbidden, false);
    }

    public static IActionResult ServerError(string message = "Internal server error")
    {
      return CreateApiResponse<object>(null, message, StatusCodes.Status500InternalServerError, false);
    }
  }

  public class ApiResponseTemplate<T>
  {
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string Message { get; set; }
    public int StatusCode { get; set; }

    public ApiResponseTemplate(bool success, T? data, string message, int statusCode)
    {
      Success = success;
      Data = data;
      Message = message;
      StatusCode = statusCode;
    }
  }
}

```

### 11. GET /api/users/{userId} -> Get Single User

```csharp
GET http://localhost:5267/api/users

###

GET http://localhost:5267/api/users/4ee4fcff-6288-4997-b6fe-a7947723506f

// UserController.cs
[HttpGet("{userId}")]
    public async Task<IActionResult> GetUserById(Guid userId)
    {
        try
        {
            var userDto = await _userService.GetUserByIdAsync(userId);
            if (userDto == null)
            {
                return ApiResponse.NotFound("User not found");
            }
            return ApiResponse.Success(userDto, "User retrieved successfully");
        }
        catch (Exception ex)
        {
            // Log the exception
            Console.WriteLine($"Exception : {ex.Message}");
            return ApiResponse.ServerError("An unexpected error occurred while retrieving the user.");
        }
    }

// UserService.cs
 // GET /api/users/{userId} -> Get a single User
    public async Task<UserDto> GetUserByIdAsync(Guid userId)
    {
        var user = await _appDbcontext.Users
            .Where(u => u.UserId == userId)
            .Select(u => new UserDto
            {
                UserId = u.UserId,
                Name = u.Name,
                Email = u.Email,
                Address = u.Address,
                Image = u.Image,
                IsAdmin = u.IsAdmin,
                IsBanned = u.IsBanned
            })
            .FirstOrDefaultAsync();

        return user; // This will be null if the user is not found
    }
```

### 12. POST /api/users -> Create an User

```csharp
POST http://localhost:5267/api/users
Content-Type: application/json

{
  "name": "user3",
  "email": "user3@example.com",
  "password": "string",
  "address": "string",
  "image": "string"
}

// Create the CreateUserModel or CreateUserDto
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class CreateUserDto
    {
        public Guid UserId { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Name must be between 2 and 100 characters.", MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(255, ErrorMessage = "Password must be between 6 and 255 characters.", MinimumLength = 6)]
        public string Password { get; set; } = string.Empty; // Note: Include cautiously

        [StringLength(255)]
        public string Address { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public bool IsAdmin { get; set; } = false;// Include only if necessary for the context
        public bool IsBanned { get; set; } = false;// Include only if necessary for the context
        public DateTime CreatedAt { get; set; }

    }

}

// UserController.cs
[HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto newUserData)
    {
        if (user == null)
            return ApiResponse.BadRequest("Invalid user data");

        try
        {
            var newUser = await _userService.AddUserAsync(newUserData);
            return ApiResponse.Created(newUser, "User created successfully");
        }
        catch (InvalidOperationException ex)
        {
            return ApiResponse.Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            // Log the exception
            Console.WriteLine($"Exception : {ex.Message}");
            return ApiResponse.ServerError("An unexpected error occurred while creating the user.");
        }
    }

// UserService.cs
 // POST /api/users -> Create an User
    public async Task<newUserData> AddUserAsync(CreateUserDto newUserData)
    {
        try
        {
            _appDbcontext.Users.Add(newUserData);
            await _appDbcontext.SaveChangesAsync();
            return user;
        }
        catch (DbUpdateException ex)
        {
            // Handle specific database errors, e.g., unique constraint violations
            throw new InvalidOperationException("Could not save user to database", ex);
        }
    }
```

### 13. Add Input Validation for User

- Model Validation: The UserModel uses data annotations to ensure that all necessary fields are present and correctly formatted before the model reaches the service layer.

- Service Layer: The service layer converts the validated CreateUserModel into a User entity, ensuring that the business logic and data storage concerns are cleanly separated.

- Controller Logic: The controller checks ModelState.IsValid to determine if the incoming request data conforms to the defined validation rules. If the data is invalid, it returns a BadRequest response with details of the validation errors.

```csharp
// Step 1: Define a User Model with Validation Attributes
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class UserModel
    {

        public Guid UserId { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Name must be between 2 and 100 characters.", MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(255, ErrorMessage = "Password must be between 6 and 255 characters.", MinimumLength = 6)]
        public string Password { get; set; } = string.Empty; // Note: Include cautiously

        [StringLength(255)]
        public string Address { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public bool IsAdmin { get; set; } = false;// Include only if necessary for the context
        public bool IsBanned { get; set; } = false;// Include only if necessary for the context
        public DateTime CreatedAt { get; set; }

        public List<OrderModel> Orders { get; set; } = new List<OrderModel>();  // One-to-many with Order

    }
}

// Step 2: Update the UserService to Use CreateUserModel

// POST /api/users -> Create an User
    public async Task<User> AddUserAsync(UserModel newUser)
    {
        var user = new User
        {
            Name = newUser.Name,
            Email = newUser.Email,
            Password = newUser.Password, // Remember to hash the password
            Address = newUser.Address,
            Image = newUser.Image,
            IsAdmin = newUser.IsAdmin,
            IsBanned = newUser.IsBanned,
        };

        try
        {
            _appDbcontext.Users.Add(user);
            await _appDbcontext.SaveChangesAsync();
            return user;
        }
        catch (DbUpdateException ex)
        {
            // Handle specific database errors, e.g., unique constraint violations
            throw new InvalidOperationException("Could not save user to database", ex);
        }
    }

// Step 3: Update the UserController to Validate Input
 [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] UserModel newUserData)
    {
        if (!ModelState.IsValid)
        {
            return ApiResponse.BadRequest("Invalid user data");
        }

        try
        {
            var newUser = await _userService.AddUserAsync(newUserData);
            return ApiResponse.Created(newUser, "User created successfully");
        }
        catch (InvalidOperationException ex)
        {
            return ApiResponse.Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            // Log the exception
            Console.WriteLine($"Exception : {ex.Message}");
            return ApiResponse.ServerError("An unexpected error occurred while creating the user.");
        }
    }
```

### 14. add MinimalApis.Extenstion by using nuget gallery extension in vscode

```csharp
// to all the endpoints in Program.cs

// app.MapControllers().WithParameterValidation();

// to individual endpoints in Program.cs file
// app.MapGet("/api/users", () => new UserController().GetAllUsers()).WithParameterValidation();
// app.MapGet("/api/users/{userId}", (Guid userId) => new UserController().GetUser(userId)).WithParameterValidation();
// app.MapPost("/api/users", (User newUser) => new UserController().CreateUser(newUser)).WithParameterValidation();
// app.MapPut("/api/users/{userId}", (Guid userId, User updateUser) => new UserController().UpdateUser(userId, updateUser)).WithParameterValidation();
// app.MapDelete("/api/users/{userId}", (Guid userId) => new UserController().DeleteUser(userId)).WithParameterValidation();
```

### 15. Hash the Password before adding to Database

```csharp
// Step 1: Update UserService to Include IPasswordHasher
private readonly IPasswordHasher<User> _passwordHasher;
public UserService(AppDbContext context, IPasswordHasher<User> passwordHasher)
{
    _appDbcontext = context;
    _passwordHasher = passwordHasher;
}

// POST /api/users -> Create an User
public async Task<User> AddUserAsync(CreateUserModel userModel)
{
    var user = new User
    {
        Name = userModel.Name,
        Email = userModel.Email,
        Address = userModel.Address,
        Image = userModel.Image,
        IsAdmin = userModel.IsAdmin,
        IsBanned = userModel.IsBanned,
    };

    user.Password = _passwordHasher.HashPassword(user, userModel.Password),

    try
    {
        _appDbcontext.Users.Add(user);
        await _appDbcontext.SaveChangesAsync();
        return user;
    }
    catch (DbUpdateException ex)
    {
        // Handle specific database errors, e.g., unique constraint violations
        throw new InvalidOperationException("Could not save user to database", ex);
    }
}

// Step 2: Register IPasswordHasher in Startup.cs
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
```

### 16. Return specific data

```csharp
// for getting all users without password
 var users = await _dbContext.Users
            .Select(u => new UserDto
            {
                UserId = u.UserId,
                Name = u.Name,
                Email = u.Email,
                Address = u.Address,
                Image = u.Image,
                IsAdmin = u.IsAdmin,
                IsBanned = u.IsBanned,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();

        return users;

// when user is created and skip password
  try
    {
        _appDbcontext.Users.Add(user);
        await _appDbcontext.SaveChangesAsync();
        // Convert the User entity to a UserDto
        var userDto = new UserDto
        {
            UserId = user.UserId,
            Name = user.Name,
            Email = user.Email,
            Address = user.Address,
            Image = user.Image,
            IsAdmin = user.IsAdmin,
            IsBanned = user.IsBanned
        };

        return userDto;

    }
```

### 17. Pagination

```csharp
// receive the query parameters
 [HttpGet]
    public async Task<IActionResult> GetAllUsers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var response = await _userService.GetAllUsersAsync(pageNumber, pageSize);

            return ApiResponse.Success(response, "Users retrieved successfully.");
        }
        catch (ApplicationException ex)
        {
            return ApiResponse.ServerError(ex.Message);
        }
        catch (Exception ex)
        {
            // Log the exception details here to debug or trace issues
            Console.WriteLine($"Exception : {ex.Message}");
            return ApiResponse.ServerError("An unexpected error occurred.");
        }
    }

// create the PaginationResult
public class PaginatedResult<T>
{
  public IEnumerable<T> Items { get; set; } = new List<T>();
  public int TotalCount { get; set; }
  public int PageNumber { get; set; }
  public int PageSize { get; set; }
  public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}


// adjust the service
// GET /api/users -> Get All Users
    public async Task<PaginatedResult<UserDto>> GetAllUsersAsync(int pageNumber, int pageSize)
    {
        try
        {

            // var query = _appDbcontext.Users.AsQueryable();

            var totalCount = await _appDbcontext.Users.CountAsync();

            var users = await _appDbcontext.Users
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserDto
            {
                UserId = u.UserId,
                Name = u.Name,
                Email = u.Email,
                Address = u.Address,
                Image = u.Image,
                IsAdmin = u.IsAdmin,
                IsBanned = u.IsBanned,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();

            return new PaginatedResult<UserDto>
            {
                Items = users,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
        catch (Exception ex)
        {
            // Optionally log the exception
            throw new ApplicationException("An error occurred while retrieving users.", ex);
        }
    }

```

### 18. Searching & sorting

```csharp
// Create a class for Parameter template
public class QueryParameters
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SearchTerm { get; set; } = string.Empty;
    public string SortBy { get; set; } = "Name"; // Default sorting by name
    public string SortOrder { get; set; } = "asc"; // asc or desc
}


// update the service
 // GET /api/users -> Get All Users
    public async Task<PaginatedResult<UserDto>> GetAllUsersAsync(QueryParameters queryParams)
    {
        var query = _appDbcontext.Users.AsQueryable();

        // Search based on name or email
       if (!string.IsNullOrEmpty(queryParams.SearchTerm))
        {
            var lowerCaseSearchTerm = queryParams.SearchTerm.ToLower();
            query = query.Where(u => u.Name.Contains(lowerCaseSearchTerm) || u.Email.Contains(lowerCaseSearchTerm));
        }

        // Sorting
        if (!string.IsNullOrEmpty(queryParams.SortBy))
        {
            query = queryParams.SortOrder == "desc" ?
                query.OrderByDescending(u => EF.Property<object>(u, queryParams.SortBy)) :
                query.OrderBy(u => EF.Property<object>(u, queryParams.SortBy));
        }

        var totalCount = await query.CountAsync();
        var users = await query
            .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .Select(u => new UserDto
            {
                UserId = u.UserId,
                Name = u.Name,
                Email = u.Email,
                Address = u.Address,
                Image = u.Image,
                IsAdmin = u.IsAdmin,
                IsBanned = u.IsBanned
            })
            .ToListAsync();

        return new PaginatedResult<UserDto>
        {
            Items = users,
            TotalCount = totalCount,
            PageNumber = queryParams.PageNumber,
            PageSize = queryParams.PageSize
        };
    }

// update the controller
[HttpGet]
public async Task<IActionResult> GetAllUsers([FromQuery] QueryParameters queryParams)
{
    try
    {
        var result = await _userService.GetAllUsersAsync(queryParams);
        if (!result.Items.Any())
        {
            return ApiResponse.NotFound("No users found.");
        }
        return ApiResponse.Success(result, "Users retrieved successfully.");
    }
    catch (Exception ex)
    {
        return ApiResponse.ServerError("An unexpected error occurred while retrieving users.");
    }
}

```

### Filtering by categories

```csharp
// QueryParams
public class QueryParameters
{
  public int PageNumber { get; set; } = 1;
  public int PageSize { get; set; } = 5;
  public string SearchTerm { get; set; } = "";
  public string SortBy { get; set; } = "Name"; // Default sorting by name
  public string SortOrder { get; set; } = "asc"; // asc or desc

  public List<Guid> SelectedCategories { get; set; } = new List<Guid>();
}

// Service
public async Task<PaginatedResult<ProductDto>> GetAllProductsAsync(QueryParameters queryParams)
  {
    var query = _appDbcontext.Products
                                  .Include(p => p.Categories)
                                  .AsQueryable();


    if (!string.IsNullOrEmpty(queryParams.SearchTerm))
    {
      var lowerCaseSearchTerm = queryParams.SearchTerm.ToLower();
      query = query.Where(p => p.Name.ToLower().Contains(lowerCaseSearchTerm) || p.Description.ToLower().Contains(lowerCaseSearchTerm));
    }

    if (!string.IsNullOrEmpty(queryParams.SortBy))
    {
      query = queryParams.SortOrder == "desc"
          ? query.OrderByDescending(u => EF.Property<object>(u, queryParams.SortBy))
          : query.OrderBy(u => EF.Property<object>(u, queryParams.SortBy));
    }

    if (queryParams.SelectedCategories != null && queryParams.SelectedCategories.Any())
    {
      query = query.Where(p => p.Categories.Any(c => queryParams.SelectedCategories.Contains(c.CategoryId)));
    }

    var totalCount = await query.CountAsync();
    var products = await query
        .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
        .Take(queryParams.PageSize)
        .ToListAsync();
    var productDtos = _mapper.Map<List<ProductDto>>(products);

    return new PaginatedResult<ProductDto>
    {
      Items = productDtos,
      TotalCount = totalCount,
      PageNumber = queryParams.PageNumber,
      PageSize = queryParams.PageSize
    };
  }
```

### 19. PUT /api/users/{userId} -> Update User By Id

```csharp
// DTO
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

// Controller
 // [Authorize]
  [HttpPut("{userId}")]
  public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] UpdateUserDto updateUserDto)
  {
      if (!ModelState.IsValid)
      {
          return ApiResponse.BadRequest("Invalid user data provided.");
      }

      var updatedUserDto = await _userService.UpdateUserAsync(userId, updateUserDto);

      if (updatedUserDto == null)
      {
          return ApiResponse.NotFound($"User with ID {userId} not found.");
      }

      return ApiResponse.Success(updatedUserDto, "User updated successfully.");
  }

// Service
 public async Task<UserDto?> UpdateUserAsync(Guid userId, UpdateUserDto userData)
{
    var user = await _appDbcontext.Users.FindAsync(userId);
    if (user == null)
    {
        return null;
    }

    // without mapper
    // if (!string.IsNullOrEmpty(userData.Name))
    //     user.Name = userData.Name;

    // if (!string.IsNullOrEmpty(userData.Address))
    //     user.Address = userData.Address;

    // if (!string.IsNullOrEmpty(userData.Image))
    //     user.Image = userData.Image;

    // if (userData.IsAdmin.HasValue)
    //     user.IsAdmin = userData.IsAdmin.Value;

    // if (userData.IsBanned.HasValue)
    //     user.IsBanned = userData.IsBanned.Value;

    // with mapper
    _mapper.Map(userData, user);

    if (userData.IsAdmin.HasValue)
        user.IsAdmin = userData.IsAdmin.Value;
    if (userData.IsBanned.HasValue)
        user.IsBanned = userData.IsBanned.Value;

    _appDbcontext.Users.Update(user);
    await _appDbcontext.SaveChangesAsync();
    return _mapper.Map<UserDto>(user);
}

// Mapper
using api.EntityFrameworkCore;
using api.Models;
using AutoMapper;
using ECommerceAPI.Models;

namespace api.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDto>();
            CreateMap<CreateUserDto, User>();
            CreateMap<Category, CategoryDto>();
            CreateMap<CreateCategoryDto, Category>();
            CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.Categories));
            CreateMap<CreateProductDto, Product>()
             .ForMember(dest => dest.Categories, opt => opt.Ignore());

            // Add this line to map UpdateUserDto to User
            CreateMap<UpdateUserDto, User>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // With AutoMapper now configured, your service methods can utilize the IMapper interface to map data models to DTOs and vice versa, simplifying data transformation and reducing boilerplate code.
        }
    }
}
```

### 20. DELETE /api/users/{userId} -> Delete User By Id

```csharp
// Controller
[Authorize(Roles = "Admin")] // Ensuring only administrators can delete users
[HttpDelete("{userId}")]
public async Task<IActionResult> DeleteUser(Guid userId)
{
    try
    {
        bool deleted = await _userService.DeleteUserAsync(userId);
        if (!deleted)
        {
            return ApiResponse.NotFound("User not found");
        }

        return ApiResponse.Success("null", "User deleted successfully");
    }
    catch (Exception ex)
    {
        // Log the exception
        Console.WriteLine($"Exception : {ex.Message}");
        return ApiResponse.ServerError("An unexpected error occurred while deleting the user.");
    }
}

// Service
// DELETE /api/users/{userId}
public async Task<bool> DeleteUserAsync(Guid userId)
{
    var user = await _appDbcontext.Users.FindAsync(userId);
    if (user == null)
    {
        return false; // User not found
    }

    _appDbcontext.Users.Remove(user);
    await _appDbcontext.SaveChangesAsync();
    return true;
}
```

### 21. POST /api/users/login -> User Login

```csharp
POST http://localhost:5267/api/users/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "string"
}

// add the service
public async Task<UserDto?> ValidateUserAsync(LoginDto loginDto)
{
    var user = await _appDbcontext.Users.SingleOrDefaultAsync(u => u.Email == loginDto.Email);
    if (user == null)
    {
        return null;
    }

    if (user.IsBanned)
    {
        throw new UserBannedException("User is banned");
    }

    var result = _passwordHasher.VerifyHashedPassword(user, user.Password, loginDto.Password);
    if (result == PasswordVerificationResult.Failed)
    {
        return null;
    }

    return new UserDto
    {
        UserId = user.UserId,
        Name = user.Name,
        Email = user.Email,
        Image = user.Image,
        IsAdmin = user.IsAdmin,
        IsBanned = user.IsBanned
    };
}

// add the controller
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
{
    if (!ModelState.IsValid)
    {
        return ApiResponse.BadRequest("Invalid User Data");
    }

    try
    {
        var user = await _userService.LoginUserAsync(loginDto);
        if (user == null)
        {
            return ApiResponse.Unauthorized("Invalid credentials.");
        }
        if (user.IsBanned)
        {
            return ApiResponse.Unauthorized("You are banned. Please contact us via email");
        }

        var token = _authService.GenerateJwtToken(user);

        // return ApiResponse.Success(new { Token = token, User = user }, "User logged in successfully"); // Returns the user data
        return ApiResponse.Success(new { token, user }, "User logged in successfully"); // Returns the user data
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Exception : {ex.Message}");
        return ApiResponse.ServerError("An internal error occurred while processing your request.");
    }
}
```

### 22. How to generate JWT Token

- step 1: Install the package `dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer`
- step 2: Configure JWT Settings in appsettings.json

```json

 "Jwt": {
    "Key": "your_secret_key_here_make_it_long_and_random",
    "Issuer": "LocalhostIssuer",
    "Audience": "LocalhostAudience"
  }

Explanation:

Key: This should be a secret key used for signing the JWTs. Make sure it's complex and not guessable. Even in development, it's a good practice to keep this key secure to mimic production conditions as closely as possible.

Issuer: This is the entity that issues the token. In a local development environment, it will typically be your localhost URL along with the port number that your API is running on. For ASP.NET Core applications, the default URL is often http://localhost:5000 unless you've configured it differently.

Audience: This is intended to be the recipient of the JWT. In most cases for local development, this will be the same as the issuer since your front-end and back-end will likely both be hosted locally for development purposes.
```

- step 3: create the AuthService.cs

```csharp
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
    Console.WriteLine($"JWT Key Length: {_configuration["Jwt:Key"].Length}");
    Console.WriteLine($"JWT Issuer: {_configuration["Jwt:Issuer"]}");
    Console.WriteLine($"JWT Audience: {_configuration["Jwt:Audience"]}");
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
      Expires = DateTime.UtcNow.AddMinutes(1), // Set the token to expire in 1 minute from creation.
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

```

- step 4: use it from the controller to generate the token

```csharp
 [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var user = await _userService.LoginUserAsync(loginDto);
            if (user == null)
            {
                return ApiResponse.Unauthorized("Invalid credentials.");
            }

            var token = _authService.GenerateJwtToken(user);

            // return ApiResponse.Success(new { Token = token, User = user }, "User logged in successfully"); // Returns the user data
            return ApiResponse.Success(new { token, user }, "User logged in successfully"); // Returns the user data
        }
        catch (Exception ex)
        {
            return ApiResponse.ServerError("An internal error occurred while processing your request.");
        }
    }
```

- step 5: setup startup file for validating the token

```csharp
// for jwt
// Add services to the DI container.
var Configuration = builder.Configuration; // Ensure this is accessible

var key = Encoding.ASCII.GetBytes(Configuration["Jwt:Key"]);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Set to true in production
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = Configuration["Jwt:Issuer"],
        ValidAudience = Configuration["Jwt:Audience"],
        ClockSkew = TimeSpan.Zero
    };
});

```

- step 6: use the token to access an authorized route to find info about the user

```csharp

GET http://localhost:5267/api/users/profile
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiJjMTU4OTliOS0zNTQzLTQ2YzQtOGYxNy1mZjQzMzI4NDFh

[Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        // The presence of userIdString indicates that the user is authenticated.
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Console.WriteLine($"{userIdString}");

        // If there's no user ID claim, the token is considered invalid for this endpoint.
        if (string.IsNullOrEmpty(userIdString))
        {
            return ApiResponse.Unauthorized("User ID is missing from token.");
        }

        // Handle the case where the user ID format in the token is incorrect.
        if (!Guid.TryParse(userIdString, out Guid userId))
        {
            return ApiResponse.BadRequest("Invalid user ID format in token.");
        }

        // Attempt to retrieve the user with the provided ID.
        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null)
        {
            return ApiResponse.NotFound("User not found.");
        }

        return ApiResponse.Success(user);
    }
```

### 23. User Logout

### 24. GET /api/users/ban-unban/{id} -> Ban/unban the user

```csharp
// UserService
 public async Task<bool> BanUserAsync(Guid userId)
    {
        var user = await _appDbcontext.Users.FindAsync(userId);
        if (user == null)
        {
            return false;
        }

        user.IsBanned = !user.IsBanned;
        _appDbcontext.Users.Update(user);
        await _appDbcontext.SaveChangesAsync();
        return true;
    }

// UserController
[Authorize] // Ensure the user is authenticated
[HttpPost("ban/{userId}")]
public async Task<IActionResult> BanUser(Guid userId)
{
    // Check if the current user is an admin
    var isAdmin = User.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
    if (!isAdmin)
    {
        return ApiResponse.Forbidden("Only admins can perform this action.");
    }

    var result = await _userService.BanUserAsync(userId);
    if (result)
    {
        return Ok($"User {userId} has been banned.");
    }
    else
    {
        return ApiResponse.NotFound($"User {userId} not found.");
    }
}

or
[Authorize(Roles = "Admin")]
[HttpPost("ban/{userId}")]
public async Task<IActionResult> BanUser(Guid userId)
{
    var result = await _userService.BanUnbanUserAsync(userId);
    if (result)
    {
        return ApiResponse.Success($"User with this id {userId} has been banned.");
    }
    else
    {
        return ApiResponse.NotFound($"User with this id {userId} not found.");
    }
}
```

### 25. GET /api/users/banned-users -> Get List of banned users

- Retrieve a list of all users who are currently banned. This is useful for administrative purposes.

- http request example

```http
GET http://localhost:5267/api/users/banned-users
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiI0ZWU0ZmNmZi02Mjg4LTQ5OTctYjZmZS
```

```csharp
// Controllers
[Authorize(Roles = "Admin")]
[HttpGet("banned-users")]
public async Task<IActionResult> GetBannedUsers()
{
    try
    {
        var users = await _userService.GetBannedUsersAsync();
        if (!users.Any())
        {
            return ApiResponse.NotFound("No banned users found.");
        }

        return ApiResponse.Success(users, "Banned users retrieved successfully.");
    }
    catch (Exception ex)
    {
        return ApiResponse.ServerError("An error occurred while retrieving banned users.");
    }
}

// Services
// GET /api/users/banned-users
public async Task<List<UserDto>> GetBannedUsersAsync()
{
    var users = await _appDbcontext.Users
                                .Where(u => u.IsBanned)
                                .Select(u => new UserDto
                                {
                                    UserId = u.UserId,
                                    Name = u.Name,
                                    Email = u.Email,
                                    IsAdmin = u.IsAdmin,
                                    IsBanned = u.IsBanned
                                })
                                .ToListAsync();
    return users;
}
```

### Add Authorization option in swagger

- install `dotnet add package Swashbuckle.AspNetCore`

- add in Program.cs

```cs
builder.Services.AddSwaggerGen(c =>
{
c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
// Add JWT Authentication to Swagger
c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
{
    In = ParameterLocation.Header,
    Description = "Please enter a valid token",
    Name = "Authorization",
    Type = SecuritySchemeType.Http,
    BearerFormat = "JWT",
    Scheme = "Bearer"
});

c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
```

### 26. POST /api/users/reset-password -> Reset password

- request example

```http
POST http://localhost:5267/api/users/reset-password/4ee4fcff-6288-4997-b6fe-a7947723506f
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiI0ZWU0ZmNmZi02Mjg4LTQ5OTctYjZmZS1hNzk0NzcyMzUwNmYiLCJ1bmlxdWVfbmFtZSI6InN0cmluZyIsInJvbGUiOiJBZG1pbiIsIm5iZiI6MTcxNTE2OTQzOCwiZXhwIjoxNzE1MTY5NDk4LCJpYXQiOjE3MTUxNjk0MzgsImlzcyI6IkxvY2FsaG9zdElzc3VlciIsImF1ZCI6IkxvY2FsaG9zdEF1ZGllbmNlIn0.4RinPqcQHpTEKX1DdvPjzoTdTeVbIW7mQTvf4bD30e4
Content-Type: application/json

{
  "newPassword": "anisul"
}
```

```csharp
// Create a DTO

namespace api.Dtos.Users
{
    public class ResetPasswordDto
    {
        public string NewPassword { get; set; }
    }
}

// Controllers
[Authorize] // or [Authorize(Roles = "Admin")] if only admins should perform this action
[HttpPost("reset-password/{userId}")]
public async Task<IActionResult> ResetPassword(Guid userId, [FromBody] ResetPasswordDto resetPasswordDto)
{
    if (!ModelState.IsValid)
    {
        return ApiResponse.BadRequest("Invalid request data");
    }

    try
    {
        var result = await _userService.ResetPasswordAsync(userId, resetPasswordDto.NewPassword);
        if (!result)
        {
            return ApiResponse.NotFound($"User with ID {userId} not found.");
        }

        return ApiResponse.Success("", "Password reset successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Exception : {ex.Message}");
        return ApiResponse.ServerError("An error occurred while resetting the password.");
    }
}

// Services
// POST /api/users/reset-password
    public async Task<bool> ResetPasswordAsync(Guid userId, string newPassword)
    {
        var user = await _appDbcontext.Users.FindAsync(userId);
        if (user == null)
        {
            return false; // User not found
        }

        user.Password = _passwordHasher.HashPassword(user, newPassword);
        await _appDbcontext.SaveChangesAsync();

        return true;
    }
```

### 27. AutoMapper

- AutoMapper is a popular library in .NET that helps simplify the process of transferring data between objects, particularly between data models and data transfer objects (DTOs) in applications. can utilize the IMapper interface to map data models to DTOs and vice versa, simplifying data transformation and reducing boilerplate code.

- Without AutoMapper: You need to manually assign each property from the entity to the DTO. This can be error-prone and cumbersome, especially with complex objects.

Benfits of AutoMapper

1. Reduction of Boilerplate Code
   AutoMapper reduces the amount of manual mapping code you need to write. Without AutoMapper, you would typically write code to map properties from one object to another explicitly. This can be tedious and error-prone, especially with complex objects. AutoMapper automates this by mapping properties based on convention, which can significantly clean up your codebase.

2. Ease of Maintenance
   When your data model changes, maintaining manual mappings can be cumbersome. AutoMapper helps centralize the mapping logic, making it easier to manage and update. Changes in the data model require changes in the mapping configuration rather than throughout the code where data transformations occur.

3. Consistency
   AutoMapper encourages consistency in how mappings are handled across an application. By defining mappings in one place, you ensure that the same mapping logic is applied everywhere in the application, reducing the risk of inconsistencies in data handling and manipulation.

4. Customization and Flexibility
   Although AutoMapper works well with convention-based mapping, it also provides extensive options for customization. You can define custom conversion rules, handle complex type conversions, and conditionally map properties. This flexibility makes it suitable for a wide range of scenarios from simple to complex.

5. Improved Productivity
   Developers can focus more on the business logic rather than the details of converting between object types. AutoMapper handles the mundane task of copying data from one object to another, which can speed up development time and reduce bugs related to data transformation.

6. Integration with LINQ
   AutoMapper integrates well with LINQ, allowing for projections directly from database queries to DTOs. This can optimize performance by avoiding the need to retrieve all columns from the database or load entire entity graphs when only a subset is needed.

7. Support for Nested Objects
   AutoMapper can automatically handle nested objects and collections, which can be complex to map manually. It knows how to traverse these structures and map corresponding elements from source to destination.

```csharp
// Step 1: Install AutoMapper
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection


// Step 2: Setup AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Step 3: Define Mapping Profile:
// Create a class to define the mapping configurations.
using api.Models;
using AutoMapper;
using ECommerceAPI.Models;

namespace api.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDto>();
            CreateMap<CreateUserDto, User>();
            // With AutoMapper now configured, your service methods can utilize the IMapper interface to map data models to DTOs and vice versa, simplifying data transformation and reducing boilerplate code.
        }
    }
}

// Step 4: Refactor API to Use AutoMapper:
// Inside the service
private readonly IMapper _mapper;
public UserService(AppDbContext context, IPasswordHasher<User> passwordHasher, IMapper mapper)
{
    _appDbcontext = context;
    _passwordHasher = passwordHasher;
    _mapper = mapper;
}

[HttpGet("{id}")]
public async Task<IActionResult> GetUserById(Guid id)
{
    var user = await _dbContext.Users.FindAsync(id);
    if (user == null)
    {
        return NotFound();
    }

    var userDto = _mapper.Map<UserDto>(user); // AutoMapper handles the mapping

    return Ok(userDto);
}
```

#### After adding AutoMapper to the UserService

```csharp
using api.Dtos;
using api.Models;
using AutoMapper;
using ECommerceAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class UserService
{
    private readonly AppDbContext _appDbcontext;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IMapper _mapper;

    public UserService(AppDbContext context, IPasswordHasher<User> passwordHasher, IMapper mapper)
    {
        _appDbcontext = context;
        _passwordHasher = passwordHasher;
        _mapper = mapper;
    }

    // GET /api/users -> Get All Users
    public async Task<PaginatedResult<UserDto>> GetAllUsersAsync(UserQueryParameters queryParams)
    {
        var query = _appDbcontext.Users.AsQueryable();

        // Search based on name or email
        if (!string.IsNullOrEmpty(queryParams.SearchTerm))
        {
            query = query.Where(u => u.Name.Contains(queryParams.SearchTerm) || u.Email.Contains(queryParams.SearchTerm));
        }

        if (!string.IsNullOrEmpty(queryParams.SortBy))
        {
            query = queryParams.SortOrder == "desc" ?
                query.OrderByDescending(u => EF.Property<object>(u, queryParams.SortBy)) :
                query.OrderBy(u => EF.Property<object>(u, queryParams.SortBy));
        }

        var totalCount = await query.CountAsync();
        var users = await query
            .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync();
        var userDtos = _mapper.Map<List<UserDto>>(users);
        return new PaginatedResult<UserDto>
        {
            Items = userDtos,
            TotalCount = totalCount,
            PageNumber = queryParams.PageNumber,
            PageSize = queryParams.PageSize
        };
    }

    // GET /api/users/{userId} -> Get a single User
    public async Task<UserDto?> GetUserByIdAsync(Guid userId)
    {
        var user = await _appDbcontext.Users.FindAsync(userId);
        if (user == null)
        {
            return null;
        }

        var userDto = _mapper.Map<UserDto>(user);

        return userDto;
    }

    // POST /api/users -> Create an User
    public async Task<UserDto> AddUserAsync(CreateUserDto newUserData)
    {
        var user = _mapper.Map<User>(newUserData);
        user.Password = _passwordHasher.HashPassword(null, newUserData.Password);

        try
        {
            _appDbcontext.Users.Add(user);
            await _appDbcontext.SaveChangesAsync();
            // Convert the User entity to a UserDto
            return _mapper.Map<UserDto>(user);


        }
        catch (DbUpdateException ex)
        {
            // Handle specific database errors, e.g., unique constraint violations
            throw new InvalidOperationException("Could not save user to database", ex);
        }
    }

    // PUT /api/users/{userId} -> update an user
    public async Task<bool> UpdateUserAsync(Guid userId, UpdateUserDto userData)
    {
        var user = await _appDbcontext.Users.FindAsync(userId);
        if (user == null)
        {
            return false; // User not found
        }

        // Use AutoMapper to update the user's properties
        _mapper.Map(userData, user);

        // Handle partial update scenarios
        if (userData.IsAdmin.HasValue)
            user.IsAdmin = userData.IsAdmin.Value;
        if (userData.IsBanned.HasValue)
            user.IsBanned = userData.IsBanned.Value;

        _appDbcontext.Users.Update(user);
        await _appDbcontext.SaveChangesAsync();
        return true;
    }

    // GET /api/users/login -> login a user
    public async Task<UserDto?> LoginUserAsync(LoginDto loginDto)
    {
        var user = await _appDbcontext.Users.SingleOrDefaultAsync(u => u.Email == loginDto.Email);
        if (user == null)
        {
            return null;
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.Password, loginDto.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            return null;
        }


        return _mapper.Map<UserDto>(user);
    }

    // GET /api/users/ban-unban -> Ban / Unabn a user
    public async Task<bool> BanUnbanUserAsync(Guid userId)
    {
        var user = await _appDbcontext.Users.FindAsync(userId);
        if (user == null)
        {
            return false;
        }

        user.IsBanned = !user.IsBanned;
        _appDbcontext.Users.Update(user);
        await _appDbcontext.SaveChangesAsync();
        return true;
    }

    // GET /api/users/banned-users -> List of banned users
    public async Task<List<UserDto>> GetBannedUsersAsync()
    {
        var users = await _appDbcontext.Users
                                    .Where(u => u.IsBanned)
                                    .ToListAsync();
        return _mapper.Map<List<UserDto>>(users);
    }

    // POST /api/users/reset-password
    public async Task<bool> ResetPasswordAsync(Guid userId, string newPassword)
    {
        var user = await _appDbcontext.Users.FindAsync(userId);
        if (user == null)
        {
            return false; // User not found
        }

        user.Password = _passwordHasher.HashPassword(user, newPassword);
        await _appDbcontext.SaveChangesAsync();

        return true;
    }

    // DELETE /api/users/{userId}
    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        var user = await _appDbcontext.Users.FindAsync(userId);
        if (user == null)
        {
            return false; // User not found
        }

        _appDbcontext.Users.Remove(user);
        await _appDbcontext.SaveChangesAsync();
        return true;
    }
}

```

### 28. Middleware

#### What is Middleware?

In the context of ASP.NET Core and many modern web frameworks, middleware is software that's assembled into an application pipeline to handle requests and responses. Each component:

- Chooses whether to pass the request to the next component in the pipeline.
- Can perform actions before and after the next component in the pipeline.

Middleware components are executed in the order they are added to the application pipeline, and they can make operations such as logging, exception handling, authentication, etc., modular and reusable across an application.

#### Types of Middleware

1. **Authentication Middleware**: Validates the identity of users. It interrupts the pipeline if the user is not authenticated but is trying to access a resource that requires authentication.

2. **Authorization Middleware**: Checks if the authenticated user has permission to access the requested resource.

3. **Exception Handling Middleware**: Catches exceptions that occur during request processing and handles them, for example, by logging errors or returning specific HTTP error responses.

4. **Static File Middleware**: Enables serving static files, such as HTML, CSS, images, and JavaScript.

5. **CORS Middleware**: Manages Cross-Origin Resource Sharing (CORS) policies, enabling or restricting resources based on where the request is coming from.

6. **Routing Middleware**: Analyzes the request and decides which route or controller should handle the request.

7. **Session Middleware**: Manages user sessions by storing and retrieving user data while the user browses the application.

8. **Response Compression Middleware**: Compresses the API responses before sending them to the clients, reducing the bandwidth usage.

9. **Logging Middleware**: Logs requests, responses, and exceptions, which helps in debugging and monitoring applications.

10. **Custom Middleware**: You can create custom middleware for any specific functionality that you need to process in your application pipeline.

#### Simple Example of a Middleware

Here's an example of a simple logging middleware in an ASP.NET Core application. This middleware logs each request and its corresponding response status.

```csharp
public class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoggingMiddleware> _logger;

    public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        _logger.LogInformation($"Handling request: {context.Request.Method} {context.Request.Path}");
        try
        {
            await _next(context);
        }
        finally
        {
            _logger.LogInformation($"Finished handling request. Response Status: {context.Response.StatusCode}");
        }
    }
}
```

#### Registering the Middleware

To use this middleware, you need to register it in the application's request processing pipeline, typically in the `Startup` class or in `Program.cs` for .NET 6 or later.

```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseMiddleware<LoggingMiddleware>();

    app.UseRouting();
    app.UseAuthorization();
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
    });
}
```

In this configuration, the `LoggingMiddleware` is inserted after `UseHttpsRedirection` and `UseStaticFiles`, but before routing and authorization. This allows the middleware to log information about both the incoming request and the outgoing response.

Middleware in ASP.NET Core serves as a powerful way to handle HTTP requests and responses in a modular and reusable manner. By using middleware, you can ensure that certain operations like logging, authentication, and exception handling are consistently applied across your application. Custom middleware can be tailored to suit specific needs, enhancing the flexibility and functionality of your application.

#### Adding Exception Handling Middleware

```csharp
private static Task HandleExceptionAsync(HttpContext context, Exception exception)
{
    context.Response.ContentType = "application/json";

    // Default response for an unexpected error
    var responseCode = StatusCodes.Status500InternalServerError;
    var message = "An unexpected error has occurred.";

    // Handling specific exceptions
    switch (exception)
    {
        case NotFoundException notFoundException:
            responseCode = StatusCodes.Status404NotFound;
            message = notFoundException.Message;
            break;

        case ValidationException validationException:
            responseCode = StatusCodes.Status400BadRequest;
            message = validationException.Message;
            break;

        case UnauthorizedAccessException unauthorizedAccessException:
            responseCode = StatusCodes.Status401Unauthorized;
            message = unauthorizedAccessException.Message;
            break;

        case ForbiddenAccessException forbiddenAccessException:
            responseCode = StatusCodes.Status403Forbidden;
            message = forbiddenAccessException.Message;
            break;

        case ConflictException conflictException:
            responseCode = StatusCodes.Status409Conflict;
            message = conflictException.Message;
            break;

        case BadRequestException badRequestException:
            responseCode = StatusCodes.Status400BadRequest;
            message = badRequestException.Message;
            break;

        case ApplicationException applicationException:
            // Handling generic application exceptions that might be used for business logic errors
            responseCode = StatusCodes.Status400BadRequest;
            message = applicationException.Message;
            break;

        default:
            // Log the exception if it's not one of the expected types
            Console.WriteLine("Unhandled exception type: ", exception.GetType());
            break;
    }

    context.Response.StatusCode = responseCode;

    var response = new
    {
        StatusCode = responseCode,
        Message = message
    };

    var jsonResponse = JsonSerializer.Serialize(response);
    return context.Response.WriteAsync(jsonResponse);
}
```

1. **NotFoundException**: Represents a scenario where a requested resource was not found.
2. **ValidationException**: Used when data validation fails.
3. **UnauthorizedAccessException**: Indicates an authentication failure.
4. **ForbiddenAccessException**: Used when an authenticated user does not have permission to access a resource.
5. **ConflictException**: Represents conflicts during the request processing (e.g., editing conflicts in database operations).
6. **BadRequestException**: General bad request errors not covered by more specific exceptions.

#### Implementing Custom Exceptions

If you plan to use custom exceptions like `NotFoundException` or `ForbiddenAccessException`, ensure they are implemented in your application. For instance:

```csharp
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

public class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException(string message) : base(message) { }
}

public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}

public class BadRequestException : Exception
{
    public BadRequestException(string message) : base(message) { }
}
```

By handling different types of exceptions explicitly, your middleware not only improves the API's reliability by providing meaningful error information to the client but also aids in debugging by making the origin of errors clearer. This setup ensures that your application can gracefully handle errors and communicate them effectively to clients.

#### Let's update our code accoriding to the exception handling middleware

```csharp
// Controller
using Microsoft.AspNetCore.Mvc;
using api.Controllers;
using api.Models;
using api.Dtos;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using api.Middlewares;
using System.ComponentModel.DataAnnotations;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;
    private readonly AuthService _authService;

    public UserController(UserService userService, AuthService authService)
    {
        _userService = userService;
        _authService = authService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers([FromQuery] UserQueryParameters queryParams)
    {
        var result = await _userService.GetAllUsersAsync(queryParams);
        if (!result.Items.Any())
        {
            throw new NotFoundException("No users found.");
        }
        return ApiResponse.Success(result, "Users retrieved successfully.");
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUserById(Guid userId)
    {
        var userDto = await _userService.GetUserByIdAsync(userId) ?? throw new NotFoundException("User not found");
        return ApiResponse.Success(userDto, "User retrieved successfully");
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto newUserData)
    {
        if (!ModelState.IsValid)
        {
            throw new ValidationException("Invalid user data");
        }

        var newUser = await _userService.AddUserAsync(newUserData);
        return ApiResponse.Created(newUser, "User created successfully");
    }

    [HttpPut("{userId}")]
    public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] UpdateUserDto updateUserDto)
    {
        if (!ModelState.IsValid)
        {
            throw new ValidationException("Invalid user data provided.");
        }

        var result = await _userService.UpdateUserAsync(userId, updateUserDto);
        if (!result)
        {
            throw new NotFoundException($"User with ID {userId} not found.");
        }

        return NoContent();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        if (!ModelState.IsValid)
        {
            throw new ValidationException("Invalid User Data");
        }

        var user = await _userService.LoginUserAsync(loginDto) ?? throw new UnauthorizedAccessException("Invalid credentials.");
        var token = _authService.GenerateJwtToken(user);
        return ApiResponse.Success(new { token, user }, "User logged in successfully");
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString))
        {
            throw new UnauthorizedAccessException("User ID is missing from token.");
        }

        if (!Guid.TryParse(userIdString, out Guid userId))
        {
            throw new ValidationException("Invalid user ID format in token.");
        }

        var user = await _userService.GetUserByIdAsync(userId) ?? throw new NotFoundException("User not found.");
        return ApiResponse.Success(user);
    }
}

// Service
using api.Dtos;
using api.Models;
using AutoMapper;
using ECommerceAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class UserService
{
    private readonly AppDbContext _appDbcontext;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IMapper _mapper;

    public UserService(AppDbContext context, IPasswordHasher<User> passwordHasher, IMapper mapper)
    {
        _appDbcontext = context;
        _passwordHasher = passwordHasher;
        _mapper = mapper;
    }

    // GET /api/users -> Get All Users
    public async Task<PaginatedResult<UserDto>> GetAllUsersAsync(UserQueryParameters queryParams)
    {
        var query = _appDbcontext.Users.AsQueryable();

        // Search based on name or email
        if (!string.IsNullOrEmpty(queryParams.SearchTerm))
        {
            query = query.Where(u => u.Name.Contains(queryParams.SearchTerm) || u.Email.Contains(queryParams.SearchTerm));
        }

        if (!string.IsNullOrEmpty(queryParams.SortBy))
        {
            query = queryParams.SortOrder == "desc" ?
                query.OrderByDescending(u => EF.Property<object>(u, queryParams.SortBy)) :
                query.OrderBy(u => EF.Property<object>(u, queryParams.SortBy));
        }

        var totalCount = await query.CountAsync();
        var users = await query
            .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync();
        var userDtos = _mapper.Map<List<UserDto>>(users);
        return new PaginatedResult<UserDto>
        {
            Items = userDtos,
            TotalCount = totalCount,
            PageNumber = queryParams.PageNumber,
            PageSize = queryParams.PageSize
        };
    }

    // GET /api/users/{userId} -> Get a single User
    public async Task<UserDto?> GetUserByIdAsync(Guid userId)
    {
        var user = await _appDbcontext.Users.FindAsync(userId);
        if (user == null)
        {
            return null;
        }

        var userDto = _mapper.Map<UserDto>(user);

        return userDto;
    }

    // POST /api/users -> Create an User
    public async Task<UserDto> AddUserAsync(CreateUserDto newUserData)
    {
        var user = _mapper.Map<User>(newUserData);
        user.Password = _passwordHasher.HashPassword(null, newUserData.Password);


            _appDbcontext.Users.Add(user);
            await _appDbcontext.SaveChangesAsync();
            // Convert the User entity to a UserDto
            return _mapper.Map<UserDto>(user);

    }

    // PUT /api/users/{userId} -> update an user
    public async Task<bool> UpdateUserAsync(Guid userId, UpdateUserDto userData)
    {
        var user = await _appDbcontext.Users.FindAsync(userId);
        if (user == null)
        {
            return false; // User not found
        }

        // Use AutoMapper to update the user's properties
        _mapper.Map(userData, user);

        // Handle partial update scenarios
        if (userData.IsAdmin.HasValue)
            user.IsAdmin = userData.IsAdmin.Value;
        if (userData.IsBanned.HasValue)
            user.IsBanned = userData.IsBanned.Value;

        _appDbcontext.Users.Update(user);
        await _appDbcontext.SaveChangesAsync();
        return true;
    }

    // GET /api/users/login -> login a user
    public async Task<UserDto?> LoginUserAsync(LoginDto loginDto)
    {
        var user = await _appDbcontext.Users.SingleOrDefaultAsync(u => u.Email == loginDto.Email);
        if (user == null)
        {
            return null;
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.Password, loginDto.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            return null;
        }


        return _mapper.Map<UserDto>(user);
    }

    // GET /api/users/ban-unban -> Ban / Unabn a user
    public async Task<bool> BanUnbanUserAsync(Guid userId)
    {
        var user = await _appDbcontext.Users.FindAsync(userId);
        if (user == null)
        {
            return false;
        }

        user.IsBanned = !user.IsBanned;
        _appDbcontext.Users.Update(user);
        await _appDbcontext.SaveChangesAsync();
        return true;
    }

    // GET /api/users/banned-users -> List of banned users
    public async Task<List<UserDto>> GetBannedUsersAsync()
    {
        var users = await _appDbcontext.Users
                                    .Where(u => u.IsBanned)
                                    .ToListAsync();
        return _mapper.Map<List<UserDto>>(users);
    }

    // POST /api/users/reset-password
    public async Task<bool> ResetPasswordAsync(Guid userId, string newPassword)
    {
        var user = await _appDbcontext.Users.FindAsync(userId);
        if (user == null)
        {
            return false; // User not found
        }

        user.Password = _passwordHasher.HashPassword(user, newPassword);
        await _appDbcontext.SaveChangesAsync();

        return true;
    }

    // DELETE /api/users/{userId}
    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        var user = await _appDbcontext.Users.FindAsync(userId);
        if (user == null)
        {
            return false; // User not found
        }

        _appDbcontext.Users.Remove(user);
        await _appDbcontext.SaveChangesAsync();
        return true;
    }
}

```

### 29. Simplifying the codes with ternary operators

```csharp
using api.Dtos;
using api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace api.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly AuthService _authService;

        public UserController(UserService userService, AuthService authService)
        {
            _userService = userService;
            _authService = authService;
        }

        private IActionResult HandleNullResult<T>(T result, string notFoundMessage)
        {
            return result != null
                ? ApiResponse.Success(result, notFoundMessage.Replace("not found", "retrieved successfully"))
                : ApiResponse.NotFound(notFoundMessage);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers([FromQuery] UserQueryParameters queryParams)
        {
            var result = await _userService.GetAllUsersAsync(queryParams);
            return result.Items.Any()
                ? ApiResponse.Success(result, "Users retrieved successfully.")
                : ApiResponse.NotFound("No users found.");
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserById(Guid userId)
        {
            var userDto = await _userService.GetUserByIdAsync(userId);
            return HandleNullResult(userDto, "User not found");
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto newUserData)
        {
            if (!ModelState.IsValid)
            {
                return ApiResponse.BadRequest("Invalid user data");
            }

            var newUser = await _userService.AddUserAsync(newUserData);
            return ApiResponse.Created(newUser, "User created successfully");
        }

        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] UpdateUserDto updateUserDto)
        {
            if (!ModelState.IsValid)
            {
                return ApiResponse.BadRequest("Invalid user data provided.");
            }

            var result = await _userService.UpdateUserAsync(userId, updateUserDto);
            return result ? NoContent() : ApiResponse.NotFound($"User with ID {userId} not found.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return ApiResponse.BadRequest("Invalid User Data");
            }

            var user = await _userService.LoginUserAsync(loginDto);
            if (user == null)
            {
                return ApiResponse.Unauthorized("Invalid credentials");
            }

            var token = _authService.GenerateJwtToken(user);
            return ApiResponse.Success(new { token, user }, "User logged in successfully");
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString))
            {
                return ApiResponse.Unauthorized("User ID is missing from token.");
            }

            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                return ApiResponse.BadRequest("Invalid user ID format in token.");
            }

            var user = await _userService.GetUserByIdAsync(userId);
            return HandleNullResult(user, "User not found");
        }
    }
}
using api.Dtos;
using api.Models;
using AutoMapper;
using ECommerceAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class UserService
{
    private readonly AppDbContext _appDbcontext;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IMapper _mapper;

    public UserService(AppDbContext context, IPasswordHasher<User> passwordHasher, IMapper mapper)
    {
        _appDbcontext = context;
        _passwordHasher = passwordHasher;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<UserDto>> GetAllUsersAsync(UserQueryParameters queryParams)
    {
        var query = _appDbcontext.Users.AsQueryable();

        if (!string.IsNullOrEmpty(queryParams.SearchTerm))
        {
            query = query.Where(u => u.Name.Contains(queryParams.SearchTerm) || u.Email.Contains(queryParams.SearchTerm));
        }

        if (!string.IsNullOrEmpty(queryParams.SortBy))
        {
            query = queryParams.SortOrder == "desc"
                ? query.OrderByDescending(u => EF.Property<object>(u, queryParams.SortBy))
                : query.OrderBy(u => EF.Property<object>(u, queryParams.SortBy));
        }

        var totalCount = await query.CountAsync();
        var users = await query
            .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync();
        var userDtos = _mapper.Map<List<UserDto>>(users);

        return new PaginatedResult<UserDto>
        {
            Items = userDtos,
            TotalCount = totalCount,
            PageNumber = queryParams.PageNumber,
            PageSize = queryParams.PageSize
        };
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid userId)
    {
        var user = await _appDbcontext.Users.FindAsync(userId);
        return user != null ? _mapper.Map<UserDto>(user) : null;
    }

    public async Task<UserDto> AddUserAsync(CreateUserDto newUserData)
    {
        var user = _mapper.Map<User>(newUserData);
        user.Password = _passwordHasher.HashPassword(null, newUserData.Password);

        _appDbcontext.Users.Add(user);
        await _appDbcontext.SaveChangesAsync();
        return _mapper.Map<UserDto>(user);
    }

    public async Task<bool> UpdateUserAsync(Guid userId, UpdateUserDto userData)
    {
        var user = await _appDbcontext.Users.FindAsync(userId);
        if (user == null)
        {
            return false;
        }

        _mapper.Map(userData, user);

        if (userData.IsAdmin.HasValue)
            user.IsAdmin = userData.IsAdmin.Value;
        if (userData.IsBanned.HasValue)
            user.IsBanned = userData.IsBanned.Value;

        _appDbcontext.Users.Update(user);
        await _appDbcontext.SaveChangesAsync();
        return true;
    }

    public async Task<UserDto?> LoginUserAsync(LoginDto loginDto)
    {
        var user = await _appDbcontext.Users.SingleOrDefaultAsync(u => u.Email == loginDto.Email);
        if (user == null)
        {
            return null;
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.Password, loginDto.Password);
        return result == PasswordVerificationResult.Failed ? null : _mapper.Map<UserDto>(user);
    }

    public async Task<bool> BanUnbanUserAsync(Guid userId)
    {
        var user = await _appDbcontext.Users.FindAsync(userId);
        if (user == null)
        {
            return false;
        }

        user.IsBanned = !user.IsBanned;
        _appDbcontext.Users.Update(user);
        await _appDbcontext.SaveChangesAsync();
        return true;
    }

    public async Task<List<UserDto>> GetBannedUsersAsync()
    {
        var users = await _appDbcontext.Users
            .Where(u => u.IsBanned)
            .ToListAsync();
        return _mapper.Map<List<UserDto>>(users);
    }

    public async Task<bool> ResetPasswordAsync(Guid userId, string newPassword)
    {
        var user = await _appDbcontext.Users.FindAsync(userId);
        if (user == null)
        {
            return false;
        }

        user.Password = _passwordHasher.HashPassword(user, newPassword);
        await _appDbcontext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        var user = await _appDbcontext.Users.FindAsync(userId);
        if (user == null)
        {
            return false;
        }

        _appDbcontext.Users.Remove(user);
        await _appDbcontext.SaveChangesAsync();
        return true;
    }
}

```

### 30. Environment variables setup

- list all env variables : Type printenv or env and press Enter in terminal.

- set `export MyVariable="Value"
`

```csharp
IDictionary envVariables = Environment.GetEnvironmentVariables();
// Environment.GetEnvironmentVariables() to retrieve all environment variables and then iterates over them to print each one.

foreach (DictionaryEntry envVar in envVariables)
{
    Console.WriteLine($"{envVar.Key} = {envVar.Value}");
}


or
public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        // Accessing an environment variable
        var myVariable = Configuration["MyVariable"];

        // Use the variable for configuration purposes
        services.AddSomeService(options => options.ConfigValue = myVariable);
    }
}


```

### 31. CORS Setup

```csharp

dotnet add package Microsoft.AspNetCore.Cors

 builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowSpecificOrigin", builder =>
        {
            builder.WithOrigins("http://localhost:3000", // Specify the allowed origins
                                "https://www.yourclientapp.com") // Add additional origins as needed
                   .AllowAnyMethod() // Allows all methods
                   .AllowAnyHeader() // Allows all headers
                   .AllowCredentials(); // Allows credentials like cookies, authorization headers, etc.
        });
    });

app.UseRouting();
app.UseCors("MyAllowSpecificOrigins");
app.UseAuthentication();
app.UseAuthorization();

[EnableCors("MyAllowSpecificOrigins")]
```

### Users API

```csharp
// service

// controller
```

### 32. Category: Create the CategoryDto or CategoryModel (Data Transfer Object)

```csharp
namespace api.Models
{
    public class CategoryDto
    {
        public Guid CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<ProducDto> Products { get; set; } = new List<ProductDto>();  // Many-to-many with Product
        public DateTime CreatedAt { get; set; }
    }
}
```

### 33. Category: Create the Category Entity

```csharp
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace api.EntityFrameworkCore
{
    [Table("Categories")]
    public class Category
    {
        public Guid CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<Product> Products { get; set; } = new List<Product>();  // Many-to-many with Product
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
```

### 34. Category: Update the context and migration script

```csharp
 modelBuilder.Entity<Category>(entity =>
{
    entity.Property(e => e.CategoryId).HasDefaultValueSql("uuid_generate_v4()");
    entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
    entity.Property(e => e.Slug).IsRequired();
    entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
});

dotnet ef migrations add InitialCategoryCreate
dotnet ef database update
// CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
```

### 35. Category: Setup the basic service, controller and register the service to startup file

```csharp
// CategoryService

public class CategoryService
{
    private readonly AppDbContext _appDbcontext;
    public CategoryService(AppDbContext context)
    {
        _appDbcontext = context;
    }

}


// CategoryController
using api.Dtos;
using api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace api.Controllers
{
  [ApiController]
  [Route("api/categories")]
  public class CategoryController : ControllerBase
  {
    private readonly CategoryService _categoryService;
    private readonly AuthService _authService;

    public CategoryController(CategoryService categoryService, AuthService authService)
    {
      _categoryService = categoryService;
      _authService = authService;
    }
  }
}

// Program.cs
builder.Services.AddScoped<CategoryService>();

```

### 36. POST => /api/categories => Create a category

- create CreateCategoryDto

```csharp
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class CreateCategoryDto
    {

        [Required]
        [StringLength(100, ErrorMessage = "Name must be between 2 and 200 characters.", MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

    }

}
```

- update the mapper

```csharp
using api.EntityFrameworkCore;
using api.Models;
using AutoMapper;
using ECommerceAPI.Models;

namespace api.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDto>();
            CreateMap<CreateUserDto, User>();
            CreateMap<Category, CategoryDto>();
            CreateMap<CreateCategoryDto, Category>();
            // With AutoMapper now configured, your service methods can utilize the IMapper interface to map data models to DTOs and vice versa, simplifying data transformation and reducing boilerplate code.
        }
    }
}
```

- create the controller

```csharp
 [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto newCategoryData)
    {
      if (!ModelState.IsValid)
      {
        return ApiResponse.BadRequest("Invalid category data");
      }

      var newCategory = await _categoryService.AddCategoryAsync(newCategoryData);
      return ApiResponse.Created(newCategory, "Category created successfully");
    }
```

- create the service

```csharp
// helper for generating slug
namespace api.Helpers
{
    public class Helper
    {
        public static string GenerateSlug(string name)
        {
            return name.ToLower().Replace(" ", "-");
        }
    }
}

public async Task<CategoryDto> AddCategoryAsync(CreateCategoryDto newCategoryData)
{

    var category = _mapper.Map<Category>(newCategoryData);
    category.Slug = Helper.GenerateSlug(newCategoryData.Name);
    _appDbcontext.Categories.Add(category);
    await _appDbcontext.SaveChangesAsync();
    return _mapper.Map<CategoryDto>(category);
}
```

- here is some example JSON data for creating categories:

Example 1: Electronics Category

```json
{
  "name": "Electronics",
  "slug": "electronics",
  "description": "Devices and gadgets including phones, laptops, and other electronic equipment"
}
```

Example 2: Clothing Category

```json
{
  "name": "Clothing",
  "slug": "clothing",
  "description": "Apparel including shirts, pants, dresses, and more"
}
```

Example 3: Books Category

```json
{
  "name": "Books",
  "slug": "books",
  "description": "Various genres of books including fiction, non-fiction, educational, and more"
}
```

Example 4: Home & Kitchen Category

```json
{
  "name": "Home & Kitchen",
  "slug": "home-kitchen",
  "description": "Items for home improvement, kitchen tools, and appliances"
}
```

Example 5: Sports & Outdoors Category

```json
{
  "name": "Sports & Outdoors",
  "slug": "sports-outdoors",
  "description": "Equipment and gear for sports and outdoor activities"
}
```

### 37. GET => /api/categories => Get all categories

```csharp
// CategoryService.cs
public async Task<PaginatedResult<CategoryDto>> GetAllCategoriesAsync(QueryParameters queryParams)
    {
        var query = _appDbcontext.Categories
                                  .Include(c => c.Products)
                                  .AsQueryable();

        if (!string.IsNullOrEmpty(queryParams.SearchTerm))
        {
            query = query.Where(u => u.Name.Contains(queryParams.SearchTerm));
        }

        if (!string.IsNullOrEmpty(queryParams.SortBy))
        {
            query = queryParams.SortOrder == "desc"
                ? query.OrderByDescending(u => EF.Property<object>(u, queryParams.SortBy))
                : query.OrderBy(u => EF.Property<object>(u, queryParams.SortBy));
        }

        var totalCount = await query.CountAsync();
        var categories = await query
            .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync();
        var categoryDtos = _mapper.Map<List<CategoryDto>>(categories);

        return new PaginatedResult<CategoryDto>
        {
            Items = categoryDtos,
            TotalCount = totalCount,
            PageNumber = queryParams.PageNumber,
            PageSize = queryParams.PageSize
        };
    }

// CategoryController.cs
[HttpGet]
public async Task<IActionResult> GetAllCategories([FromQuery] QueryParameters queryParams)
{
    var result = await _categoryService.GetAllCategoriesAsync(queryParams);
    return result.Items.Any()
        ? ApiResponse.Success(result, "Categories retrieved successfully.")
        : ApiResponse.NotFound("No Categories found.");
}
```

### 38. GET => /api/categories/{identifier} => Get a category based on id or slug

```csharp
// Service
 public async Task<CategoryDto?> GetCategoryByIdentifierAsync(string identifier)
    {
        Category? category = null;

        if (Guid.TryParse(identifier, out Guid categoryId))
        {
            category = await _appDbcontext.Categories.FindAsync(categoryId);
        }
        else
        {
            category = await _appDbcontext.Categories.SingleOrDefaultAsync(c => c.Slug == identifier);
        }

        return category != null ? _mapper.Map<CategoryDto>(category) : null;
    }

// Controller
 [HttpGet("{identifier}")]
    public async Task<IActionResult> GetCategoryByIdentifier(string identifier)
    {
      var category = await _categoryService.GetCategoryByIdentifierAsync(identifier);
      if (category == null)
      {
        return ApiResponse.NotFound("Category not found");
      }
      return ApiResponse.Success(category, "Category retrieved successfully");
    }
```

### 40. DELETE => /api/categories/{identifier} => delete a category based on id or slug

```csharp

// service
public async Task<bool> DeleteCategoryAsync(Guid categoryId)
{
    var category = await _appDbcontext.Categories.FindAsync(categoryId);
    if (category == null)
    {
        return false;
    }

    _appDbcontext.Categories.Remove(category);
    await _appDbcontext.SaveChangesAsync();
    return true;
}

// controller
[HttpDelete("{categoryId}")]
public async Task<IActionResult> DeleteCategory(Guid categoryId)
{
    if (!ModelState.IsValid)
    {
    return ApiResponse.BadRequest("Invalid category data provided.");
    }

    var result = await _categoryService.DeleteCategoryAsync(categoryId);
    return result ? NoContent() : ApiResponse.NotFound($"Category with ID {categoryId} not found.");
}
```

### 41. GET => /api/categories/{slug} => GET a category based on slug

```csharp
// services
public async Task<CategoryDto?> GetCategoryBySlugAsync(string slug)
{
    var category = await _appDbcontext.Categories.SingleOrDefaultAsync(c => c.Slug == slug);
    return category != null ? _mapper.Map<CategoryDto>(category) : null;
}

// controller
/api/slug/{slug}
 [HttpGet("{identifier}")]
    public async Task<IActionResult> GetCategoryByIdOrSlug(string identifier)
    {
      if (Guid.TryParse(identifier, out Guid id))
      {
        var category = await _categoryService.GetCategoryByIdAsync(id);
        if (category == null)
        {
          return ApiResponse.NotFound("Category not found");
        }
        return ApiResponse.Success(category, "Category retrieved successfully");
      }
      else
      {
        var category = await _categoryService.GetCategoryBySlugAsync(identifier);
        if (category == null)
        {
          return ApiResponse.NotFound("Category not found");
        }
        return ApiResponse.Success(category, "Category retrieved successfully");
      }
    }
```

### 42. PUT => /api/categories/{identifier} => update a category based on slug

```csharp
// add DTO
namespace api.Models
{
    public class UpdateCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}

// mapper
using api.EntityFrameworkCore;
using api.Models;
using AutoMapper;
using ECommerceAPI.Models;

namespace api.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDto>();
            CreateMap<CreateUserDto, User>();
            CreateMap<Category, CategoryDto>();
            CreateMap<CreateCategoryDto, Category>();
            CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.Categories));
            CreateMap<CreateProductDto, Product>()
             .ForMember(dest => dest.Categories, opt => opt.Ignore());

            // Add this line to map UpdateUserDto to User
            CreateMap<UpdateUserDto, User>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<UpdateCategoryDto, Category>()
               .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // With AutoMapper now configured, your service methods can utilize the IMapper interface to map data models to DTOs and vice versa, simplifying data transformation and reducing boilerplate code.
        }
    }
}


// services
public async Task<CategoryDto?> UpdateCategoryByIdentifierAsync(string identifier, UpdateCategoryDto updateCategoryData)
{
    Category? category;

    if (Guid.TryParse(identifier, out Guid categoryId))
    {
        category = await _appDbcontext.Categories.FindAsync(categoryId);
    }
    else
    {
        category = await _appDbcontext.Categories.SingleOrDefaultAsync(c => c.Slug == identifier);
    }

    if (category == null)
    {
        return null;
    }

    _mapper.Map(updateCategoryData, category);

    // Regenerate the slug based on the updated name or other criteria
    category.Slug = Helper.GenerateSlug(category.Name);

    _appDbcontext.Categories.Update(category);
    await _appDbcontext.SaveChangesAsync();
    return _mapper.Map<CategoryDto>(category);
}


// controller
[Authorize(Roles = "Admin")]
[HttpPut("{identifier}")]
public async Task<IActionResult> UpdateCategoryByIdentifier(string identifier, [FromBody] UpdateCategoryDto updateCategoryDto)
{
  if (!ModelState.IsValid)
  {
    return ApiResponse.BadRequest("Invalid category data provided.");
  }

  var updatedCategory = await _categoryService.UpdateCategoryByIdentifierAsync(identifier, updateCategoryDto);

  return updatedCategory != null
      ? ApiResponse.Success(updatedCategory, "Category updated successfully.")
      : ApiResponse.NotFound($"Category with identifier '{identifier}' not found.");
}
```

### 43. DELETE => /api/categories/{slug} => delete a category based on slug

```csharp
// services
 public async Task<bool> DeleteCategoryBySlugAsync(string slug)
{
    var category = await _appDbcontext.Categories.SingleOrDefaultAsync(c => c.Slug == slug);
    if (category == null)
    {
        return false;
    }

    _appDbcontext.Categories.Remove(category);
    await _appDbcontext.SaveChangesAsync();
    return true;
}

// controller
[HttpDelete("{categoryId}")]
public async Task<IActionResult> DeleteCategory(Guid categoryId)
{
    if (!ModelState.IsValid)
    {
    return ApiResponse.BadRequest("Invalid category data provided.");
    }

    var result = await _categoryService.DeleteCategoryAsync(categoryId);
    return result ? NoContent() : ApiResponse.NotFound($"Category with ID {categoryId} not found.");
}
```

### 44. Product API => Create the ProductDto or Product Model

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class ProductDto
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
        public int OrderId { get; set; }  // Foreign key
        public OrderModel Order { get; set; } = new OrderModel();  // Navigation property
        public List<CategoryDto> Categories { get; set; } = new List<CategoryDto>();  // Many-to-many with Category

        public DateTime CreatedAt { get; set; }
    }
}
```

### 46. Product API => Create a product entity

```csharp
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
        public int OrderId { get; set; }  // Foreign key
        public Order Order { get; set; } = new Order();  // Navigation property
        public List<Category> Categories { get; set; } = new List<Category>();  // Many-to-many with Category

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
```

### 47. Product API => Adjust the context and add migration script

```csharp
modelBuilder.Entity<Product>(entity =>
{
    entity.HasKey(p => p.ProductId); // Primary Key
    entity.Property(p => p.ProductId).HasDefaultValueSql("uuid_generate_v4()");
    entity.Property(p => p.Name).IsRequired().HasMaxLength(255);
    entity.Property(p => p.Slug).IsRequired();
    entity.Property(p => p.Price).IsRequired();
    entity.Property(p => p.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
    // Configure the many-to-many relationship with Category
    entity.HasMany(p => p.Categories)
            .WithMany(c => c.Products);
});
```

### 48. Product API => setup service, controller and register the service

```csharp
// Service

using api.EntityFrameworkCore;
using api.Helpers;
using api.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

public class ProductService
{
  private readonly AppDbContext _appDbcontext;
  private readonly IMapper _mapper;
  public ProductService(AppDbContext context, IMapper mapper)
  {
    _appDbcontext = context;
    _mapper = mapper;
  }

}

// Controller
using api.Dtos;
using api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace api.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductController : ControllerBase
    {
        private readonly ProductService _productService;
        private readonly AuthService _authService;

        public ProductController(ProductService productService, AuthService authService)
        {
            _productService = productService;
            _authService = authService;
        }

        private IActionResult HandleNullResult<T>(T result, string notFoundMessage)
        {
            return result != null
                ? ApiResponse.Success(result, notFoundMessage.Replace("not found", "retrieved successfully"))
                : ApiResponse.NotFound(notFoundMessage);
        }
    }
}

// register the service
builder.Services.AddScoped<ProductService>();
```

### 49. Product API => POST => /api/ products => Create a product

- update the mapper

```csharp
using api.EntityFrameworkCore;
using api.Models;
using AutoMapper;
using ECommerceAPI.Models;

namespace api.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDto>();
            CreateMap<CreateUserDto, User>();
            CreateMap<Category, CategoryDto>();
            CreateMap<CreateCategoryDto, Category>();
            CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.Categories));
            CreateMap<CreateProductDto, Product>()
             .ForMember(dest => dest.Categories, opt => opt.Ignore());

            // With AutoMapper now configured, your service methods can utilize the IMapper interface to map data models to DTOs and vice versa, simplifying data transformation and reducing boilerplate code.
        }
    }
}
```

- create a dto for CreateProductDto

```csharp
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

        public DateTime CreatedAt { get; set; }
    }
}
```

- add a service

```csharp
 public async Task<ProductDto> AddProductAsync(CreateProductDto newProductData)
  {
    var product = _mapper.Map<Product>(newProductData);
    product.Slug = Helper.GenerateSlug(newProductData.Name);

    // Fetch categories based on CategoryIds
    var categories = await _appDbcontext.Categories
                                        .Where(c => newProductData.CategoryIds.Contains(c.CategoryId))
                                        .ToListAsync();

    product.Categories = categories;
    _appDbcontext.Products.Add(product);
    await _appDbcontext.SaveChangesAsync();
    return _mapper.Map<ProductDto>(product);
  }
```

- add a controller

```csharp
[HttpPost]
public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto newProductData)
{
    if (!ModelState.IsValid)
    {
        return ApiResponse.BadRequest("Invalid product data");
    }

    var newProduct = await _productService.AddProductAsync(newProductData);
    return ApiResponse.Created(newProduct, "Product created successfully");
}
```

- test

```json

{
  "name": "Fjallraven - Foldsack No. 1 Backpack, Fits 15 Laptops",
  "image": "https://fakestoreapi.com/img/81fPKd-2AYL._AC_SL1500_.jpg",
  "description": "Your perfect pack for everyday use and walks in the forest. Stash your laptop (up to 15 inches) in the padded sleeve, your everyday",
  "price": 100.95,
  "quantity": 10,
  "shipping": 2.5,
  "categoryIds": [
    "da0fe9ea-30fb-4cfd-b6ce-2dc7490d0d71"
  ]
}

{
  "name": "Mens Casual Premium Slim Fit T-Shirts ",
  "image": "https://fakestoreapi.com/img/81fPKd-2AYL._AC_SL1500_.jpg",
  "description": "Slim-fitting style, contrast raglan long sleeve, three-button henley placket, light weight & soft fabric for breathable and comfortable wearing. And Solid stitched shirts with round neck made for durability and a great fit for casual fashion wear and diehard baseball fans. The Henley style round neckline includes a three-button placket.",
  "price": 22.3,
  "quantity": 12,
  "shipping": 0,
  "categoryIds": [
    "da0fe9ea-30fb-4cfd-b6ce-2dc7490d0d71"
  ]
}


{

"name": "Mens Cotton Jacket",
"price": 55.99,
"description": "great outerwear jackets for Spring/Autumn/Winter, suitable for many occasions, such as working, hiking, camping, mountain/rock climbing, cycling, traveling or other outdoors. Good gift choice for you or your family member. A warm hearted love to Father, husband or son in this thanksgiving or Christmas Day.",
"image": "https://fakestoreapi.com/img/71li-ujtlUL._AC_UX679_.jpg",
"quantity": 12,
  "shipping": 0,
  "categoryIds": [
    "da0fe9ea-30fb-4cfd-b6ce-2dc7490d0d71"
  ]
}

{
"name": "Mens Casual Slim Fit",
"price": 15.99,
"description": "The color could be slightly different between on the screen and in practice. / Please note that body builds vary by person, therefore, detailed size information should be reviewed below on the product description.",
"image": "https://fakestoreapi.com/img/71YXzeOuslL._AC_UY879_.jpg",
"quantity": 12,
"shipping": 0,
"categoryIds": [
    "da0fe9ea-30fb-4cfd-b6ce-2dc7490d0d71"
  ]
}



{
    "name": "WD 2TB Elements Portable External Hard Drive - USB 3.0 ",
    "price": 64,
    "description": "USB 3.0 and USB 2.0 Compatibility Fast data transfers Improve PC Performance High Capacity; Compatibility Formatted NTFS for Windows 10, Windows 8.1, Windows 7; Reformatting may be required for other operating systems; Compatibility may vary depending on user’s hardware configuration and operating system",
    "image": "https://fakestoreapi.com/img/61IBBVJvSDL._AC_SY879_.jpg",
    "quantity": 12,
    "shipping": 0,
    "categoryIds": [
    "c4e6c881-507f-4db3-a3cf-b1cf3ed613b2"
    ]
}
{

    "name": "SanDisk SSD PLUS 1TB Internal SSD - SATA III 6 Gb/s",
    "price": 109,
    "description": "Easy upgrade for faster boot up, shutdown, application load and response (As compared to 5400 RPM SATA 2.5” hard drive; Based on published specifications and internal benchmarking tests using PCMark vantage scores) Boosts burst write performance, making it ideal for typical PC workloads The perfect balance of performance and reliability Read/write speeds of up to 535MB/s/450MB/s (Based on internal testing; Performance may vary depending upon drive capacity, host device, OS and application.)",
    "image": "https://fakestoreapi.com/img/61U7T1koQqL._AC_SX679_.jpg",
    "quantity": 10,
    "shipping": 3.5,
    "categoryIds": [
    "c4e6c881-507f-4db3-a3cf-b1cf3ed613b2"
    ]
}

{
    "name": "Silicon Power 256GB SSD 3D NAND A55 SLC Cache Performance Boost SATA III 2.5",
    "price": 109,
    "description": "3D NAND flash are applied to deliver high transfer speeds Remarkable transfer speeds that enable faster bootup and improved overall system performance. The advanced SLC Cache Technology allows performance boost and longer lifespan 7mm slim design suitable for Ultrabooks and Ultra-slim notebooks. Supports TRIM command, Garbage Collection technology, RAID, and ECC (Error Checking & Correction) to provide the optimized performance and enhanced reliability.",
    "image": "https://fakestoreapi.com/img/71kWymZ+c+L._AC_SX679_.jpg",
    "quantity": 22,
    "shipping": 2.5,
    "categoryIds": [
    "c4e6c881-507f-4db3-a3cf-b1cf3ed613b2"
    ]
}

{
    "name": "High-Speed Blender",
    "slug": "high-speed-blender",
    "image": "https://example.com/images/high_speed_blender.jpg",
    "description": "A versatile blender perfect for smoothies and more.",
    "price": 99.99,
    "quantity": 60,
    "sold": 15,
    "shipping": 12.00,
    "categoryIds": ["c4e6c881-507f-4db3-a3cf-b1cf3ed613b2"],
    "createdAt": "2024-05-16T12:15:00Z"
}

{
    "name": "Robotic Vacuum Cleaner",
    "slug": "robotic-vacuum-cleaner",
    "image": "https://example.com/images/robotic_vacuum_cleaner.jpg",
    "description": "An efficient robotic vacuum cleaner to keep your home spotless.",
    "price": 299.99,
    "quantity": 40,
    "sold": 10,
    "shipping": 20.00,
    "categoryIds": ["c4e6c881-507f-4db3-a3cf-b1cf3ed613b2"],
    "createdAt": "2024-05-16T12:30:00Z"
}



{
    "name": "Mystery Novel",
    "slug": "mystery-novel",
    "image": "https://example.com/images/mystery_novel.jpg",
    "description": "An intriguing mystery novel that keeps you on the edge of your seat.",
    "price": 19.99,
    "quantity": 120,
    "sold": 40,
    "shipping": 3.00,
    "categoryIds": ["f3a38e8f-7b24-4b1b-a110-8509a18e9e9e"],
    "createdAt": "2024-05-16T11:45:00Z"
}

{
    "name": "Gourmet Cookbook",
    "slug": "gourmet-cookbook",
    "image": "https://example.com/images/gourmet_cookbook.jpg",
    "description": "A cookbook filled with gourmet recipes for all occasions.",
    "price": 25.99,
    "quantity": 80,
    "sold": 25,
    "shipping": 4.00,
    "categoryIds": ["f3a38e8f-7b24-4b1b-a110-8509a18e9e9e"],
    "createdAt": "2024-05-16T12:00:00Z"
}


```

### 50. Product API => GET => /api/ products => get all products

```csharp
// Services
public async Task<PaginatedResult<ProductDto>> GetAllProductsAsync(QueryParameters queryParams)
  {
    var query = _appDbcontext.Products.Include(p => p.Categories).AsQueryable();

    if (!string.IsNullOrEmpty(queryParams.SearchTerm))
    {
      query = query.Where(p => p.Name.Contains(queryParams.SearchTerm) || p.Description.Contains(queryParams.SearchTerm));
    }

    if (!string.IsNullOrEmpty(queryParams.SortBy))
    {
      query = queryParams.SortOrder == "desc"
          ? query.OrderByDescending(u => EF.Property<object>(u, queryParams.SortBy))
          : query.OrderBy(u => EF.Property<object>(u, queryParams.SortBy));
    }

    var totalCount = await query.CountAsync();
    var products = await query
        .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
        .Take(queryParams.PageSize)
        .ToListAsync();
    var productDtos = _mapper.Map<List<ProductDto>>(products);

    return new PaginatedResult<ProductDto>
    {
      Items = productDtos,
      TotalCount = totalCount,
      PageNumber = queryParams.PageNumber,
      PageSize = queryParams.PageSize
    };
  }
// Controllers
[HttpGet]
public async Task<IActionResult> GetAllUsers([FromQuery] QueryParameters queryParams)
{
    var result = await _productService.GetAllProductsAsync(queryParams);
    return result.Items.Any()
        ? ApiResponse.Success(result, "Products retrieved successfully.")
        : ApiResponse.NotFound("No products found.");
}
```

### 51. Product API => GET => /api/products/{identifier} => get a product by id or slug

```csharp
// services
public async Task<ProductDto?> GetProductByIdentifierAsync(string identifier)
{
    Product? product = null;

    if (Guid.TryParse(identifier, out Guid productId))
    {
        product = await _appDbcontext.Products
            .Include(p => p.Categories) // Include related categories
            .SingleOrDefaultAsync(p => p.ProductId == productId);
    }
    else
    {
        product = await _appDbcontext.Products
            .Include(p => p.Categories) // Include related categories
            .SingleOrDefaultAsync(p => p.Slug == identifier);
    }

    return product != null ? _mapper.Map<ProductDto>(product) : null;
}


// controllers
[HttpGet("{identifier}")]
public async Task<IActionResult> GetProductByIdentifier(string identifier)
{
    var product = await _productService.GetProductByIdentifierAsync(identifier);
    if (product == null)
    {
        return ApiResponse.NotFound("product not found");
    }
    return ApiResponse.Success(product, "product retrieved successfully");
}

```

### 52. Product API => DELETE => /api/products/{identifier} => delete a product by id or slug

```csharp
// Service
 public async Task<bool> DeleteProductByIdentifierAsync(string identifier)
{
    Product? product = null;

    if (Guid.TryParse(identifier, out Guid productId))
    {
        product = await _appDbcontext.Products.FindAsync(productId);
    }
    else
    {
        product = await _appDbcontext.Products.SingleOrDefaultAsync(p => p.Slug == identifier);
    }

    if (product == null)
    {
        return false;
    }

    _appDbcontext.Products.Remove(product);
    await _appDbcontext.SaveChangesAsync();
    return true;
}

// Controller
[HttpDelete("{identifier}")]
public async Task<IActionResult> DeleteProductByIdentifier(string identifier)
{
    if (!ModelState.IsValid)
    {
        return ApiResponse.BadRequest("Invalid product data provided.");
    }

    var result = await _productService.DeleteProductByIdentifierAsync(identifier);
    return result ? NoContent() : ApiResponse.NotFound($"Product with identifier {identifier} not found.");
}
```

### 53. Product API => PUT => /api/products/{identifier} => update a product by id or slug

```csharp
// create a dto first: UpdateProductDto
namespace api.Models
{
  public class UpdateProductDto
  {
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public int Sold { get; set; }
    public decimal Shipping { get; set; }
    public List<Guid> CategoryIds { get; set; } = new List<Guid>();
    public string Image { get; set; } = string.Empty;
  }
}

// add to the mapper
 CreateMap<UpdateProductDto, Product>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<UpdateProductDto, Product>()
                .ForMember(dest => dest.Categories, opt => opt.Ignore());


// create the service
public async Task<ProductDto?> UpdateProductByIdentifierAsync(string identifier, UpdateProductDto updateProductData)
{
    Product? product;

    if (Guid.TryParse(identifier, out Guid productId))
    {
        product = await _appDbcontext.Products.FindAsync(productId);
    }
    else
    {
        product = await _appDbcontext.Products.SingleOrDefaultAsync(p => p.Slug == identifier);
    }

    if (product == null)
    {
        return null;
    }

    _mapper.Map(updateProductData, product);

    // Regenerate the slug based on the updated name or other criteria
    product.Slug = Helper.GenerateSlug(product.Name);

    // Update categories
    var categories = await _appDbcontext.Categories
        .Where(c => updateProductData.CategoryIds.Contains(c.CategoryId))
        .ToListAsync();

    product.Categories.Clear();
    product.Categories.AddRange(categories);

    _appDbcontext.Products.Update(product);
    await _appDbcontext.SaveChangesAsync();
    return _mapper.Map<ProductDto>(product);
}


// create the controller
[Authorize(Roles = "Admin")]
[HttpPut("{identifier}")]
public async Task<IActionResult> UpdateProductByIdentifier(string identifier, [FromBody] UpdateProductDto updateProductDto)
{
    if (!ModelState.IsValid)
    {
        return ApiResponse.BadRequest("Invalid product data provided.");
    }

    var updatedProduct = await _productService.UpdateProductByIdentifierAsync(identifier, updateProductDto);

    return updatedProduct != null
        ? ApiResponse.Success(updatedProduct, "Product updated successfully.")
        : ApiResponse.NotFound($"Product with identifier '{identifier}' not found.");
}

```

### Front-end

### setup all the basics

### setup category slice

### setup products slice and Products, SingleProduct Component

```ts
// types/index.ts
export type Category = {
  categoryId: string;
  name: string;
  slug: string;
  description: string;
  products: Product[];
  createdAt: string;
};

export type Product = {
  productId: string;
  name: string;
  slug: string;
  image: string;
  description: string;
  price: number;
  quantity: number;
  sold: number;
  shipping: number;
  categories: Category[];
  createdAt: string;
};

// ProductSlice.tsx
import api from '@/api';
import { Product } from '@/types';
import { createAsyncThunk, createSlice } from '@reduxjs/toolkit';

export type ProductState = {
  products: Product[];
  error: null | string;
  isLoading: boolean;
};
const initialState: ProductState = {
  products: [],
  error: null,
  isLoading: false,
};

export const fetchProducts = createAsyncThunk(
  'products/fetchProducts',
  async () => {
    const response = await api.get(`/products`);
    console.log(response.data.data.items);
    return response.data;
  }
);

const productSlice = createSlice({
  name: 'products',
  initialState: initialState,
  reducers: {},
  extraReducers(builder) {
    builder.addCase(fetchProducts.pending, (state) => {
      state.isLoading = true;
      state.error = null;
    });
    builder.addCase(fetchProducts.fulfilled, (state, action) => {
      // console.log(action.payload)
      state.products = action.payload.data.items;
      state.isLoading = false;
    });
    builder.addCase(fetchProducts.rejected, (state, action) => {
      state.isLoading = false;
      state.error = action.error.message || 'an error occured';
    });
  },
});

export default productSlice.reducer;

// Products.tsx
import React, { useEffect, useState } from 'react';
import SingleProduct from './SingleProduct';
import { useDispatch, useSelector } from 'react-redux';
import { AppDispatch, RootState } from '@/tookit/store';
import { fetchProducts } from '@/tookit/slices/productSlice';

const Products = () => {
  // access store for all the products

  const { products, isLoading, error } = useSelector(
    (state: RootState) => state.productR
  );

  const dispatch: AppDispatch = useDispatch();

  useEffect(() => {
    const fetchData = async () => {
      await dispatch(fetchProducts());
    };
    fetchData();
  }, []);
  return (
    <div>
      <h1>Products</h1>
      {isLoading && <p>Loading...</p>}
      {error && <p>Error: {error}</p>}
      {products &&
        products.length > 1 &&
        products.map((product) => (
          <SingleProduct key={product.productId} product={product} />
        ))}
    </div>
  );
};

export default Products;

// SingleProduct component
import React from 'react';
import { Product } from '@/types'; // Assuming you have a types file where Product is defined

const SingleProduct = (props: { product: Product }) => {
  const { product } = props;
  return (
    <article className="product card">
      <img src={product.image} alt={product.name} className="product__img" />
      <div className="product__body">
        <h2 className="product__name">{product.name}</h2>
        <p className="product__details">
          {product.description.substring(0, 100)}...
        </p>
        <p className="product__price">
          Price:
          {product.price.toLocaleString('en-US', {
            style: 'currency',
            currency: 'USD',
          })}
        </p>
        <p>Quantity: {product.quantity}</p>
        <p>Sold: {product.sold}</p>
        <p>Shipping: ${product.shipping.toFixed(2)}</p>
        {/* <p>Categories: {product.categories.map((category) => category.name).join(", ")}</p> */}
        <p>Created At: {new Date(product.createdAt).toLocaleDateString()}</p>

        <div>
          <button className="btn product__btn">
            View More <i className="fa fa-eye" aria-hidden="true"></i>
          </button>
          <button className="btn product__btn">
            {' '}
            Add To Cart <i className="fa fa-shopping-cart"></i>
          </button>
        </div>
      </div>
    </article>
  );
};

export default SingleProduct;
```

### add product styling

```css
/* product starts here  */
.products {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 2rem;
  padding: 2rem 0;
  overflow: scroll;
}
.product {
  position: relative;
}

.badge {
  background-color: green;
  position: absolute;
  top: 0;
  left: 0;
  color: white;
  padding: 4px 8px;
  text-align: center;
  border-radius: 5px;
  z-index: 10;
}
.product__img {
  width: 100%;
  height: 15rem;
  filter: saturate(0);
  transition: var(--transition);
}
.product__img:hover {
  filter: saturate(1);
}
.product__body {
  padding: var(--padding);
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}
.learn-more {
  color: blue;
}
.product__btn {
  margin-right: 0.5rem;
}
.load-btn {
  margin: 1rem 0;
}

.badge1 {
  background-color: red;
  position: absolute;
  top: 0;
  left: 0;
  color: white;
  padding: 1px 1px;
  text-align: center;
  border-radius: 5px;
}

.badge2 {
  background-color: blue;
  position: absolute;
  top: 0;
  right: 0;
  color: white;
  padding: 4px 8px;
  text-align: center;
  border-radius: 5px;
}
/* product ends here  */

/* product details starts */

.product-details__left {
  flex: 1;
}
.product-details__left img {
  height: 20em;
  width: 30rem;
}
.product-details__right {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}
/* product details ends */

@media (max-width: 992px) {
  .products {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}
@media (max-width: 768px) {
  .products {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}
@media (max-width: 600px) {
  .products {
    grid-template-columns: repeat(1, minmax(0, 1fr));
  }
}
```

### add Badge component for the product

```js
// step 1: Badge.js component
import React from 'react';

const Badge = ({ quantity, sold }: { quantity: number; sold: number }) => {
  return (
    <div className="product-badge">
      In stock: {quantity - sold > 0 ? quantity - sold : "Out of stock"}
    </div>
  )
}

export default Badge

export default Badge;

// step 2: add badge styling
.product {
  position: relative;
}
.product-badge {
  background-color: green;
  position: absolute;
  top: 0;
  left: -15px;
  color: white;
  padding: 4px 8px;
  text-align: center;
  border-radius: 5px;
  z-index: 10;
}

// step 3: add badge component to the Product component
  <Badge quantity={product.quantity} sold={product.sold} />
```

### Add Product Details page

- create a product detils page

- handle the view more click

- add the route

```tsx
// index.ts (route)
<Route path="/products/:id" element={<ProductDetails />} />
```

```js
// wrap the button inside the Link
<Link to={`/products/${product.id}`}>
  <button className="btn product__btn">
    View More <i className="fa fa-eye" aria-hidden="true"></i>
  </button>
</Link>
```

- update the slice for the api call

```js
import api from "@/api"
import { Product } from "@/types"
import { createAsyncThunk, createSlice } from "@reduxjs/toolkit"

export type ProductState = {
  products: Product[]
  product: Product | null
  error: null | string
  isLoading: boolean
}

const initialState: ProductState = {
  products: [],
  product: null,
  error: null,
  isLoading: false
}

export const fetchProducts = createAsyncThunk(
  "products/fetchProducts",
  async () => {
    const response = await api.get(`/products`)
    console.log(response.data.data.items)
    return response.data
  }
)

export const fetchProductById = createAsyncThunk(
  "products/fetchProductById",
  async (productId: string) => {
    const response = await api.get(`/products/${productId}`)
    return response.data
  }
)

const productSlice = createSlice({
  name: "products",
  initialState: initialState,
  reducers: {},
  extraReducers(builder) {
    builder.addCase(fetchProducts.pending, (state) => {
      state.isLoading = true
      state.error = null
    })
    builder.addCase(fetchProducts.fulfilled, (state, action) => {
      state.products = action.payload.data.items
      state.isLoading = false
    })
    builder.addCase(fetchProducts.rejected, (state, action) => {
      state.isLoading = false
      state.error = action.error.message || "An error occurred"
    })
    builder.addCase(fetchProductById.pending, (state) => {
      state.isLoading = true
      state.error = null
    })
    builder.addCase(fetchProductById.fulfilled, (state, action) => {
      state.product = action.payload
      state.isLoading = false
    })
    builder.addCase(fetchProductById.rejected, (state, action) => {
      state.isLoading = false
      state.error = action.error.message || "An error occurred"
    })
  }
})

export default productSlice.reducer

```

- display the data in productdetails page

```js
import React, { useEffect } from "react"
import { useDispatch, useSelector } from "react-redux"
import { useParams } from "react-router-dom"
import { fetchProductById } from "@/tookit/slices/productSlice"
import { AppDispatch, RootState } from "@/tookit/store"
import { Product } from "@/types" // Assuming you have a types file where Product is defined

const ProductDetails: React.FC = () => {
  const { productId } = useParams<{ productId: string }>()
  const dispatch: AppDispatch = useDispatch()

  const { product, isLoading, error } = useSelector((state: RootState) => state.productR)

  useEffect(() => {
    dispatch(fetchProductById(productId))
  }, [dispatch, productId])

  if (isLoading) return <div>Loading...</div>
  if (error) return <div>{error}</div>
  if (!product) return <div>No product found</div>

  return (
    <article className="product-details">
      <img src={product.image} alt={product.name} className="product-details__img" />
      <div className="product-details__body">
        <h2 className="product-details__name">{product.name}</h2>
        <p className="product-details__description">{product.description}</p>
        <p className="product-details__price">
          Price:
          {product.price.toLocaleString("en-US", {
            style: "currency",
            currency: "USD",
          })}
        </p>
        <p>Quantity: {product.quantity}</p>
        <p>Sold: {product.sold}</p>
        <p>Shipping: ${product.shipping.toFixed(2)}</p>
        <p>Categories: {product.categories.map((category) => category.name).join(", ")}</p>
        <p>Created At: {new Date(product.createdAt).toLocaleDateString()}</p>
        <p>Categories: {product.categories.map((category) => category.name).join(", ")}</p>
      </div>
    </article>
  )
}

export default ProductDetails

```

### use matcher in slice

```ts
import api from '@/api';
import { CustomError, LoginFormData, User, UserState } from '@/types';
import { PayloadAction, createAsyncThunk, createSlice } from '@reduxjs/toolkit';

const data =
  localStorage.getItem('loginData') !== null
    ? JSON.parse(String(localStorage.getItem('loginData')))
    : [];

const initialState: UserState = {
  error: null,
  isLoading: false,
  userData: data.userData,
  token: data.token,
  isLoggedIn: data.isLoggedIn,
};

export const registerUser = createAsyncThunk(
  'users/registerUser',
  async (newUser: User) => {
    const response = await api.post('/users', newUser);
    return response.data;
  }
);

export const loginUser = createAsyncThunk(
  'users/loginUser',
  async (userData: LoginFormData) => {
    const response = await api.post('/users/login', userData);
    return response.data;
  }
);

// cases: pending, fullfilled, rejected
const userSlice = createSlice({
  name: 'users',
  initialState: initialState,
  reducers: {
    logoutUser: (state) => {
      state.isLoggedIn = false;
      state.userData = null;
      state.token = null;
      localStorage.setItem(
        'loginData',
        JSON.stringify({
          isLoggedIn: state.isLoggedIn,
          userData: state.userData,
          token: state.token,
        })
      );
    },
  },
  extraReducers(builder) {
    builder.addCase(loginUser.fulfilled, (state, action) => {
      state.isLoggedIn = true;
      state.userData = action.payload.data.user;
      state.token = action.payload.data.token;
      localStorage.setItem(
        'loginData',
        JSON.stringify({
          isLoggedIn: state.isLoggedIn,
          userData: state.userData,
          token: state.token,
        })
      );
    });

    builder.addMatcher(
      (action) => action.type.endsWith('/pending'),
      (state) => {
        state.error = null;
        state.isLoading = true;
      }
    );

    builder.addMatcher(
      (action) => action.type.endsWith('/rejected'),
      (state, action: PayloadAction<CustomError>) => {
        state.error = action.payload?.message || 'An error occured';
        state.isLoading = false;
      }
    );
  },
});

export const { logoutUser } = userSlice.actions;
export default userSlice.reducer;
```

### Pagination

```js
// Products.tsx
const [pageNumber, setPageNumber] = useState(1)
const [pageSize, setPageSize] = useState(3)

// pass them as a parameters
  useEffect(() => {
    const fetchData = async () => {
      await dispatch(fetchProducts({ pageNumber, pageSize }))
    }
    fetchData()
  }, [pageNumber])

// receieve them in slice
export const fetchProducts = createAsyncThunk(
  "products/fetchProducts",
  async ({
    pageNumber,
    pageSize,
    searchTerm,
    sortBy
  }: {
    pageNumber: number
    pageSize: number
    searchTerm: string
    sortBy: string
  }) => {
    const response = await api.get(`/products?pageNumber=${pageNumber}&pageSize=${pageSize}`)
    return response.data
  }
)

// have the value for totalPages
const initialState: ProductState = {
  products: [],
  totalPages: 1,
  product: null,
  error: null,
  isLoading: false
}

// update the fulfilled state
builder.addCase(fetchProducts.fulfilled, (state, action) => {
      state.products = action.payload.data.items
      state.totalPages = action.payload.data.totalPages
      state.isLoading = false
    })


// in products.tsx page
  const handlePreviousPage = () => {
    setPageNumber((currentPage) => currentPage - 1)
  }
  const handleNextPage = () => {
    setPageNumber((currentPage) => currentPage + 1)
  }
<div className="pagination">
<button onClick={handlePreviousPage} disabled={pageNumber === 1}>
    Previous
</button>
{Array.from({ length: totalPages }, (_, index) => (
    <button key={index} onClick={() => setPageNumber(index + 1)}>
    {index + 1}
    </button>
))}
<button onClick={handleNextPage} disabled={pageNumber === totalPages}>
    Next
</button>
```

### Searching

```js
// Products.tsx
 const [searchTerm, setSearchTerm] = useState("")


useEffect(() => {
const fetchData = async () => {
    await dispatch(fetchProducts({ pageNumber, pageSize, searchTerm }))
}
fetchData()
}, [pageNumber, searchTerm])


 // productSlice.tsx
export const fetchProducts = createAsyncThunk(
  "products/fetchProducts",
  async ({
    pageNumber,
    pageSize,
    searchTerm,
    sortBy
  }: {
    pageNumber: number
    pageSize: number
    searchTerm: string
    sortBy: string
  }) => {
    const response =
      searchTerm.length > 0
        ? await api.get(
            `/products?pageNumber=${pageNumber}&pageSize=${pageSize}&searchTerm=${searchTerm}`
          )
        : await api.get(`/products?pageNumber=${pageNumber}&pageSize=${pageSize}`)
    return response.data
  }
)

// add component for searching in Products.tsx
<div className="action flex-space-around">
        <div>
          <input
            type="text"
            placeholder="Search Products"
            value={searchTerm}
            onChange={handleSearchChange}
          />
        </div>


</div>

// changeEvent
 const handleSearchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setSearchTerm(e.target.value)
  }
```

### Sorting

```js
// Products.tsx
  const [sortBy, setSortBy] = useState("Name")
 useEffect(() => {
    const fetchData = async () => {
      await dispatch(fetchProducts({ pageNumber, pageSize, searchTerm, sortBy }))
    }
    fetchData()
  }, [pageNumber, searchTerm, sortBy])

    const handleSortChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    setSortBy(e.target.value)
  }

   <div className="action flex-space-around">
        <div>
          <input
            type="text"
            placeholder="Search Products"
            value={searchTerm}
            onChange={handleSearchChange}
          />
        </div>

        <div className="flex-center">
          <p>Sort By: </p>
          <select name="" id="" onChange={handleSortChange}>
            <option value="Name">Name</option>
            <option value="Price">Price</option>
          </select>
        </div>
      </div>


// productsSlice.tsx

export const fetchProducts = createAsyncThunk(
  "products/fetchProducts",
  async ({
    pageNumber,
    pageSize,
    searchTerm,
    sortBy
  }: {
    pageNumber: number
    pageSize: number
    searchTerm: string
    sortBy: string
  }) => {
    const response =
      searchTerm.length > 0
        ? await api.get(
            `/products?pageNumber=${pageNumber}&pageSize=${pageSize}&searchTerm=${searchTerm}&sortBy=${sortBy}`
          )
        : await api.get(`/products?pageNumber=${pageNumber}&pageSize=${pageSize}&sortBy=${sortBy}`)
    return response.data
  }
)
```

### Filtering

### Cart

### User Sign up & Login Page

- create a register and login page

- create the sign up and sign in route

```js
<Route path="/register" element={<Register />} />
<Route path="/login" element={<Login />} />
```

- add to the navbar please

```js
 <li>
    <Link className="nav__link" to="/register">
    Regisetr
    </Link>
</li>
<li>
    <Link className="nav__link" to="/login">
    Login
    </Link>
</li>
```

- add the types

```ts
export type User = {
  name: string;
  image?: string;
  address: string;
  email: string;
  password: string;
  isAdmin?: string;
  isBanned?: string;
  createdAt?: string;
};

export type UserState = {
  error: null | string;
  isLoading: boolean;
};

export type LoginFormData = {
  email: string;
  password: string;
};
```

- create the Register page

```js
import { registerUser } from "@/tookit/slices/userSlice"
import { AppDispatch } from "@/tookit/store"
import React from "react"
import { SubmitHandler, useForm } from "react-hook-form"
import { useDispatch } from "react-redux"
import { useNavigate } from "react-router-dom"
import { toast } from "react-toastify"


type FormData = {
  name: string
  email: string
  password: string
  image: string
  phone: string
  address: string
}

export const Register = () => {
  const navigate = useNavigate()
  const dispatch: AppDispatch = useDispatch()
  const {
    register,
    handleSubmit,
    formState: { errors }
  } = useForm<FormData>()

  const onSubmit: SubmitHandler<FormData> = async (data) => {
    try {
      const response = await dispatch(registerUser(data))
      console.log("Response from Register: " + response)
      toast.success(response.payload.message)
      navigate("/login")
    } catch (error) {
      console.error(error);
      toast.error("Login failed")
    }
  }

  return (
    <div className="register">
      <h2>User Registration</h2>
      <form onSubmit={handleSubmit(onSubmit)}>
        <div className="form-field">
          <label htmlFor="name"> Name: </label>
          <input
            type="text"
            {...register("name", {
              required: "Name is required",
              minLength: { value: 2, message: "Name must be at least 2 characters" }
            })}
          />
          {errors.name && <p>{errors.name.message}</p>}
        </div>
        <div className="form-field">
          <label htmlFor="name"> Email: </label>
          <input
            type="email"
            {...register("email", {
              required: "Email is required",
              pattern: { value: /^[^@ ]+@[^@ ]+\.[^@ .]{2,}$/, message: "Email is not valid" }
            })}
          />
          {errors.email && <p>{errors.email.message}</p>}
        </div>

        <div className="form-field">
          <label htmlFor="name"> password: </label>
          <input
            type="password"
            {...register("password", {
              required: "Password is required",
              minLength: { value: 6, message: "Pssword must be at least 6 characters" }
            })}
          />
          {errors.password && <p>{errors.password.message}</p>}
        </div>

        <div className="form-field">
          <label htmlFor="address"> Address: </label>
          <textarea id="" {...register("address")}></textarea>
        </div>

        <button className="btn" type="submit">
          Register
        </button>
      </form>
    </div>
  )
}

// we will add image -> for user register
// login function

```

- create the userSlice

```js
import api from '@/api';
import { LoginFormData, User, UserState } from '@/types';
import { createAsyncThunk, createSlice } from '@reduxjs/toolkit';

const initialState: UserState = {
  error: null,
  isLoading: false,
};

export const registerUser = createAsyncThunk(
  'users/registerUser',
  async (newUser: User) => {
    const response = await api.post('/users', newUser);
    return response.data;
  }
);

export const loginUser = createAsyncThunk(
  'users/registerUser',
  async (userData: LoginFormData) => {
    const response = await api.post('/users/login', userData);
    return response.data;
  }
);

// cases: pending, fullfilled, rejected
const userSlice = createSlice({
  name: 'users',
  initialState: initialState,
  reducers: {},
});

export default userSlice.reducer;
```

- sign in page

```js
import { loginUser, registerUser } from "@/tookit/slices/userSlice"
import { AppDispatch } from "@/tookit/store"
import { LoginFormData } from "@/types"
import React from "react"
import { SubmitHandler, useForm } from "react-hook-form"
import { useDispatch } from "react-redux"
import { useNavigate } from "react-router-dom"
import { toast } from "react-toastify"



export const Login = () => {
  const navigate = useNavigate()
  const dispatch: AppDispatch = useDispatch()
  const {
    register,
    handleSubmit,
    formState: { errors }
  } = useForm<LoginFormData>()

  const onSubmit: SubmitHandler<LoginFormData> = async (data) => {
    try {
      const response = await dispatch(loginUser(data))
      toast.success(response.payload.message)
      // navigate("/login")
    } catch (error: any) {
      toast.error(error.message || "Login failed")
    }
  }

  return (
    <div className="login">
      <h2>User Login</h2>
      <form onSubmit={handleSubmit(onSubmit)}>
        <div className="form-field">
          <label htmlFor="name"> Email: </label>
          <input
            type="email"
            {...register("email", {
              required: "Email is required",
              pattern: { value: /^[^@ ]+@[^@ ]+\.[^@ .]{2,}$/, message: "Email is not valid" }
            })}
          />
          {errors.email && <p>{errors.email.message}</p>}
        </div>

        <div className="form-field">
          <label htmlFor="name"> password: </label>
          <input
            type="password"
            {...register("password", {
              required: "Password is required",
              minLength: { value: 6, message: "Pssword must be at least 6 characters" }
            })}
          />
          {errors.password && <p>{errors.password.message}</p>}
        </div>

        <button className="btn" type="submit">
          Login
        </button>
      </form>
    </div>
  )
}

```

### add image for user sign up && cloudinary url for storing image

- create the image input field
- show the preview
- register in cloudinary, set upload preset with unsigned => settings => upload

```tsx
// Register.tsx
import { registerUser } from '@/tookit/slices/userSlice';
import { AppDispatch } from '@/tookit/store';
import { uploadImageToCloudinary } from '@/utils/cloudinary';
import React, { useState } from 'react';
import { SubmitHandler, useForm } from 'react-hook-form';
import { useDispatch } from 'react-redux';
import { useNavigate } from 'react-router-dom';
import { toast } from 'react-toastify';

type FormData = {
  name: string;
  email: string;
  password: string;
  image: FileList;
  phone: string;
  address: string;
};

export const Register = () => {
  const navigate = useNavigate();
  const dispatch: AppDispatch = useDispatch();
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<FormData>();

  const [imagePreview, setImagePreview] = useState<string | null>(null);

  const onSubmit: SubmitHandler<FormData> = async (data) => {
    try {
      let imageUrl = '';
      if (data.image && data.image.length > 0) {
        const file = data.image[0];

        imageUrl = await uploadImageToCloudinary(file);
        console.log(imageUrl);
      }
      const userData = {
        ...data,
        image: imageUrl,
      };
      const response = await dispatch(registerUser(userData));
      console.log('Response from Register: ' + response);
      toast.success(response.payload.message);
      navigate('/login');
    } catch (error: any) {
      toast.error(error.message || 'Registration failed');
    }
  };

  const handleImageChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (file) {
      setImagePreview(URL.createObjectURL(file));
    }
  };

  return (
    <div className="register">
      <h2>User Registration</h2>
      <form onSubmit={handleSubmit(onSubmit)}>
        <div className="form-field">
          <label htmlFor="name"> Name: </label>
          <input
            type="text"
            {...register('name', {
              required: 'Name is required',
              minLength: {
                value: 2,
                message: 'Name must be at least 2 characters',
              },
            })}
          />
          {errors.name && <p>{errors.name.message}</p>}
        </div>
        <div className="form-field">
          <label htmlFor="name"> Email: </label>
          <input
            type="email"
            {...register('email', {
              required: 'Email is required',
              pattern: {
                value: /^[^@ ]+@[^@ ]+\.[^@ .]{2,}$/,
                message: 'Email is not valid',
              },
            })}
          />
          {errors.email && <p>{errors.email.message}</p>}
        </div>

        <div className="form-field">
          <label htmlFor="name"> password: </label>
          <input
            type="password"
            {...register('password', {
              required: 'Password is required',
              minLength: {
                value: 6,
                message: 'Pssword must be at least 6 characters',
              },
            })}
          />
          {errors.password && <p>{errors.password.message}</p>}
        </div>

        <div className="form-field">
          <label htmlFor="address"> Address: </label>
          <textarea id="" {...register('address')}></textarea>
        </div>

        <div className="form-field">
          <label htmlFor="image"> Image: </label>
          <input
            type="file"
            accept="image/*"
            {...register('image')}
            onChange={handleImageChange}
          />
          {imagePreview && (
            <img
              src={imagePreview}
              alt="Image Preview"
              className="image-preview"
            />
          )}
        </div>

        <button className="btn" type="submit">
          Register
        </button>
      </form>
    </div>
  );
};

// we will add image -> for user register
// login function

// utils/cloudinary.ts
export const uploadImageToCloudinary = async (file: File): Promise<string> => {
  const formData = new FormData();
  formData.append('file', file);
  formData.append('upload_preset', 'ulifygv2'); // Ensure you have set up an upload preset in Cloudinary
  formData.append('folder', 'e-commerce-sda2'); // Specify the folder where you want to store the image

  try {
    const response = await fetch(
      `https://api.cloudinary.com/v1_1/anisul-cloud/image/upload`,
      {
        method: 'POST',
        body: formData,
      }
    );

    if (!response.ok) {
      throw new Error('Failed to upload image');
    }

    const data = await response.json();
    return data.secure_url; // Return the secure URL of the uploaded image
  } catch (error) {
    console.error('Error uploading image to Cloudinary:', error);
    throw error;
  }
};
```

### UserSlice for storing user info after login

- if the user login is successful then add user data, login status and token in the state

```tsx
// update types
export type UserState = {
  error: null | string
  isLoading: boolean
  isLoggedIn: boolean
  userData: User | null
  token: string | null
}

// userSlice
// set the data in the localstorage
const data =
  localStorage.getItem("loginData") !== null
    ? JSON.parse(String(localStorage.getItem("loginData")))
    : []

const initialState: UserState = {
  error: null,
  isLoading: false,
  isLoggedIn: data.isLoggedIn,
  userData: data.userData,
  token: data.token
}

export const loginUser = createAsyncThunk("users/loginUser", async (userData: LoginFormData) => {
  const response = await api.post("/users/login", userData)
  return response.data
})
 extraReducers(builder) {
    builder.addCase(loginUser.fulfilled, (state, action) => {
      state.isLoggedIn = true
      state.userData = action.payload.data.user
      state.token = action.payload.data.token
      localStorage.setItem(
        "loginData",
        JSON.stringify({
          isLoggedIn: state.isLoggedIn,
          userData: state.userData,
          token: state.token
        })
      )
      state.isLoading = false
    })
  }

```

### logout

```tsx
// update state and localstorage
reducers: {
    logoutUser: (state) => {
      state.isLoggedIn = false
      state.userData = null
      state.token = null
      localStorage.setItem(
        "loginData",
        JSON.stringify({
          isLoggedIn: state.isLoggedIn,
          userData: state.userData,
          token: state.token
        })
      )
      state.isLoading = false
    }
  },

  export const {logoutUser} = userSlice.actions

// add logout option in the navbar and dispatch the logout option
 <li>
  <Link className="nav__link" to="/" onClick={handleLogout}>
    Logout
  </Link>
</li>

const dispatch: AppDispatch = useDispatch()

const handleLogout = () => {
  dispatch(logoutUser())
}
```

### create admin and user dashboard with all the pages

```tsx

```

### Protected routing based on login and admin status

```tsx
// routes/index.ts
import { BrowserRouter, Routes, Route } from 'react-router-dom';

import {
  AdminDashboard,
  Categories,
  Contact,
  Error,
  Home,
  Login,
  Orders,
  Products,
  Register,
  UserDashboard,
  UserOrders,
  UserProfile,
  Users,
} from '@/pages';
import Navbar from '@/components/layout/Navbar';
import Footer from '@/components/layout/Footer';
import { ProductDetails } from '@/pages/ProductDetails';
import ProtectedRoute from './ProtectedRoute';
import AdminRoute from './AdminRoute';

const Index = () => {
  return (
    <BrowserRouter>
      <Navbar />
      <main>
        <Routes>
          <Route path="/" element={<Home />} />
          <Route path="/contact" element={<Contact />} />
          <Route path="/products/:slug" element={<ProductDetails />} />
          <Route path="/register" element={<Register />} />
          <Route path="/login" element={<Login />} />

          <Route path="/dashboard" element={<ProtectedRoute />}>
            <Route path="user" element={<UserDashboard />} />
            <Route path="user/profile" element={<UserProfile />} />
            <Route path="user/orders" element={<UserOrders />} />
          </Route>

          <Route path="/dashboard" element={<AdminRoute />}>
            <Route path="admin" element={<AdminDashboard />} />
            <Route path="admin/categories" element={<Categories />} />
            <Route path="admin/products" element={<Products />} />
            <Route path="admin/users" element={<Users />} />
            <Route path="admin/orders" element={<Orders />} />
          </Route>

          <Route path="*" element={<Error />} />
        </Routes>
      </main>
      <Footer />
    </BrowserRouter>
  );
};

export default Index;

// routes/ProtectedRoute.tsx
import { Login } from '@/pages';
import { RootState } from '@/tookit/store';
import React from 'react';
import { useSelector } from 'react-redux';
import { Outlet } from 'react-router-dom';

const ProtectedRoute = () => {
  const { isLoggedIn } = useSelector((state: RootState) => state.userR);
  return isLoggedIn ? <Outlet /> : <Login />;
};

export default ProtectedRoute;

// routes/AdminRoute.tsx
import { Login } from '@/pages';
import { RootState } from '@/tookit/store';
import React from 'react';
import { useSelector } from 'react-redux';
import { Outlet } from 'react-router-dom';

const AdminRoute = () => {
  const { isLoggedIn, userData } = useSelector(
    (state: RootState) => state.userR
  );
  return isLoggedIn && userData?.isAdmin ? <Outlet /> : <Login />;
};

export default AdminRoute;
```

### add a custom hook for selector

```tsx
import { useSelector } from 'react-redux';
import { RootState } from 'your-redux-store-path'; // Replace with the actual path

export function useUserState() {
  const { users, isLoading, error, searchTerm } = useSelector(
    (state: RootState) => state.usersR
  );

  return { users, isLoading, error, searchTerm };
}

import React from 'react';
import { useUserState } from './useUserState'; // Replace with the actual path

function YourComponent() {
  const { users, isLoading, error, searchTerm } = useUserState();

  // Your component logic using these values
}
```

### add dashboard to the navbar component

```tsx
{
  isLoggedIn && (
    <>
      <li>
        <Link to="/logout" onClick={handleLogout} className="nav__link">
          Logout
        </Link>
      </li>

      <li>
        <Link
          to={`/dashboard/${userData && userData.isAdmin ? 'admin' : 'user'}`}
          className="nav__link"
        >
          {userData && userData.isAdmin ? 'admin' : 'user'} Dashboard
        </Link>
      </li>
    </>
  );
}
```

### User Dashboard: User Profile

```tsx
// create the profile section
// add a form for editing profile
// get the form data after submission
// show the form only when necessary
import UserSidebar from '@/components/UserSidebar';
import useUsersState from '@/hooks/useUsersState';
import { updateUser } from '@/tookit/slices/userSlice';
import { AppDispatch } from '@/tookit/store';
import { ProfileFormData } from '@/types';
import React, { useState } from 'react';
import { SubmitHandler, useForm } from 'react-hook-form';
import { useDispatch } from 'react-redux';
import { toast } from 'react-toastify';

export const UserProfile = () => {
  const { userData } = useUsersState();
  const dispatch: AppDispatch = useDispatch();

  const [isFormOpen, setIsFormOpen] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<ProfileFormData>();

  const onSubmit: SubmitHandler<ProfileFormData> = async (data) => {
    if (!userData?.userId) {
      toast.error('User data is not available');
      return;
    }

    try {
      const response = await dispatch(
        updateUser({ updateUserData: data, userId: userData.userId })
      );
      console.log('Response from Update: ', response);
      // toast.success(response.payload.message)
    } catch (error) {
      console.log(error);
      toast.error('Update failed');
    }
  };

  return (
    <div className="container flex-space-around">
      <UserSidebar />
      <div className="main-container">
        {userData && (
          <>
            <img
              src={userData.image}
              alt="profile image"
              className="round-image"
            />
            <h3>Name: {userData.name}</h3>
            <p>Email: {userData.email}</p>
            <p>Address: {userData.address}</p>
            <p>Role: {userData.isAdmin ? 'Admin' : 'User'}</p>
            <p>
              Account Created:{' '}
              {userData.createdAt &&
                new Date(userData.createdAt).toLocaleDateString()}
            </p>
            <button
              className="btn"
              onClick={() => {
                setIsFormOpen(!isFormOpen);
              }}
            >
              {isFormOpen ? 'Close Edit Profile' : 'Edit Profile'}
            </button>
          </>
        )}

        {/* add edit profile form  */}
        {isFormOpen && (
          <form onSubmit={handleSubmit(onSubmit)}>
            <div className="form-field">
              <label htmlFor="name"> Name: </label>
              <input
                type="text"
                {...register('name', {
                  required: 'Name is required',
                  minLength: {
                    value: 2,
                    message: 'Name must be at least 2 characters',
                  },
                })}
              />
              {errors.name && <p>{errors.name.message}</p>}
            </div>

            <div className="form-field">
              <label htmlFor="address"> Address: </label>
              <textarea id="" {...register('address')}></textarea>
            </div>

            <button className="btn" type="submit">
              Update Profile
            </button>
          </form>
        )}
      </div>
    </div>
  );
};

// userSlice -> dispacth an action to make an api call
export const updateUser = createAsyncThunk(
  'users/updateUser',
  async ({
    updateUserData,
    userId,
  }: {
    updateUserData: ProfileFormData;
    userId: string;
  }) => {
    const response = await api.put(`/users/${userId}`, updateUserData);
    return response.data;
  }
);

// update the userData state
builder.addCase(updateUser.fulfilled, (state, action) => {
  const updatedUserData = action.payload.data;
  if (state.userData) {
    state.userData.name = updatedUserData.name;
    state.userData.address = updatedUserData.address;
    localStorage.setItem(
      'loginData',
      JSON.stringify({
        isLoggedIn: state.isLoggedIn,
        userData: state.userData,
        token: state.token,
      })
    );
  }
});

// make set localStorage code more better
// utils/localStorage.ts
export const setLocalStorage = <T>(key: string, value: T): void => {
  localStorage.setItem(key, JSON.stringify(value))
}

export const getLocalStorage = <T>(key: string, defaultValue: T): T => {
  const storedValue = localStorage.getItem(key)
  return storedValue !== null ? JSON.parse(storedValue) as T : defaultValue
}

// update the userSlice.tsx
import api from "@/api"
import { CustomError, LoginFormData, ProfileFormData, User, UserState } from "@/types"
import { PayloadAction, createAsyncThunk, createSlice } from "@reduxjs/toolkit"
import { getLocalStorage, setLocalStorage } from "@/utils/localStorage"

const data = getLocalStorage("loginData", {
  userData: null,
  token: null,
  isLoggedIn: false
})

const initialState: UserState = {
  error: null,
  isLoading: false,
  userData: data.userData,
  token: data.token,
  isLoggedIn: data.isLoggedIn
}

export const registerUser = createAsyncThunk("users/registerUser", async (newUser: User) => {
  const response = await api.post("/users", newUser)
  return response.data
})

export const loginUser = createAsyncThunk("users/loginUser", async (userData: LoginFormData) => {
  const response = await api.post("/users/login", userData)
  return response.data
})

export const updateUser = createAsyncThunk(
  "users/updateUser",
  async ({ updateUserData, userId }: { updateUserData: ProfileFormData; userId: string }) => {
    const response = await api.put(`/users/${userId}`, updateUserData)
    return response.data
  }
)

const userSlice = createSlice({
  name: "users",
  initialState: initialState,
  reducers: {
    logoutUser: (state) => {
      state.isLoggedIn = false
      state.userData = null
      state.token = null
      setLocalStorage("loginData", {
        isLoggedIn: state.isLoggedIn,
        userData: state.userData,
        token: state.token
      })
    }
  },
  extraReducers(builder) {
    builder.addCase(loginUser.fulfilled, (state, action) => {
      state.isLoggedIn = true
      state.userData = action.payload.data.user
      state.token = action.payload.data.token
      setLocalStorage("loginData", {
        isLoggedIn: state.isLoggedIn,
        userData: state.userData,
        token: state.token
      })
    })

    builder.addCase(updateUser.fulfilled, (state, action) => {
      const updatedUserData = action.payload.data
      if (state.userData) {
        state.userData.name = updatedUserData.name
        state.userData.address = updatedUserData.address
        setLocalStorage("loginData", {
          isLoggedIn: state.isLoggedIn,
          userData: state.userData,
          token: state.token
        })
      }
    })

    builder.addMatcher(
      (action) => action.type.endsWith("/pending"),
      (state) => {
        state.error = null
        state.isLoading = true
      }
    )

    builder.addMatcher(
      (action) => action.type.endsWith("/rejected"),
      (state, action: PayloadAction<CustomError>) => {
        state.error = action.payload?.message || "An error occurred"
        state.isLoading = false
      }
    )
  }
})

export const { logoutUser } = userSlice.actions
export default userSlice.reducer


// add authorization in header
// utils/localStorage.ts

import { User } from "@/types"

export type LoginData = {
  isLoggedIn: boolean;
  userData: User | null ;
  token: string;
};

export const setLocalStorage = <T>(key: string, value: T): void => {
  localStorage.setItem(key, JSON.stringify(value))
}

export const getLocalStorage = <T>(key: string, defaultValue: T): T => {
  const storedValue = localStorage.getItem(key)
  return storedValue !== null ? (JSON.parse(storedValue) as T) : defaultValue
}

export const getToken = (): string | null => {
  const dataFromLocalStorage = getLocalStorage<LoginData>("loginData", {
    isLoggedIn: false,
    userData: null,
    token: ""
  })
  return dataFromLocalStorage.token
}

// change in slice now
export const updateUser = createAsyncThunk(
  "users/updateUser",
  async ({ updateUserData, userId }: { updateUserData: ProfileFormData; userId: string }) => {
    const token = getToken()
    const response = await api.put(`/users/${userId}`, updateUserData, {
      headers: {
        Authorization: `Bearer ${token}`
      }
    })
    return response.data
  }
)
```

### Admin Dashboard: Categories

- Read categories, Delete a category, Create a category, Update a category

- category slice

```tsx
// categorySlice
import api from '@/api';
import { CategoryState, CreateCategoryFormData, CustomError } from '@/types';
import { getToken } from '@/utils/localStorage';
import { PayloadAction, createAsyncThunk, createSlice } from '@reduxjs/toolkit';

const initialState: CategoryState = {
  categories: [],
  totalPages: 1,
  category: null,
  error: null,
  isLoading: false,
};

export const fetchCategories = createAsyncThunk(
  'categories/fetchCategories',
  async ({
    pageNumber,
    pageSize,
    searchTerm,
    sortBy,
  }: {
    pageNumber: number;
    pageSize: number;
    searchTerm: string;
    sortBy: string;
  }) => {
    const response =
      searchTerm.length > 0
        ? await api.get(
            `/categories?pageNumber=${pageNumber}&pageSize=${pageSize}&searchTerm=${searchTerm}&sortBy=${sortBy}`
          )
        : await api.get(
            `/categories?pageNumber=${pageNumber}&pageSize=${pageSize}&sortBy=${sortBy}`
          );
    return response.data;
  }
);
export const deleteCategory = createAsyncThunk(
  'categories/deleteCategory',
  async (categoryId: string) => {
    await api.delete(`/categories/${categoryId}`, {
      headers: {
        Authorization: `Bearer ${getToken()}`,
      },
    });
    return categoryId;
  }
);

export const createCategory = createAsyncThunk(
  'categories/createCategory',
  async (newCategory: CreateCategoryFormData) => {
    const response = await api.post('/categories', newCategory, {
      headers: {
        Authorization: `Bearer ${getToken()}`,
      },
    });
    console.log(response.data);
    return response.data.data;
  }
);

export const updateCategory = createAsyncThunk(
  'categories/updateCategory',
  async ({
    updateCategoryData,
    slug,
  }: {
    updateCategoryData: CreateCategoryFormData;
    slug: string;
  }) => {
    const response = await api.put(`/categories/${slug}`, updateCategoryData, {
      headers: {
        Authorization: `Bearer ${getToken()}`,
      },
    });
    return response.data;
  }
);

const categorySlice = createSlice({
  name: 'categories',
  initialState: initialState,
  reducers: {},
  extraReducers(builder) {
    builder.addCase(fetchCategories.fulfilled, (state, action) => {
      state.categories = action.payload.data.items;
      state.totalPages = action.payload.data.totalPages;
      state.isLoading = false;
    });
    builder.addCase(deleteCategory.fulfilled, (state, action) => {
      state.categories = state.categories.filter(
        (category) => category.categoryId !== action.payload
      );
    });
    builder.addCase(createCategory.fulfilled, (state, action) => {
      state.categories.push(action.payload);
    });

    builder.addCase(updateCategory.fulfilled, (state, action) => {
      const foundCategory = state.categories.find(
        (category) => category.categoryId === action.payload.data.categoryId
      );
      if (foundCategory) {
        foundCategory.name = action.payload.data.name;
        foundCategory.description = action.payload.data.description;
      }
    });

    builder.addMatcher(
      (action) => action.type.endsWith('/pending'),
      (state) => {
        state.error = null;
        state.isLoading = true;
      }
    );

    builder.addMatcher(
      (action) => action.type.endsWith('/rejected'),
      (state, action: PayloadAction<CustomError>) => {
        state.error = action.payload?.message || 'An error occured';
        state.isLoading = false;
      }
    );
  },
});

export default categorySlice.reducer;
```

- custom selector

```tsx
import { RootState } from '@/tookit/store';
import { useSelector } from 'react-redux';

const useCategoriesState = () => {
  const { categories, isLoading, error, totalPages, category } = useSelector(
    (state: RootState) => state.categoryR
  );
  return { categories, isLoading, error, totalPages, category };
};

export default useCategoriesState;
```

- Admin Categories components

```tsx
import React, { useEffect, useState } from 'react';
import { useDispatch } from 'react-redux';
import { AppDispatch } from '@/tookit/store';
import { SubmitHandler, useForm } from 'react-hook-form';
import AdminSidebar from '@/components/AdminSidebar';
import useCategoriesState from '@/hooks/useCategoriesState';
import {
  createCategory,
  deleteCategory,
  fetchCategories,
  updateCategory,
} from '@/tookit/slices/categorySlice';
import { Category, CreateCategoryFormData } from '@/types';

const AdminCategories = () => {
  const { categories, isLoading, error, totalPages } = useCategoriesState();
  const dispatch: AppDispatch = useDispatch();

  const {
    register,
    handleSubmit,
    reset,
    setValue,
    formState: { errors },
  } = useForm<CreateCategoryFormData>();
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize] = useState(5);
  const [searchTerm, setSearchTerm] = useState('');
  const [sortBy, setSortBy] = useState('Name');
  const [isEdit, setIsEdit] = useState(false);
  const [selectedCategorySlug, setSelectedCategorySlug] = useState('');

  useEffect(() => {
    const fetchData = async () => {
      await dispatch(
        fetchCategories({ pageNumber, pageSize, searchTerm, sortBy })
      );
    };
    fetchData();
  }, [pageNumber, searchTerm, sortBy]);

  const handlePreviousPage = () =>
    setPageNumber((currentPage) => currentPage - 1);
  const handleNextPage = () => setPageNumber((currentPage) => currentPage + 1);
  const handleSearchChange = (e: React.ChangeEvent<HTMLInputElement>) =>
    setSearchTerm(e.target.value);
  const handleSortChange = (e: React.ChangeEvent<HTMLSelectElement>) =>
    setSortBy(e.target.value);

  const onSubmit: SubmitHandler<CreateCategoryFormData> = async (data) => {
    try {
      if (isEdit) {
        await dispatch(
          updateCategory({
            updateCategoryData: data,
            slug: selectedCategorySlug,
          })
        );
        setIsEdit(false);
      } else {
        await dispatch(createCategory(data));
      }
      reset();
    } catch (error) {
      console.log(error);
    }
  };

  const handleEdit = (category: Category) => {
    setIsEdit(true);
    setSelectedCategorySlug(category.slug);
    setValue('name', category.name);
    setValue('description', category.description);
  };

  const handleDelete = async (id: string) => {
    try {
      await dispatch(deleteCategory(id));
    } catch (error) {
      console.log(error);
    }
  };

  return (
    <div className="container flex-space-around">
      <AdminSidebar />
      <div className="main-container">
        {isLoading && <p>Loading...</p>}
        {error && <p>Error: {error}</p>}

        <div className="action flex-space-around">
          <div>
            <input
              type="text"
              placeholder="Search Categories"
              value={searchTerm}
              onChange={handleSearchChange}
            />
          </div>
          <div className="flex-center">
            <p>Sort By: </p>
            <select name="" id="" onChange={handleSortChange}>
              <option value="Name">Name</option>
              <option value="Price">Price</option>
            </select>
          </div>
        </div>
        <br />

        {/* create or edit category form starts here  */}
        <div className="card">
          <h2>{isEdit ? 'Edit Category' : 'Create Category'}</h2>
          <form onSubmit={handleSubmit(onSubmit)}>
            <div className="form-field">
              <label htmlFor="name"> Name: </label>
              <input
                type="text"
                {...register('name', {
                  required: 'Name is required',
                  minLength: {
                    value: 2,
                    message: 'Name must be at least 2 characters',
                  },
                })}
              />
              {errors.name && <p>{errors.name.message}</p>}
            </div>
            <div className="form-field">
              <label htmlFor="description"> Description: </label>
              <textarea {...register('description')} />
            </div>
            <button className="btn" type="submit">
              {isEdit ? 'Update Category' : 'Create Category'}
            </button>
          </form>
        </div>
        <br />
        {/* create or edit category form ends here  */}

        {/* list of categories starts here  */}
        <table className="category-table">
          <thead>
            <tr>
              <th>Name</th>
              <th>Description</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {categories.map((category) => (
              <tr key={category.categoryId}>
                <td>{category.name}</td>
                <td>{category.description.substring(0, 100)}...</td>
                <td>
                  <button className="btn" onClick={() => handleEdit(category)}>
                    Edit
                  </button>
                  <button
                    className="btn"
                    onClick={() => handleDelete(category.categoryId)}
                  >
                    Delete
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
        {/* list of categories ends here  */}

        {/* pagination  */}
        <div className="pagination">
          <button onClick={handlePreviousPage} disabled={pageNumber === 1}>
            Previous
          </button>
          {Array.from({ length: totalPages }, (_, index) => (
            <button key={index} onClick={() => setPageNumber(index + 1)}>
              {index + 1}
            </button>
          ))}
          <button onClick={handleNextPage} disabled={pageNumber === totalPages}>
            Next
          </button>
        </div>
      </div>
    </div>
  );
};

export default AdminCategories;
```

- add css

```css
table {
  width: 100%;
  border-collapse: collapse;
  margin: 20px 0;
  font-size: 1em;
  text-align: left;
}

th,
td {
  padding: 12px 15px;
}

th {
  background-color: #4caf50;
  color: white;
  text-align: left;
}

tr {
  border-bottom: 1px solid #dddddd;
}

tr:nth-of-type(even) {
  background-color: #f3f3f3;
}

tr:last-of-type {
  border-bottom: 2px solid #4caf50;
}
.ban-btn {
  background-color: #4caf50;
  width: 5rem;
  margin-right: 5px;
}

.delete-btn {
  background-color: #f44336;
}
```

### Admin Dashboard: Users

#### get all the users (not admin) in a table

- update the type to get users

```tsx
// in types folder
export type UserState = {
  error: null | string;
  isLoading: boolean;
  userData: null | User;
  users: User[];
  totalPages: number;
  token: null | string;
  isLoggedIn: boolean;
};
// in slice
const initialState: UserState = {
  users: [],
  totalPages: 1,
  error: null,
  isLoading: false,
  userData: data.userData,
  token: data.token,
  isLoggedIn: data.isLoggedIn,
};
```

- create the action creator for fetching data

```tsx
export const fetchUsers = createAsyncThunk(
  'users/fetchUsers',
  async ({
    pageNumber,
    pageSize,
    searchTerm,
    sortBy,
  }: {
    pageNumber: number;
    pageSize: number;
    searchTerm: string;
    sortBy: string;
  }) => {
    const response =
      searchTerm.length > 0
        ? await api.get(
            `/users?pageNumber=${pageNumber}&pageSize=${pageSize}&searchTerm=${searchTerm}&sortBy=${sortBy}`,
            {
              headers: {
                Authorization: `Bearer ${getToken()}`,
              },
            }
          )
        : await api.get(
            `/users?pageNumber=${pageNumber}&pageSize=${pageSize}&sortBy=${sortBy}`,
            {
              headers: {
                Authorization: `Bearer ${getToken()}`,
              },
            }
          );
    return response.data;
  }
);
```

- handle the fulfilled state

```tsx
builder.addCase(fetchUsers.fulfilled, (state, action) => {
  state.users = action.payload.data.items;
  state.totalPages = action.payload.data.totalPages;
  state.isLoading = false;
});
```

- update the custom selector hook

```tsx
import { RootState } from '@/tookit/store';
import { useSelector } from 'react-redux';

const useUsersState = () => {
  const { users, userData, isLoading, error, token, isLoggedIn, totalPages } =
    useSelector((state: RootState) => state.userR);
  return { users, userData, isLoading, error, token, isLoggedIn, totalPages };
};

export default useUsersState;
```

- list all users in Users component

```tsx
// copy paste everything from categories and adjust everything
import AdminSidebar from '@/components/AdminSidebar';
import useUsersState from '@/hooks/useUsersState';
import { fetchUsers } from '@/tookit/slices/userSlice';
import { AppDispatch } from '@/tookit/store';
import React, { useEffect, useState } from 'react';
import { useDispatch } from 'react-redux';

export const AdminUserManagement = () => {
  const { users, isLoading, error, totalPages } = useUsersState();
  const dispatch: AppDispatch = useDispatch();

  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize] = useState(5);
  const [searchTerm, setSearchTerm] = useState('');
  const [sortBy, setSortBy] = useState('Name');

  useEffect(() => {
    const fetchData = async () => {
      await dispatch(fetchUsers({ pageNumber, pageSize, searchTerm, sortBy }));
    };
    fetchData();
  }, [pageNumber, searchTerm, sortBy, dispatch]);

  const handlePreviousPage = () =>
    setPageNumber((currentPage) => currentPage - 1);
  const handleNextPage = () => setPageNumber((currentPage) => currentPage + 1);
  const handleSearchChange = (e: React.ChangeEvent<HTMLInputElement>) =>
    setSearchTerm(e.target.value);
  const handleSortChange = (e: React.ChangeEvent<HTMLSelectElement>) =>
    setSortBy(e.target.value);

  return (
    <div className="container flex-space-around">
      <AdminSidebar />
      <div className="main-container">
        {isLoading && <p>Loading...</p>}
        {error && <p>Error: {error}</p>}
        <div className="action flex-space-around">
          <div>
            <input
              type="text"
              placeholder="Search Users"
              value={searchTerm}
              onChange={handleSearchChange}
            />
          </div>
          <div className="flex-center">
            <p>Sort By: </p>
            <select name="" id="" onChange={handleSortChange}>
              <option value="Name">Name</option>
              <option value="Email">Email</option>
            </select>
          </div>
        </div>
        <br />
        // list users in a table
        <h2>User List</h2>
        <table className="user-table">
          <thead>
            <tr>
              <th>Name</th>
              <th>Email</th>
              <th>Address</th>
              <th>Is Admin</th>
              <th>Is Banned</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {users && users.length > 0 ? (
              users.map((user) => (
                <tr key={user.userId}>
                  <td>{user.name}</td>
                  <td>{user.email}</td>
                  <td>{user.address}</td>
                  <td>{user.isAdmin ? 'Yes' : 'No'}</td>
                  <td>{user.isBanned ? 'Yes' : 'No'}</td>
                  <td>
                    <button className="btn ban-btn">
                      {user.isBanned ? 'Unban' : 'Ban'}
                    </button>
                  </td>
                </tr>
              ))
            ) : (
              <tr>
                <td colSpan={6}>No users found</td>
              </tr>
            )}
          </tbody>
        </table>
        <br />
        <div className="pagination">
          <button onClick={handlePreviousPage} disabled={pageNumber === 1}>
            Previous
          </button>
          {Array.from({ length: totalPages }, (_, index) => (
            <button key={index} onClick={() => setPageNumber(index + 1)}>
              {index + 1}
            </button>
          ))}
          <button onClick={handleNextPage} disabled={pageNumber === totalPages}>
            Next
          </button>
        </div>
      </div>
    </div>
  );
};

export default AdminUserManagement;
```

- add css

```css
table {
  width: 100%;
  border-collapse: collapse;
  margin: 20px 0;
  font-size: 1em;
  text-align: left;
}

th,
td {
  padding: 12px 15px;
}

th {
  background-color: #4caf50;
  color: white;
  text-align: left;
}

tr {
  border-bottom: 1px solid #dddddd;
}

tr:nth-of-type(even) {
  background-color: #f3f3f3;
}

tr:last-of-type {
  border-bottom: 2px solid #4caf50;
}
.ban-btn {
  background-color: #4caf50;
  width: 5rem;
  margin-right: 5px;
}

.delete-btn {
  background-color: #f44336;
}
/* users ends here  */
```

#### ban or delete user

```tsx
// get the user id based on click
const handleBanChange = async (userId: string | undefined) => {
  try {
    const response = await dispatch(banUnbanUser(userId));
    console.log(response);
    // toast.success("user ban status updated successfully")
  } catch (error) {
    toast.error('user ban status updated failed');
    console.log(error);
  }
};

// dispatch the action and update the state
export const banUnbanUser = createAsyncThunk(
  'users/banUnbanUser',
  async (userId: string | undefined) => {
    await api.put(
      `/users/ban-unban/${userId}`,
      {},
      {
        headers: {
          Authorization: `Bearer ${getToken()}`,
        },
      }
    );
    return userId;
  }
);

builder.addCase(banUnbanUser.fulfilled, (state, action) => {
  const foundUser = state.users.find((user) => user.userId === action.payload);
  if (foundUser) {
    foundUser.isBanned = !foundUser.isBanned;
  }
  state.error = null;
  state.isLoading = false;
});
```

### Admin Dashboard: Products

#### Get all products on a table

#### CRUD Product

- AdminProductManagement.tsx

```tsx
import React, { useEffect, useState } from 'react';
import { useDispatch } from 'react-redux';
import { AppDispatch } from '@/tookit/store';
import { Controller, SubmitHandler, useForm } from 'react-hook-form';
import AdminSidebar from '@/components/AdminSidebar';
import useCategoriesState from '@/hooks/useCategoriesState';
import { fetchCategories } from '@/tookit/slices/categorySlice';
import { CreateProductFormData, Product } from '@/types';
import useProductsState from '@/hooks/useProductsState';
import {
  createProduct,
  deleteProduct,
  fetchProducts,
  updateProduct,
} from '@/tookit/slices/productSlice';
import { uploadImageToCloudinary } from '@/utils/cloudinary';
import { toast } from 'react-toastify';

const AdminProductManagement = () => {
  const {
    categories,
    isLoading: categoriesLoading,
    error: categoriesError,
  } = useCategoriesState();
  const { products, totalPages, isLoading, error } = useProductsState();

  const dispatch: AppDispatch = useDispatch();

  console.log(products);

  const {
    register,
    handleSubmit,
    reset,
    setValue,
    control,
    watch,
    formState: { errors },
  } = useForm<CreateProductFormData>();
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize] = useState(5);
  const [searchTerm, setSearchTerm] = useState('');
  const [sortBy, setSortBy] = useState('Name');
  const [isEdit, setIsEdit] = useState(false);
  const [imagePreview, setImagePreview] = useState<string | null>(null);
  const [selectedProductSlug, setSelectedProductSlug] = useState<string | null>(
    null
  );

  const watchedCategoryIds = watch('categoryIds');
  useEffect(() => {
    console.log('Selected categories: ', watchedCategoryIds);
  }, [watchedCategoryIds]);

  useEffect(() => {
    const fetchData = async () => {
      await dispatch(
        fetchCategories({ pageNumber, pageSize, searchTerm, sortBy })
      );
    };
    fetchData();
  }, []);

  useEffect(() => {
    const fetchData = async () => {
      await dispatch(
        fetchProducts({ pageNumber, pageSize, searchTerm, sortBy })
      );
    };
    fetchData();
  }, [pageNumber, searchTerm, sortBy]);

  const handlePreviousPage = () =>
    setPageNumber((currentPage) => currentPage - 1);
  const handleNextPage = () => setPageNumber((currentPage) => currentPage + 1);
  const handleSearchChange = (e: React.ChangeEvent<HTMLInputElement>) =>
    setSearchTerm(e.target.value);
  const handleSortChange = (e: React.ChangeEvent<HTMLSelectElement>) =>
    setSortBy(e.target.value);

  const onSubmit: SubmitHandler<CreateProductFormData> = async (data) => {
    try {
      let imageUrl = '';
      if (data.image && data.image.length > 0) {
        const file = data.image[0];
        imageUrl = await uploadImageToCloudinary(file);
      }

      const productData = {
        ...data,
        image: imageUrl,
      };

      if (isEdit && selectedProductSlug) {
        const response = await dispatch(
          updateProduct({
            updateProductData: productData,
            slug: selectedProductSlug,
          })
        );
        if (response.meta.requestStatus === 'fulfilled') {
          toast.success('Product updated successfully');
        } else {
          toast.error('Product update failed');
        }
      } else {
        const response = await dispatch(createProduct(productData));
        if (response.meta.requestStatus === 'fulfilled') {
          toast.success('Product created successfully');
        } else {
          toast.error('Product creation failed');
        }
      }

      reset();
      setIsEdit(false);
      setSelectedProductSlug(null);
    } catch (error) {
      console.log('Product creation/update failed');
      toast.error('Product creation/update failed');
    }
  };

  const handleImageChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (file) {
      setImagePreview(URL.createObjectURL(file));
    }
  };

  const handleEdit = (product: Product) => {
    // alert(JSON.stringify(product))
    setIsEdit(true);
    setSelectedProductSlug(product.slug);
    // setSelectedCategorySlug(category.slug)
    setValue('name', product.name);
    setValue('description', product.description);
    setValue('price', product.price);
    setValue('quantity', product.quantity);
    setValue('shipping', product.shipping);
    setValue(
      'categoryIds',
      product.categories.map((category) => category.categoryId)
    );
    product.image && setImagePreview(product.image);
  };

  const handleDelete = async (slug: string) => {
    try {
      dispatch(deleteProduct(slug));
    } catch (error) {
      console.log(error);
    }
  };

  return (
    <div className="container flex-space-around">
      <AdminSidebar />
      <div className="main-container">
        {isLoading && <p>Loading...</p>}
        {error && <p>Error: {error}</p>}

        <div className="action flex-space-around">
          <div>
            <input
              type="text"
              placeholder="Search Categories"
              value={searchTerm}
              onChange={handleSearchChange}
            />
          </div>
          <div className="flex-center">
            <p>Sort By: </p>
            <select name="" id="" onChange={handleSortChange}>
              <option value="Name">Name</option>
              <option value="Price">Price</option>
            </select>
          </div>
        </div>
        <br />

        {/* create or edit category form starts here  */}
        <div className="card">
          <h2>{isEdit ? 'Edit Product' : 'Create Product'}</h2>
          <form onSubmit={handleSubmit(onSubmit)}>
            <div className="form-field">
              <label htmlFor="name"> Name: </label>
              <input
                type="text"
                {...register('name', {
                  required: 'Name is required',
                  minLength: {
                    value: 2,
                    message: 'Name must be at least 2 characters',
                  },
                })}
              />
              {errors.name && <p>{errors.name.message}</p>}
            </div>
            <div className="form-field">
              <label htmlFor="description"> Description: </label>
              <textarea {...register('description')} />
            </div>
            <div className="form-field">
              <label htmlFor="price"> Price: </label>
              <input
                type="number"
                step="0.01"
                {...register('price', {
                  required: 'Price is required',
                })}
              />
              {errors.price && <p>{errors.price.message}</p>}
            </div>
            <div className="form-field">
              <label htmlFor="quantity"> Quantity: </label>
              <input
                type="number"
                step="0.01"
                {...register('quantity', {
                  required: 'Quantity is required',
                })}
              />
              {errors.quantity && <p>{errors.quantity.message}</p>}
            </div>

            <div className="form-field">
              <label htmlFor="shipping"> Shipping: </label>
              <input
                type="number"
                step="0.01"
                {...register('shipping', {
                  required: 'Shipping is required',
                })}
              />
              {errors.shipping && <p>{errors.shipping.message}</p>}
            </div>
            <div className="form-field">
              <label htmlFor="image"> Image: </label>
              <input
                type="file"
                accept="image/*"
                {...register('image')}
                onChange={handleImageChange}
              />
              {imagePreview && (
                <img
                  src={imagePreview}
                  alt="image preview"
                  className="table-img"
                />
              )}
            </div>

            <br />
            <div className="form-field">
              <label htmlFor="categoryIds"> Categories: </label>
              <Controller
                name="categoryIds"
                control={control}
                render={({ field }) => (
                  <select
                    multiple
                    {...field}
                    onChange={(e) => {
                      const selectedOptions = Array.from(
                        e.target.selectedOptions
                      );
                      const selectedValues = selectedOptions.map(
                        (option) => option.value
                      );
                      field.onChange(selectedValues);
                    }}
                  >
                    {categories.map((category) => (
                      <option
                        key={category.categoryId}
                        value={category.categoryId}
                      >
                        {category.name}
                      </option>
                    ))}
                  </select>
                )}
              />
            </div>
            <br />
            <button className="btn" type="submit">
              {isEdit ? 'Update Product' : 'Create Product'}
            </button>
          </form>
        </div>
        <br />
        {/* create or edit category form ends here  */}

        {/* list of categories starts here  */}
        <h2>List of Products</h2>
        <table>
          <thead>
            <tr>
              <th>Image</th>
              <th>Name</th>
              <th>Categories</th>
              <th>Description</th>
              <th>Price</th>
              <th>Quantity</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {products &&
              products.length > 0 &&
              products.map((product) => (
                <tr key={product.productId}>
                  <td>
                    <img
                      src={product.image}
                      alt={product.slug}
                      className="table-img"
                    />
                  </td>
                  <td>{product.name}</td>
                  <td>
                    {product.categories &&
                      product.categories
                        .map((category) => category.name)
                        .join(', ')}
                  </td>
                  <td>
                    {product.description &&
                      product.description.substring(0, 100)}
                    ...
                  </td>
                  <td>{product.price}</td>
                  <td>{product.quantity}</td>
                  <td>
                    <button
                      className="btn edit-btn"
                      onClick={() => handleEdit(product)}
                    >
                      Edit
                    </button>
                    <button
                      className="btn delete-btn"
                      onClick={() => handleDelete(product.slug)}
                    >
                      Delete
                    </button>
                  </td>
                </tr>
              ))}
          </tbody>
        </table>
        {/* list of categories ends here  */}

        {/* pagination  */}
        <div className="pagination">
          <button onClick={handlePreviousPage} disabled={pageNumber === 1}>
            Previous
          </button>
          {Array.from({ length: totalPages }, (_, index) => (
            <button key={index} onClick={() => setPageNumber(index + 1)}>
              {index + 1}
            </button>
          ))}
          <button onClick={handleNextPage} disabled={pageNumber === totalPages}>
            Next
          </button>
        </div>
      </div>
    </div>
  );
};

export default AdminProductManagement;
```

- productsSlice.tsx

```tsx
import api from '@/api';
import {
  CreateProductForBackend,
  CreateProductFormData,
  CustomError,
  Product,
  ProductState,
} from '@/types';
import { getToken } from '@/utils/localStorage';
import { PayloadAction, createAsyncThunk, createSlice } from '@reduxjs/toolkit';

const initialState: ProductState = {
  products: [],
  totalPages: 1,
  product: null,
  error: null,
  isLoading: false,
};

export const fetchProducts = createAsyncThunk(
  'products/fetchProducts',
  async ({
    pageNumber,
    pageSize,
    searchTerm,
    sortBy,
  }: {
    pageNumber: number;
    pageSize: number;
    searchTerm: string;
    sortBy: string;
  }) => {
    const response =
      searchTerm.length > 0
        ? await api.get(
            `/products?pageNumber=${pageNumber}&pageSize=${pageSize}&searchTerm=${searchTerm}&sortBy=${sortBy}`
          )
        : await api.get(
            `/products?pageNumber=${pageNumber}&pageSize=${pageSize}&sortBy=${sortBy}`
          );
    return response.data;
  }
);

export const fetchProductBySlug = createAsyncThunk(
  'products/fetchProductBySlug',
  async (slug: string | undefined) => {
    const response = await api.get(`/products/${slug}`);
    return response.data;
  }
);

export const deleteProduct = createAsyncThunk(
  'products/deleteProduct',
  async (slug: string) => {
    await api.delete(`/products/${slug}`, {
      headers: {
        Authorization: `Bearer ${getToken()}`,
      },
    });
    return slug;
  }
);

export const createProduct = createAsyncThunk(
  'products/createProduct',
  async (newProduct: CreateProductForBackend) => {
    const response = await api.post('/products', newProduct, {
      headers: {
        Authorization: `Bearer ${getToken()}`,
      },
    });
    return response.data;
  }
);

export const updateProduct = createAsyncThunk(
  'products/updateProduct',
  async ({
    updateProductData,
    slug,
  }: {
    updateProductData: CreateProductForBackend;
    slug: string;
  }) => {
    const response = await api.put(`/products/${slug}`, updateProductData, {
      headers: {
        Authorization: `Bearer ${getToken()}`,
      },
    });
    return response.data;
  }
);

// cases: pending, fullfilled, rejected
const productSlice = createSlice({
  name: 'products',
  initialState: initialState,
  reducers: {},
  extraReducers(builder) {
    builder.addCase(fetchProducts.fulfilled, (state, action) => {
      state.products = action.payload.data.items;
      state.totalPages = action.payload.data.totalPages;
      state.isLoading = false;
    });

    builder.addCase(fetchProductBySlug.fulfilled, (state, action) => {
      state.product = action.payload.data;
      state.isLoading = false;
    });

    builder.addCase(deleteProduct.fulfilled, (state, action) => {
      state.products = state.products.filter(
        (product) => product.slug !== action.payload
      );
      state.isLoading = false;
    });
    builder.addCase(createProduct.fulfilled, (state, action) => {
      state.products.push(action.payload);
      state.isLoading = false;
    });
    builder.addCase(updateProduct.fulfilled, (state, action) => {
      const updatedProduct = action.payload.data;

      const foundProduct = state.products.find(
        (product) => product.productId === updatedProduct.productId
      );

      // simplified
      if (foundProduct) {
        Object.assign(foundProduct, updatedProduct);
      }

      // not simplified
      // if (foundProduct) {
      //   foundProduct.name = updatedProduct.name
      //   foundProduct.description = updatedProduct.description
      //   foundProduct.price = updatedProduct.price
      //   foundProduct.quantity = updatedProduct.quantity
      //   foundProduct.sold = updatedProduct.sold
      //   foundProduct.shipping = updatedProduct.shipping
      //   foundProduct.categories = updatedProduct.categories
      //   foundProduct.image = updatedProduct.image
      // }
      state.isLoading = false;
    });

    builder.addMatcher(
      (action) => action.type.endsWith('/pending'),
      (state) => {
        state.error = null;
        state.isLoading = true;
      }
    );

    builder.addMatcher(
      (action) => action.type.endsWith('/rejected'),
      (state, action: PayloadAction<CustomError>) => {
        state.error = action.payload?.message || 'An error occured';
        state.isLoading = false;
      }
    );
  },
});

export default productSlice.reducer;
```

### Filtering based on categories and prices

- display the categories in checkboxes

```tsx
const {
  categories,
  isLoading: categoriesLoading,
  error: categoriesError,
} = useCategoriesState();

useEffect(() => {
    const fetchData = async () => {
      await dispatch(fetchCategories({ pageNumber, pageSize: 20, searchTerm, sortBy }))
    }
    fetchData()
  }, [])

// display categories
<div>
  <h3>Filter by Category goes here</h3>
  {categories.map((category) => (
    <div key={category.categoryId}>
      <label>
        <input
          type="checkbox"
          value={category.categoryId}
          checked={selectedCategories.includes(category.categoryId)}
          onChange={() => handleCategoryChange(category.categoryId)}
        />
        {category.name}
      </label>
    </div>
  ))}
</div>

// handle the checked and store in an array of string
const [selectedCategories, setSelectedCategories] = useState<string[]>([])
const handleCategoryChange = (categoryId: string) => {
    setSelectedCategories((prevSelected) =>
      prevSelected.includes(categoryId)
        ? prevSelected.filter((id) => id !== categoryId)
        : [...prevSelected, categoryId]
    )
  }

  // update the dispatch and slice
   useEffect(() => {
    const fetchProductsData = async () => {
      console.log("Fetching products with selected categories: ", selectedCategories)
      await dispatch(
        fetchProducts({ pageNumber, pageSize, searchTerm, sortBy, selectedCategories })
      )
    }
    fetchProductsData()
  }, [pageNumber, searchTerm, sortBy, selectedCategories, dispatch])

  export const fetchProducts = createAsyncThunk(
  "products/fetchProducts",
  async ({
    pageNumber,
    pageSize,
    searchTerm,
    sortBy,
    selectedCategories
  }: {
    pageNumber: number
    pageSize: number
    searchTerm: string
    sortBy: string
    selectedCategories: string[]
  }) => {
    const params = new URLSearchParams({
      pageNumber: pageNumber.toString(),
      pageSize: pageSize.toString(),
      searchTerm,
      sortBy
    })

    selectedCategories.forEach((categoryId) => {
      params.append("SelectedCategories", categoryId)
    })

    const response = await api.get("/products", { params })
    return response.data
  }
)
```

#### Lets add filter by price

```tsx
// backend api queryparameters
public class QueryParameters
{
  public int PageNumber { get; set; } = 1;
  public int PageSize { get; set; } = 5;
  public string? SearchTerm { get; set; } = "";
  public string? SortBy { get; set; } = "Name"; // Default sorting by name
  public string? SortOrder { get; set; } = "asc"; // asc or desc
  public List<Guid>? SelectedCategories { get; set; } = new List<Guid>();

  public decimal? MinPrice { get; set; }
  public decimal? MaxPrice { get; set; }

}

// backend api inside service
if (queryParams.MinPrice.HasValue)
  {
    query = query.Where(p => p.Price >= queryParams.MinPrice.Value);
  }

  if (queryParams.MaxPrice.HasValue)
  {
    query = query.Where(p => p.Price <= queryParams.MaxPrice.Value);
  }

// test from swagger

// display price range
const [minPrice, setMinPrice] = useState<number | undefined>(undefined)
const [maxPrice, setMaxPrice] = useState<number | undefined>(undefined)

const handleMinPriceChange = (e: React.ChangeEvent<HTMLInputElement>) => setMinPrice(Number(e.target.value))
const handleMaxPriceChange = (e: React.ChangeEvent<HTMLInputElement>) => setMaxPrice(Number(e.target.value))

 <div>
  <h3>Filter by Price</h3>
  <label>
    Min Price:
    <input type="number" value={minPrice ?? ""} onChange={handleMinPriceChange} />
  </label>
  <label>
    Max Price:
    <input type="number" value={maxPrice ?? ""} onChange={handleMaxPriceChange} />
  </label>
</div>

// useEffect
useEffect(() => {
  const fetchProductsData = async () => {
    console.log("Fetching products with selected categories: ", selectedCategories)
    await dispatch(
      fetchProducts({
        pageNumber,
        pageSize,
        searchTerm,
        sortBy,
        selectedCategories,
        minPrice,
        maxPrice
      })
    )
  }
  fetchProductsData()
}, [pageNumber, searchTerm, sortBy, selectedCategories, dispatch, minPrice, maxPrice])


// update the product slice
export const fetchProducts = createAsyncThunk(
  "products/fetchProducts",
  async ({
    pageNumber,
    pageSize,
    searchTerm,
    sortBy,
    selectedCategories,
    minPrice,
    maxPrice
  }: {
    pageNumber: number
    pageSize: number
    searchTerm: string
    sortBy: string
    selectedCategories: string[]
    minPrice?: number
    maxPrice?: number
  }) => {
    const params = new URLSearchParams({
      pageNumber: pageNumber.toString(),
      pageSize: pageSize.toString(),
      searchTerm,
      sortBy
    });

    selectedCategories.forEach((categoryId) => {
      params.append("SelectedCategories", categoryId);
    });

    if (minPrice !== undefined) {
      params.append("MinPrice", minPrice.toString());
    }

    if (maxPrice !== undefined) {
      params.append("MaxPrice", maxPrice.toString());
    }

    const response = await api.get(`/products?${params.toString()}`);
    return response.data;
  }
);
```

### User Dashboard: User Order

### Cart Page

- Create a cart route `<Route path="/cart" element={<Cart />} />`

- Create a basic cart page

- Create a cart Icon and show it a navbar

  ```tsx
  // npm install react-icons --save

  import { FaCartPlus } from 'react-icons/fa'
  const CartIcon = ({ value }: { value: string }) => {
    return (
      <div className="cart-icon">
        <FaCartPlus />
        <span className="badge">{value}</span>
      </div>
    )
  }

  export default CartIcon

  // add css
  .cart-icon {
    height: 100%;
    position: relative;
    color: white;
  }

  .badge {
    background-color: red;
    position: absolute;
    top: -18px;
    right: -15px;
    color: white;
    padding: 1px 6px;
    text-align: center;
    border-radius: 5px;
  }

  // add in the Navbar component
  <Link to="/cart" className="nav__link">
    <CartIcon value={cartItems.length > 0 ? cartItems.length.toString() : '0'} />
  </Link>
  ```

- handle the add to cart click and display product in console

  ```tsx
  <button
    className="btn product__btn"
    onClick={() => {
      handleAddToCart(product);
    }}
  >
    Add To Cart
  </button>;

  const handleAddToCart = (product: Product) => {
    console.log(product);
    // dispatch the product for setCart
    dispatch(setCart(product));
    // show a toast message
  };
  ```

- create the cartSlice

```tsx
import { Product } from '@/types';
import { getLocalStorage, setLocalStorage } from '@/utils/localStorage';
import { PayloadAction, createSlice } from '@reduxjs/toolkit';

const data = getLocalStorage<CartState>('cart', { cartItems: [] });

export type CartItem = Product & { orderQuantity: number };

export type CartState = {
  cartItems: CartItem[];
};
const initialState: CartState = {
  cartItems: data.cartItems,
};

console.log(data.cartItems);

// cases: pending, fullfilled, rejected
const cartSlice = createSlice({
  name: 'cart',
  initialState: initialState,
  reducers: {
    addToCart: (state, action: PayloadAction<Product>) => {
      const item = state.cartItems.find(
        (cartItem) => cartItem.productId === action.payload.productId
      );
      if (item) {
        item.orderQuantity += 1;
      } else {
        state.cartItems.push({ ...action.payload, orderQuantity: 1 });
      }
      setLocalStorage('cart', state);
    },
    incrementQuantity: (state, action: PayloadAction<string>) => {
      const item = state.cartItems.find(
        (cartItem) => cartItem.productId === action.payload
      );
      if (item) {
        item.orderQuantity += 1;
      }
      setLocalStorage('cart', state);
    },
    decrementQuantity: (state, action: PayloadAction<string>) => {
      const item = state.cartItems.find(
        (cartItem) => cartItem.productId === action.payload
      );
      if (item && item.orderQuantity > 1) {
        item.orderQuantity -= 1;
      }
      setLocalStorage('cart', state);
    },
    removeFromCart: (state, action: PayloadAction<string>) => {
      state.cartItems = state.cartItems.filter(
        (cartItem) => cartItem.productId !== action.payload
      );
      setLocalStorage('cart', state);
    },
    removeAllFromCart: (state) => {
      state.cartItems = [];
      setLocalStorage('cart', state);
    },
  },
});

export const {
  addToCart,
  removeAllFromCart,
  removeFromCart,
  incrementQuantity,
  decrementQuantity,
} = cartSlice.actions;
export default cartSlice.reducer;
```

- create a custom hook to access cart values from store

  ```tsx
  import { RootState } from '@/tookit/store';
  import { useSelector } from 'react-redux';

  const useCartState = () => {
    const { cartItems } = useSelector((state: RootState) => state.cartR);
    return { cartItems };
  };

  export default useCartState;
  ```

- display total items in navbar

```tsx
<li>
  <Link className="nav__link" to="/cart">
    <CartIcon value={cartItems.length > 0 ? cartItems.length : 0} />
  </Link>
</li>
```

#### Create cart component

```tsx
import useCartState from '@/hooks/useCartState';
import useUsersState from '@/hooks/useUsersState';
import {
  decrementQuantity,
  incrementQuantity,
  removeAllFromCart,
  removeFromCart,
} from '@/tookit/slices/cartSlice';
import { AppDispatch } from '@/tookit/store';
import React from 'react';
import { useDispatch } from 'react-redux';
import { useNavigate } from 'react-router-dom';

export const Cart = () => {
  const { cartItems } = useCartState();
  const { userData, isLoggedIn } = useUsersState();

  console.log(cartItems && cartItems.length);

  const dispatch: AppDispatch = useDispatch();
  const navigate = useNavigate();

  const handleRemoveAllProductsFromCart = () => {
    dispatch(removeAllFromCart());
  };

  const handleRemoveFromCart = (productId?: string) => {
    if (productId) {
      dispatch(removeFromCart(productId));
    }
  };
  const handleIncrementQuantity = (productId?: string) => {
    if (productId) {
      dispatch(incrementQuantity(productId));
    }
  };
  const handleDecrementQuantity = (productId?: string) => {
    if (productId) {
      dispatch(decrementQuantity(productId));
    }
  };

  const formatPrice = (amount: number) => {
    return amount.toLocaleString('en-US', {
      style: 'currency',
      currency: 'USD',
    });
  };

  const cartTotal = () => {
    let total = 0;
    cartItems &&
      cartItems.map(
        (cartItem) => (total += cartItem.price * cartItem.orderQuantity)
      );
    return formatPrice(total);
  };

  return (
    <div className="cart">
      {cartItems && cartItems.length > 0 ? (
        <>
          <div className="cart-heading card">
            <h2>Shopping Cart [{cartItems.length}] items</h2>
            <button className="btn" onClick={handleRemoveAllProductsFromCart}>
              Remove all items from cart <i className="fas fa-trash-alt"></i>
            </button>
            <button
              className="btn"
              onClick={() => {
                navigate('/');
              }}
            >
              Shop More
            </button>
          </div>

          <div className="cart-body card flex-space-around">
            <div className="cart-items">
              {cartItems.map((cartItem) => (
                <div
                  className="cart-item card flex-space-around"
                  key={cartItem.productId}
                >
                  <div className="cart-item__left">
                    <img
                      className="cart-img"
                      src={cartItem.image}
                      alt={cartItem.name}
                    />
                  </div>
                  <div className="cart-item__center">
                    <p>{cartItem.name}</p>
                    <p>Price: {formatPrice(cartItem.price)}</p>
                    <p>In stock: {cartItem.quantity}</p>
                  </div>
                  <div className="cart-item__right">
                    <div className="quantity-controls">
                      <button
                        className="btn"
                        onClick={() => {
                          handleIncrementQuantity(cartItem.productId);
                        }}
                        disabled={cartItem.quantity === cartItem.orderQuantity}
                      >
                        +
                      </button>
                      <span>{cartItem.orderQuantity}</span>
                      <button
                        className="btn"
                        onClick={() => {
                          handleDecrementQuantity(cartItem.productId);
                        }}
                      >
                        -
                      </button>
                    </div>
                    <br />
                    <button
                      className="btn cart-btn"
                      onClick={() => {
                        handleRemoveFromCart(cartItem.productId);
                      }}
                    >
                      <i className="fas fa-trash-alt"></i> Remove
                    </button>
                  </div>
                </div>
              ))}
            </div>
            <div className="cart-summary">
              <h3>Cart Summary</h3>
              <h4>Total You have to pay : {cartTotal()}</h4>

              {isLoggedIn ? (
                <div>
                  <p>{userData && userData.address}</p>
                  <button className="btn">Update Delivery Address</button>
                  <br />
                  <button>Pay here</button>
                </div>
              ) : (
                <p className="warning">
                  login first for placing the order and updating the delivery
                  address
                </p>
              )}
            </div>
          </div>
        </>
      ) : (
        <p> No items in the cart</p>
      )}
    </div>
  );
};
```

#### add the css

```css
.cart-items {
  flex: 2;
}
.cart-item {
  margin: 1rem;
}
.cart-item-left {
  flex: 1;
}
.cart-item__center {
  flex: 1.5;
}
.cart-item__right {
  flex: 0.5;
}
.cart-img {
  height: 100px;
  width: 100px;
}
.cart-btn {
  border: none;
  border-radius: 50%;
}
.cart-summary {
  flex: 1;
}
```

### Place the order

### Payment with Braintree API / Stripe API

### List all the orders

### how to deploy

#### deploy postgres database

- Supabase is an open-source database infrastructure built on PostgreSQL. Using Supabase's simple setup and user interface, users can quickly build a complete web and mobile backend to really focus on building their product within minutes, without getting caught up in overly complex data structures.

- Go to Supabse: create a database and generate password: PXGGn3TAWR31E9hH
- previous setup

```json
 "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=5432;Database=full-ecommerce-db;Username=postgres;Password=new_password;"
  },
```

- get the connection string and replace in appsettings.json
  `User Id=postgres.crmluepmoyfqvpfdkpem;Password=PXGGn3TAWR31E9hH;Server=aws-0-us-east-1.pooler.supabase.com;Port=5432;Database=postgres;`

- delete previous migration script and create new

```json
dotnet ef migrations add InitialPro
dotnet ef database update
```

#### set environamenr variable

- first you can setup default value in appsetting.json
- env+enter list all env
- create .env file in the root directory

```shell
ConnectionStrings__DefaultConnection=User Id=postgres.crmluepmoyfqvpfdkpem;Password=PXGGn3TAWR31E9hH;Server=aws-0-us-east-1.pooler.supabase.com;Port=5432;Database=postgres;
Jwt__Key=YourJWTSecretKey
Jwt__Issuer=LocalhostIssuer
Jwt__Audience=LocalhostAudience
```

- Then, use a library like DotNetEnv to load these variables. First, add the DotNetEnv package to your project:

```bash
dotnet add package DotNetEnv
```

- update the program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);

// Load environment variables from .env file
DotNetEnv.Env.Load();
```

- update the program.cs

```csharp
// old Program.cs
using System.Text;
using System.Text.Json.Serialization;
using api.Middlewares;
using ECommerceAPI.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SDA Ecommerce API", Version = "v1" });

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });
// ReferenceHandler.IgnoreCycles => This option prevents the serializer from generating circular references by ignoring them, eliminating the $id and $ref properties.
// JsonIgnoreCondition.WhenWritingNull => This option ensures that null properties are not included in the JSON output.


builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<AuthService>();



// for jwt
// Add services to the DI container.
var Configuration = builder.Configuration; // Ensure this is accessible

var jwtKey = Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is missing in configuration.");

var key = Encoding.ASCII.GetBytes(jwtKey);


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", builder =>
    {
        builder.WithOrigins("http://localhost:3000")
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Set to true in production
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = Configuration["Jwt:Issuer"],
        ValidAudience = Configuration["Jwt:Audience"],
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));




var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowSpecificOrigins");
app.UseHttpsRedirection();
app.UseMiddleware<LoggingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

// new program.cs
using System.Text;
using System.Text.Json.Serialization;
using api.Middlewares;
using ECommerceAPI.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from .env file
DotNetEnv.Env.Load();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SDA Ecommerce API", Version = "v1" });

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });
// ReferenceHandler.IgnoreCycles => This option prevents the serializer from generating circular references by ignoring them, eliminating the $id and $ref properties.
// JsonIgnoreCondition.WhenWritingNull => This option ensures that null properties are not included in the JSON output.

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<AuthService>();

var Configuration = builder.Configuration;

// Log environment variables for debugging
Console.WriteLine($"OLDPWD: {Environment.GetEnvironmentVariable("OLDPWD")}");
Console.WriteLine($"Jwt__Key: {Environment.GetEnvironmentVariable("Jwt__Key")}");
Console.WriteLine($"Jwt__Issuer: {Environment.GetEnvironmentVariable("Jwt__Issuer")}");
Console.WriteLine($"Jwt__Audience: {Environment.GetEnvironmentVariable("Jwt__Audience")}");
Console.WriteLine($"DefaultConnection: {Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")}");

// Get JWT settings from environment variables
var jwtKey = Environment.GetEnvironmentVariable("Jwt__Key") ?? throw new InvalidOperationException("JWT Key is missing in environment variables.");
var jwtIssuer = Environment.GetEnvironmentVariable("Jwt__Issuer") ?? throw new InvalidOperationException("JWT Issuer is missing in environment variables.");
var jwtAudience = Environment.GetEnvironmentVariable("Jwt__Audience") ?? throw new InvalidOperationException("JWT Audience is missing in environment variables.");

// Get the database connection string from environment variables
var defaultConnection = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") ?? throw new InvalidOperationException("Default Connection is missing in environment variables.");

var key = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", builder =>
    {
        builder.WithOrigins("http://localhost:3000")
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Set to true in production
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(defaultConnection));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowSpecificOrigins");
app.UseHttpsRedirection();
app.UseMiddleware<LoggingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

```

#### deploy your web api

Deploying a .NET web API to Render is a straightforward process. Render is a cloud platform that makes it easy to deploy web applications and APIs with minimal configuration. Here’s a step-by-step guide to deploy a .NET web API to Render:

##### Prerequisites

1. **Render Account**: Sign up for a Render account at [render.com](https://render.com/).
2. **GitHub Repository**: Ensure your .NET web API is pushed to a GitHub repository.

##### Steps to Deploy a .NET Web API to Render

###### 1. Prepare Your .NET Web API

Ensure your .NET web API project is ready for deployment. Typically, this involves:

- Having a `.csproj` file in the root directory of your project.
- Ensuring the project builds and runs correctly locally.
- Adding a `Dockerfile` (optional, Render can build your project without it).

###### 2. Create a New Web Service on Render

1. **Login to Render**: Go to [render.com](https://render.com/) and log in to your account.

2. **Create New Web Service**:

   - Click the `New` button in the Render dashboard.
   - Select `Web Service`.

3. **Connect to GitHub**:

   - Connect your GitHub account to Render if you haven’t already.
   - Select the repository containing your .NET web API.

4. **Configure the Web Service**:

   - **Name**: Enter a name for your service.
   - **Branch**: Select the branch you want to deploy (e.g., `main`).
   - **Build Command**: Render automatically detects .NET projects and usually sets this to `dotnet publish -c Release -o out`.
   - **Start Command**: Render sets this to `dotnet yourapp.dll` (replace `yourapp.dll` with your project's DLL name, usually matching the project name).

5. **Environment**: Select the environment type (e.g., `Standard`).

6. **Region**: Select the deployment region.

7. **Instance Type**: Choose the instance type based on your resource requirements.

8. **Click `Create Web Service`**.

###### 3. Optional: Add Environment Variables

If your .NET web API requires environment variables (e.g., for database connection strings), you can add them in the Render dashboard:

1. Go to your web service settings.
2. Click on the `Environment` tab.
3. Add the required environment variables.

###### 4. Deploy Your Application

Render will automatically start the deployment process. It will:

- Clone the repository.
- Install dependencies.
- Build the application.
- Deploy the application.

You can monitor the build and deployment logs in the Render dashboard.

###### Example `Dockerfile` (Optional)

If you prefer using a Dockerfile for more control over the build process, here’s a basic example for a .NET web API:

```dockerfile
# Use the official .NET image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["YourProject.csproj", "./"]
RUN dotnet restore "YourProject.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "YourProject.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "YourProject.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "YourProject.dll"]
```

Replace `YourProject.csproj` and `YourProject.dll` with the actual names of your project files.

##### Access Your Application

Once the deployment is complete, Render will provide you with a URL where your .NET web API is hosted. You can use this URL to access your API endpoints.

###### Summary

Deploying a .NET web API to Render is easy and requires minimal setup. By following the steps above, you can quickly get your application running on a reliable cloud platform. Render handles the heavy lifting of infrastructure management, allowing you to focus on developing your application.
