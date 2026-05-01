using backend.Application.Abstractions.Orders;
using backend.Constants;
using backend.DTOs.CouponDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/admin/coupons")]
[Tags("Admin Coupons")]
[Authorize(Roles = Roles.Admin + "," + Roles.SuperAdmin)]
[Produces("application/json")]
public class CouponsController : ControllerBase
{
    private readonly IAdminCouponService _couponService;
    private readonly ILogger<CouponsController> _logger;

    public CouponsController(IAdminCouponService couponService, ILogger<CouponsController> logger)
    {
        _couponService = couponService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CouponResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _couponService.GetAllAsync());
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CouponResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id)
    {
        return Ok(await _couponService.GetByIdAsync(id));
    }

    [HttpPost]
    [ProducesResponseType(typeof(CouponResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateCouponDto dto)
    {
        _logger.LogInformation("Admin coupon create request received.");
        var coupon = await _couponService.CreateAsync(dto);
        return Created($"/api/admin/coupons/{coupon.CouponId}", coupon);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CouponResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateCouponDto dto)
    {
        return Ok(await _couponService.UpdateAsync(id, dto));
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id)
    {
        await _couponService.DeleteAsync(id);
        return NoContent();
    }
}
