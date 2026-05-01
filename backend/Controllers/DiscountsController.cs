using backend.Application.Abstractions.Orders;
using backend.Constants;
using backend.DTOs.DiscountDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/admin/discounts")]
[Tags("Admin Discounts")]
[Authorize(Roles = Roles.Admin + "," + Roles.SuperAdmin)]
[Produces("application/json")]
public class DiscountsController : ControllerBase
{
    private readonly IAdminDiscountService _discountService;
    private readonly ILogger<DiscountsController> _logger;

    public DiscountsController(IAdminDiscountService discountService, ILogger<DiscountsController> logger)
    {
        _discountService = discountService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DiscountResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var discounts = await _discountService.GetAllAsync();
        return Ok(discounts);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(DiscountResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id)
    {
        return Ok(await _discountService.GetByIdAsync(id));
    }

    [HttpPost]
    [ProducesResponseType(typeof(DiscountResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateDiscountDto dto)
    {
        _logger.LogInformation("Admin discount create request received. AppliesTo={AppliesTo}", dto.AppliesTo);
        var discount = await _discountService.CreateAsync(dto);
        return Created($"/api/admin/discounts/{discount.DiscountId}", discount);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(DiscountResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateDiscountDto dto)
    {
        return Ok(await _discountService.UpdateAsync(id, dto));
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id)
    {
        await _discountService.DeleteAsync(id);
        return NoContent();
    }
}
