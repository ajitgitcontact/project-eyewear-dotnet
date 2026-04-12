using System.Security.Claims;
using backend.Application.Abstractions.Users;
using backend.Constants;
using backend.DTOs.UserDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ITokenService _tokenService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ITokenService tokenService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _tokenService = tokenService;
        _logger = logger;
    }

    // ── Public: Login ─────────────────────────────────────────────────────────

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        _logger.LogInformation("Login request received.");
        var user = await _userService.LoginAsync(dto);
        var token = _tokenService.GenerateToken(user);
        _logger.LogInformation("Login succeeded. UserId={UserId}, Role={Role}", user.Id, user.UserRole);
        return Ok(new LoginResponseDto { Token = token, Role = user.UserRole });
    }

    // ── Public: Register (non-admins can only create CUSTOMER accounts) ───────

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        _logger.LogInformation("Create user request received.");

        if (!Enum.TryParse<Roles.RoleEnum>(dto.UserRole, ignoreCase: true, out var parsedRole))
            return BadRequest(new { message = "Invalid role. Valid roles: SUPER_ADMIN, ADMIN, CUSTOMER" });

        var callerRole = User.FindFirstValue(ClaimTypes.Role);
        bool callerIsAdmin = callerRole == Roles.Admin || callerRole == Roles.SuperAdmin;

        if (!callerIsAdmin && parsedRole != Roles.RoleEnum.CUSTOMER)
        {
            _logger.LogWarning("Unauthorized role assignment attempt. Requested={Role}", dto.UserRole);
            return Forbid();
        }

        // Always store role from constants — never trust raw frontend string
        dto.UserRole = parsedRole switch
        {
            Roles.RoleEnum.SUPER_ADMIN => Roles.SuperAdmin,
            Roles.RoleEnum.ADMIN => Roles.Admin,
            _ => Roles.Customer
        };

        var created = await _userService.CreateUserAsync(dto);
        _logger.LogInformation("Create user succeeded. UserId={UserId}", created.Id);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // ── Admin/SuperAdmin: List all users ──────────────────────────────────────

    [HttpGet]
    [Authorize(Roles = Roles.Admin + "," + Roles.SuperAdmin)]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogInformation("GetAll users request received.");
        var users = await _userService.GetAllUsersAsync();
        _logger.LogInformation("GetAll users succeeded. Count={Count}", users.Count());
        return Ok(users);
    }

    // ── Authorized: Admin/SuperAdmin OR own profile ───────────────────────────

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
    {
        _logger.LogInformation("Get user by id request received. UserId={UserId}", id);

        if (!IsAdminOrOwner(id))
            return Forbid();

        var user = await _userService.GetUserByIdAsync(id);
        _logger.LogInformation("Get user by id succeeded. UserId={UserId}", id);
        return Ok(user);
    }

    // ── Admin/SuperAdmin only ─────────────────────────────────────────────────

    [HttpGet("email/{email}")]
    [Authorize(Roles = Roles.Admin + "," + Roles.SuperAdmin)]
    public async Task<IActionResult> GetByEmail(string email)
    {
        _logger.LogInformation("Get user by email request received.");
        var user = await _userService.GetUserByEmailAsync(email);
        _logger.LogInformation("Get user by email succeeded. UserId={UserId}", user.Id);
        return Ok(user);
    }

    // ── Authorized: Admin/SuperAdmin OR own profile ───────────────────────────

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
    {
        _logger.LogInformation("Update user request received. UserId={UserId}", id);

        if (!IsAdminOrOwner(id))
            return Forbid();

        if (!Enum.TryParse<Roles.RoleEnum>(dto.UserRole, ignoreCase: true, out var parsedRole))
            return BadRequest(new { message = "Invalid role. Valid roles: SUPER_ADMIN, ADMIN, CUSTOMER" });

        var callerRole = User.FindFirstValue(ClaimTypes.Role);
        bool callerIsAdmin = callerRole == Roles.Admin || callerRole == Roles.SuperAdmin;

        if (!callerIsAdmin && parsedRole != Roles.RoleEnum.CUSTOMER)
        {
            _logger.LogWarning("Role escalation blocked. UserId={UserId}, Requested={Role}", id, dto.UserRole);
            return Forbid();
        }

        dto.UserRole = parsedRole switch
        {
            Roles.RoleEnum.SUPER_ADMIN => Roles.SuperAdmin,
            Roles.RoleEnum.ADMIN => Roles.Admin,
            _ => Roles.Customer
        };

        var user = await _userService.UpdateUserAsync(id, dto);
        _logger.LogInformation("Update user succeeded. UserId={UserId}", id);
        return Ok(user);
    }

    // ── Admin/SuperAdmin only ─────────────────────────────────────────────────

    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.Admin + "," + Roles.SuperAdmin)]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogInformation("Delete user request received. UserId={UserId}", id);
        await _userService.DeleteUserAsync(id);
        _logger.LogInformation("Delete user succeeded. UserId={UserId}", id);
        return NoContent();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private bool IsAdminOrOwner(int resourceUserId)
    {
        var callerRole = User.FindFirstValue(ClaimTypes.Role);
        if (callerRole == Roles.Admin || callerRole == Roles.SuperAdmin)
            return true;

        var callerIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(callerIdClaim, out var callerId) && callerId == resourceUserId;
    }
}

