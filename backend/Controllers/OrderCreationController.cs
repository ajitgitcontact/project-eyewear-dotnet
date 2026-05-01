using System.Security.Claims;
using backend.Application.Abstractions.Orders;
using backend.Application.Exceptions;
using backend.Constants;
using backend.DTOs.OrderCreationDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/orders")]
[Tags("Orders")]
[Produces("application/json")]
public class OrderCreationController : ControllerBase
{
    private readonly IOrderCreationService _orderCreationService;
    private readonly ILogger<OrderCreationController> _logger;

    public OrderCreationController(IOrderCreationService orderCreationService, ILogger<OrderCreationController> logger)
    {
        _orderCreationService = orderCreationService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a complete checkout order in a single transaction.
    /// </summary>
    /// <remarks>
    /// CustomerOrderId, UserId, item prices, totals, payment status, and order status are generated or calculated by the backend.
    /// Send Idempotency-Key header for retry-safe checkout submissions. Reusing the same key for the same customer returns the existing order instead of creating a duplicate.
    /// Active admin discounts are applied automatically. Customers may pass CouponCode only; coupon amounts and final totals from the frontend are ignored.
    /// The response includes original subtotal, product discount total, coupon discount amount, and final payable amount snapshots.
    /// </remarks>
    [HttpPost("create")]
    [Authorize(Roles = Roles.Customer + "," + Roles.Admin + "," + Roles.SuperAdmin)]
    [ProducesResponseType(typeof(OrderCreationResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create(
        [FromBody] OrderCreationRequestDto? dto,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey)
    {
        _logger.LogInformation("Order creation request received.");

        if (dto is null)
        {
            _logger.LogWarning("Order creation blocked. Request body is empty.");
            throw new BadRequestException("Request body is required.");
        }

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
        {
            _logger.LogWarning("Order creation blocked. Missing or invalid UserId claim.");
            throw new UnauthorizedException("Invalid authentication token.");
        }

        idempotencyKey = NormalizeIdempotencyKey(idempotencyKey);
        _logger.LogInformation("Order creation request authenticated. UserId={UserId}, IdempotencyKeyProvided={IdempotencyKeyProvided}", userId, idempotencyKey is not null);
        var result = await _orderCreationService.CreateAsync(userId, dto, idempotencyKey);
        _logger.LogInformation("Order creation request completed. UserId={UserId}, CustomerOrderId={CustomerOrderId}", userId, result.CustomerOrderId);

        return Created($"/api/orders/{result.CustomerOrderId}", result);
    }

    private static string? NormalizeIdempotencyKey(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        value = value.Trim();
        if (value.Length > 100)
            throw new BadRequestException("Idempotency-Key must be 100 characters or fewer.");

        return value;
    }
}
