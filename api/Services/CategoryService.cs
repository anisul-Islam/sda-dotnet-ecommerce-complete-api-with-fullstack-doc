
using api.EntityFrameworkCore;
using api.Helpers;
using api.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

public class CategoryService
{
    private readonly AppDbContext _appDbcontext;
    private readonly IMapper _mapper;
    public CategoryService(AppDbContext context, IMapper mapper)
    {
        _appDbcontext = context;
        _mapper = mapper;
    }

    public async Task<CategoryDto> AddCategoryAsync(CreateCategoryDto newCategoryData)
    {

        var category = _mapper.Map<Category>(newCategoryData);
        category.Slug = Helper.GenerateSlug(newCategoryData.Name);
        _appDbcontext.Categories.Add(category);
        await _appDbcontext.SaveChangesAsync();
        return _mapper.Map<CategoryDto>(category);
    }

    public async Task<PaginatedResult<CategoryDto>> GetAllCategoriesAsync(QueryParameters queryParams)
    {
        var query = _appDbcontext.Categories
                                  .Include(c => c.Products)
                                  .AsQueryable();

        if (!string.IsNullOrEmpty(queryParams.SearchTerm))
        {
            var lowerCaseSearchTerm = queryParams.SearchTerm.ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(lowerCaseSearchTerm));
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

    // public async Task<CategoryDto?> GetCategoryByIdAsync(Guid categoryId)
    // {
    //     var category = await _appDbcontext.Categories.FindAsync(categoryId);
    //     return category != null ? _mapper.Map<CategoryDto>(category) : null;
    // }

    // public async Task<CategoryDto?> GetCategoryBySlugAsync(string slug)
    // {
    //     var category = await _appDbcontext.Categories.SingleOrDefaultAsync(c => c.Slug == slug);
    //     return category != null ? _mapper.Map<CategoryDto>(category) : null;
    // }

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


    public async Task<bool> DeleteCategoryByIdentifierAsync(string identifier)
    {
        Category? category = null;

        if (Guid.TryParse(identifier, out Guid productId))
        {
            category = await _appDbcontext.Categories.FindAsync(productId);
        }
        else
        {
            category = await _appDbcontext.Categories.SingleOrDefaultAsync(c => c.Slug == identifier);
        }

        if (category == null)
        {
            return false;
        }

        _appDbcontext.Categories.Remove(category);
        await _appDbcontext.SaveChangesAsync();
        return true;
    }

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


}
