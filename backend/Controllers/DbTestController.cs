using backend.Data;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DbTestController : ControllerBase
{
    private readonly AppDbContext _context;

    public DbTestController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> TestConnection()
    {
        try
        {
            var canConnect = await _context.Database.CanConnectAsync();
            if (canConnect)
            {
                return Ok(new { status = "success", message = "Database connection is working." });
            }
            return StatusCode(500, new { status = "failure", message = "Cannot connect to database." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { status = "error", message = ex.Message, inner = ex.InnerException?.Message });
        }
    }
}
