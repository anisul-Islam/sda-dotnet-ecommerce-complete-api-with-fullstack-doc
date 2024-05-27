using api.Dtos;
using api.Middlewares;
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

    public async Task<PaginatedResult<UserDto>> GetAllUsersAsync(QueryParameters queryParams)
    {
        var query = _appDbcontext.Users.AsQueryable();

        // Exclude users with admin privileges
        query = query.Where(u => !u.IsAdmin);

        if (!string.IsNullOrEmpty(queryParams.SearchTerm))
        {
            var lowerCaseSearchTerm = queryParams.SearchTerm.ToLower();
            query = query.Where(u => u.Name.Contains(lowerCaseSearchTerm) || u.Email.Contains(lowerCaseSearchTerm));
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
        user.Password = _passwordHasher.HashPassword(user, newUserData.Password);


        _appDbcontext.Users.Add(user);
        await _appDbcontext.SaveChangesAsync();
        return _mapper.Map<UserDto>(user);
    }

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
