using backend.DTOs.UserDtos;

namespace backend.Application.Abstractions.Users;

public interface IUserService
{
    Task<IEnumerable<UserResponseDto>> GetAllUsersAsync();
    Task<UserResponseDto> GetUserByIdAsync(int id);
    Task<UserResponseDto> GetUserByEmailAsync(string email);
    Task<UserResponseDto> CreateUserAsync(CreateUserDto dto);
    Task<UserResponseDto> UpdateUserAsync(int id, UpdateUserDto dto);
    Task DeleteUserAsync(int id);
    Task<UserResponseDto> LoginAsync(LoginDto dto);
}
