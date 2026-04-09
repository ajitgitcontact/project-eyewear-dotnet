using backend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DbTestController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<DbTestController> _logger;

    public DbTestController(AppDbContext context, ILogger<DbTestController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> TestConnection()
    {
        _logger.LogInformation("DB connectivity test requested.");

        var canConnect = await _context.Database.CanConnectAsync();
        if (!canConnect)
        {
            _logger.LogWarning("DB connectivity test failed.");
            return StatusCode(500, new { status = "failure", message = "Cannot connect to database." });
        }

        _logger.LogInformation("DB connectivity test succeeded.");
        return Ok(new { status = "success", message = "Database connection is working." });
    }
}
