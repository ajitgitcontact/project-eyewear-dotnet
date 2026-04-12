using backend.DTOs.UserDtos;

namespace backend.Application.Abstractions.Users;

public interface ITokenService
{
    string GenerateToken(UserResponseDto user);
}
