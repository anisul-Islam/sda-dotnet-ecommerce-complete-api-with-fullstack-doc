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

        [HttpGet]
        public async Task<IActionResult> GetAllProducts([FromQuery] QueryParameters queryParams)
        {
            var result = await _productService.GetAllProductsAsync(queryParams);
            return result.Items.Any()
                ? ApiResponse.Success(result, "Products retrieved successfully.")
                : ApiResponse.NotFound("No products found.");
        }

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


    }
}
