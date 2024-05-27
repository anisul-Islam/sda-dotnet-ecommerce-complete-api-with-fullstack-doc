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


    private IActionResult HandleNullResult<T>(T result, string notFoundMessage)
    {
      return result != null
          ? ApiResponse.Success(result, notFoundMessage.Replace("not found", "retrieved successfully"))
          : ApiResponse.NotFound(notFoundMessage);
    }

    [Authorize(Roles = "Admin")]
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

    [HttpGet]
    public async Task<IActionResult> GetAllCategories([FromQuery] QueryParameters queryParams)
    {
      var result = await _categoryService.GetAllCategoriesAsync(queryParams);
      return result.Items.Any()
          ? ApiResponse.Success(result, "Categories retrieved successfully.")
          : ApiResponse.NotFound("No Categories found.");
    }

    // [HttpGet("{identifier}")]
    // public async Task<IActionResult> GetCategoryByIdOrSlug(string identifier)
    // {
    //   if (Guid.TryParse(identifier, out Guid id))
    //   {
    //     var category = await _categoryService.GetCategoryByIdAsync(id);
    //     if (category == null)
    //     {
    //       return ApiResponse.NotFound("Category not found");
    //     }
    //     return ApiResponse.Success(category, "Category retrieved successfully");
    //   }
    //   else
    //   {
    //     var category = await _categoryService.GetCategoryBySlugAsync(identifier);
    //     if (category == null)
    //     {
    //       return ApiResponse.NotFound("Category not found");
    //     }
    //     return ApiResponse.Success(category, "Category retrieved successfully");
    //   }
    // }

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


    [Authorize(Roles = "Admin")]
    [HttpDelete("{identifier}")]
    public async Task<IActionResult> DeleteCategoryByIdentifier(string identifier)
    {
      if (!ModelState.IsValid)
      {
        return ApiResponse.BadRequest("Invalid category data provided.");
      }
      var result = await _categoryService.DeleteCategoryByIdentifierAsync(identifier);
      return result ? NoContent() : ApiResponse.NotFound($"Category with {identifier} not found.");
    }

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


  }
}
