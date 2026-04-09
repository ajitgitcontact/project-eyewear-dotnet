using backend.DTOs.UserDtos;
using backend.Services.UserService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogInformation("GetAll users request received.");
        var users = await _userService.GetAllUsersAsync();
        _logger.LogInformation("GetAll users succeeded.");
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        _logger.LogInformation("Get user by id request received. UserId={UserId}", id);
        var user = await _userService.GetUserByIdAsync(id);
        if (user is null)
        {
            _logger.LogInformation("Get user by id not found. UserId={UserId}", id);
            return NotFound(new { message = "User not found." });
        }
        _logger.LogInformation("Get user by id succeeded. UserId={UserId}", id);
        return Ok(user);
    }

    [HttpGet("email/{email}")]
    public async Task<IActionResult> GetByEmail(string email)
    {
        _logger.LogInformation("Get user by email request received.");
        var user = await _userService.GetUserByEmailAsync(email);
        if (user is null)
        {
            _logger.LogInformation("Get user by email not found.");
            return NotFound(new { message = "User not found." });
        }
        _logger.LogInformation("Get user by email succeeded. UserId={UserId}", user.Id);
        return Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        _logger.LogInformation("Create user request received.");
        try
        {
            var created = await _userService.CreateUserAsync(dto);
            _logger.LogInformation("Create user succeeded. UserId={UserId}", created.Id);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Create user failed due to business validation.");
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
    {
        _logger.LogInformation("Update user request received. UserId={UserId}", id);
        try
        {
            var user = await _userService.UpdateUserAsync(id, dto);
            if (user is null)
            {
                _logger.LogInformation("Update user not found. UserId={UserId}", id);
                return NotFound(new { message = "User not found." });
            }
            _logger.LogInformation("Update user succeeded. UserId={UserId}", id);
            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Update user failed due to business validation. UserId={UserId}", id);
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        _logger.LogInformation("Login request received.");
        var user = await _userService.LoginAsync(dto);
        if (user is null)
        {
            _logger.LogInformation("Login failed.");
            return Unauthorized(new { message = "Invalid email or password." });
        }
        _logger.LogInformation("Login succeeded. UserId={UserId}", user.Id);
        return Ok(user);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogInformation("Delete user request received. UserId={UserId}", id);
        var deleted = await _userService.DeleteUserAsync(id);
        if (!deleted)
        {
            _logger.LogInformation("Delete user not found. UserId={UserId}", id);
            return NotFound(new { message = "User not found." });
        }
        _logger.LogInformation("Delete user succeeded. UserId={UserId}", id);
        return NoContent();
    }
}
