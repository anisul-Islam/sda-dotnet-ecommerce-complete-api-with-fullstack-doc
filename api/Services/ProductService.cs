
using api.EntityFrameworkCore;
using api.Helpers;
using api.Models;
using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
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

    if (queryParams.MinPrice.HasValue)
    {
      query = query.Where(p => p.Price >= queryParams.MinPrice.Value);
    }

    if (queryParams.MaxPrice.HasValue)
    {
      query = query.Where(p => p.Price <= queryParams.MaxPrice.Value);
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

  public async Task<ProductDto?> GetProductByIdentifierAsync(string identifier)
  {
    Product? product = null;

    if (Guid.TryParse(identifier, out Guid productId))
    {
      product = await _appDbcontext.Products
          .Include(p => p.Categories)
          .SingleOrDefaultAsync(p => p.ProductId == productId);
    }
    else
    {
      product = await _appDbcontext.Products
          .Include(p => p.Categories)
          .SingleOrDefaultAsync(p => p.Slug == identifier);
    }

    return product != null ? _mapper.Map<ProductDto>(product) : null;
  }

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



}
