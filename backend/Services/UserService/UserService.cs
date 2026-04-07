using backend.Data;
using backend.DTOs.UserDtos;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services.UserService;

public class UserService : IUserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
    {
        return await _context.Users
            .Select(u => MapToResponseDto(u))
            .ToListAsync();
    }

    public async Task<UserResponseDto?> GetUserByIdAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        return user is null ? null : MapToResponseDto(user);
    }

    public async Task<UserResponseDto?> GetUserByEmailAsync(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        return user is null ? null : MapToResponseDto(user);
    }

    public async Task<UserResponseDto> CreateUserAsync(CreateUserDto dto)
    {
        var emailExists = await _context.Users.AnyAsync(u => u.Email == dto.Email);
        if (emailExists)
        {
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
        return MapToResponseDto(user);
    }

    public async Task<UserResponseDto?> UpdateUserAsync(int id, UpdateUserDto dto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user is null)
        {
            return null;
        }

        if (user.Email != dto.Email)
        {
            var emailTaken = await _context.Users.AnyAsync(u => u.Email == dto.Email);
            if (emailTaken)
            {
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
        return MapToResponseDto(user);
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user is null)
        {
            return false;
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<UserResponseDto?> LoginAsync(LoginDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            return null;
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
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
