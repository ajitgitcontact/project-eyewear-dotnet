using backend.DTOs.UserDtos;
using backend.Application.Abstractions.Users;
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
        _logger.LogInformation("Get user by id succeeded. UserId={UserId}", id);
        return Ok(user);
    }

    [HttpGet("email/{email}")]
    public async Task<IActionResult> GetByEmail(string email)
    {
        _logger.LogInformation("Get user by email request received.");
        var user = await _userService.GetUserByEmailAsync(email);
        _logger.LogInformation("Get user by email succeeded. UserId={UserId}", user.Id);
        return Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        _logger.LogInformation("Create user request received.");
        var created = await _userService.CreateUserAsync(dto);
        _logger.LogInformation("Create user succeeded. UserId={UserId}", created.Id);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
    {
        _logger.LogInformation("Update user request received. UserId={UserId}", id);
        var user = await _userService.UpdateUserAsync(id, dto);
        _logger.LogInformation("Update user succeeded. UserId={UserId}", id);
        return Ok(user);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        _logger.LogInformation("Login request received.");
        var user = await _userService.LoginAsync(dto);
        _logger.LogInformation("Login succeeded. UserId={UserId}", user.Id);
        return Ok(user);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogInformation("Delete user request received. UserId={UserId}", id);
        await _userService.DeleteUserAsync(id);
        _logger.LogInformation("Delete user succeeded. UserId={UserId}", id);
        return NoContent();
    }
}
