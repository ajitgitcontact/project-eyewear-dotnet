using backend.DTOs.UserDtos;
using backend.Services.UserService;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user is null)
        {
            return NotFound(new { message = "User not found." });
        }
        return Ok(user);
    }

    [HttpGet("email/{email}")]
    public async Task<IActionResult> GetByEmail(string email)
    {
        var user = await _userService.GetUserByEmailAsync(email);
        if (user is null)
        {
            return NotFound(new { message = "User not found." });
        }
        return Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        try
        {
            var created = await _userService.CreateUserAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
    {
        try
        {
            var user = await _userService.UpdateUserAsync(id, dto);
            if (user is null)
            {
                return NotFound(new { message = "User not found." });
            }
            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _userService.LoginAsync(dto);
        if (user is null)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }
        return Ok(user);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _userService.DeleteUserAsync(id);
        if (!deleted)
        {
            return NotFound(new { message = "User not found." });
        }
        return NoContent();
    }
}
