using backend.Application.Abstractions.Users;
using backend.Constants;
using backend.DTOs.UserDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUserService userService, ITokenService tokenService, ILogger<AuthController> logger)
    {
        _userService = userService;
        _tokenService = tokenService;
        _logger = logger;
    }

    [HttpPost("signup")]
    [AllowAnonymous]
    public async Task<IActionResult> SignUp([FromBody] SignUpDto dto)
    {
        _logger.LogInformation("SignUp request received.");

        // Customer sign-up should never trust frontend-provided role.
        var createUserDto = new CreateUserDto
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            ContactNumber = dto.ContactNumber,
            Password = dto.Password,
            UserRole = Roles.Customer
        };

        var created = await _userService.CreateUserAsync(createUserDto);
        var token = _tokenService.GenerateToken(created);

        _logger.LogInformation("SignUp succeeded. UserId={UserId}", created.Id);
        return Ok(new LoginResponseDto
        {
            Token = token,
            Role = created.UserRole
        });
    }
}
