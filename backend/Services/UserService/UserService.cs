using backend.Data;
using backend.DTOs.UserDtos;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace backend.Services.UserService;

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly ILogger<UserService> _logger;

    public UserService(AppDbContext context, ILogger<UserService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
    {
        _logger.LogInformation("Fetching all users.");
        var users = await _context.Users
            .Select(u => MapToResponseDto(u))
            .ToListAsync();
        _logger.LogInformation("Fetched all users. Output: Count={Count}", users.Count);
        return users;
    }

    public async Task<UserResponseDto?> GetUserByIdAsync(int id)
    {
        _logger.LogInformation("Fetching user by id. Input: UserId={UserId}", id);
        var user = await _context.Users.FindAsync(id);
        if (user is null)
        {
            _logger.LogInformation("Get user by id result. Input: UserId={UserId} => Output: Found=false", id);
            return null;
        }
        _logger.LogInformation("Get user by id result. Input: UserId={UserId} => Output: Found=true, Email={Email}, Role={Role}", id, user.Email, user.UserRole);
        return MapToResponseDto(user);
    }

    public async Task<UserResponseDto?> GetUserByEmailAsync(string email)
    {
        _logger.LogInformation("Fetching user by email. Input: Email={Email}", email);
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user is null)
        {
            _logger.LogInformation("Get user by email result. Input: Email={Email} => Output: Found=false", email);
            return null;
        }
        _logger.LogInformation("Get user by email result. Input: Email={Email} => Output: Found=true, UserId={UserId}, Role={Role}", email, user.Id, user.UserRole);
        return MapToResponseDto(user);
    }

    public async Task<UserResponseDto> CreateUserAsync(CreateUserDto dto)
    {
        _logger.LogInformation("Creating user. Input: Name='{FirstName} {LastName}', Role={Role}", dto.FirstName, dto.LastName, dto.UserRole);
        var emailExists = await _context.Users.AnyAsync(u => u.Email == dto.Email);
        if (emailExists)
        {
            _logger.LogError("Create user blocked. Email already exists.");
            throw new InvalidOperationException("A user with this email already exists.");
        }

        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            ContactNumber = dto.ContactNumber,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            UserRole = dto.UserRole,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        _logger.LogInformation("User created. Input: Name='{FirstName} {LastName}', Role={Role} => Output: UserId={UserId}", user.FirstName, user.LastName, user.UserRole, user.Id);
        return MapToResponseDto(user);
    }

    public async Task<UserResponseDto?> UpdateUserAsync(int id, UpdateUserDto dto)
    {
        _logger.LogInformation("Updating user. Input: UserId={UserId}, NewRole={Role}, IsActive={IsActive}", id, dto.UserRole, dto.IsActive);
        var user = await _context.Users.FindAsync(id);
        if (user is null)
        {
            _logger.LogInformation("Update user not found. UserId={UserId}", id);
            return null;
        }

        if (user.Email != dto.Email)
        {
            var emailTaken = await _context.Users.AnyAsync(u => u.Email == dto.Email);
            if (emailTaken)
            {
                _logger.LogError("Update user blocked. Email already exists. UserId={UserId}", id);
                throw new InvalidOperationException("A user with this email already exists.");
            }
        }

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.Email = dto.Email;
        user.ContactNumber = dto.ContactNumber;
        user.UserRole = dto.UserRole;
        user.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();
        _logger.LogInformation("User updated. Input: UserId={UserId} => Output: Role={Role}, IsActive={IsActive}", user.Id, user.UserRole, user.IsActive);
        return MapToResponseDto(user);
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        _logger.LogInformation("Deleting user. Input: UserId={UserId}", id);
        var user = await _context.Users.FindAsync(id);
        if (user is null)
        {
            _logger.LogInformation("Delete user result. Input: UserId={UserId} => Output: Found=false, Deleted=false", id);
            return false;
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Delete user result. Input: UserId={UserId} => Output: Deleted=true", id);
        return true;
    }

    public async Task<UserResponseDto?> LoginAsync(LoginDto dto)
    {
        _logger.LogInformation("Login attempt. Input: Email={Email}", dto.Email);
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            _logger.LogInformation("Login result. Input: Email={Email} => Output: Authenticated=false", dto.Email);
            return null;
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        _logger.LogInformation("Login result. Input: Email={Email} => Output: Authenticated=true, UserId={UserId}, Role={Role}", dto.Email, user.Id, user.UserRole);
        return MapToResponseDto(user);
    }

    private static UserResponseDto MapToResponseDto(User user)
    {
        return new UserResponseDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            ContactNumber = user.ContactNumber,
            UserRole = user.UserRole,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }
}
