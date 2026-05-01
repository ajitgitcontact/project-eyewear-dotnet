using System.Security.Claims;
using backend.Application.Abstractions.Orders;
using backend.Application.Exceptions;
using backend.Constants;
using backend.DTOs.CartDtos;
using backend.DTOs.OrderCreationDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/cart")]
[Tags("Cart")]
[Authorize(Roles = Roles.Customer)]
[Produces("application/json")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;
    private readonly ILogger<CartController> _logger;

    public CartController(ICartService cartService, ILogger<CartController> logger)
    {
        _cartService = cartService;
        _logger = logger;
    }

    /// <summary>
    /// Returns the logged-in customer's active cart, or an empty cart response if none exists.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(CartResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Get()
    {
        var userId = GetAuthenticatedUserId();
        _logger.LogInformation("Cart fetch request received. UserId={UserId}", userId);
        return Ok(await _cartService.GetActiveCartAsync(userId));
    }

    /// <summary>
    /// Adds a product, selected customizations, and optional prescription snapshot to the active cart.
    /// </summary>
    [HttpPost("items")]
    [ProducesResponseType(typeof(CartResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddItem([FromBody] AddCartItemDto dto)
    {
        var userId = GetAuthenticatedUserId();
        _logger.LogInformation("Cart add item request received. UserId={UserId}, ProductId={ProductId}", userId, dto.ProductId);
        return Ok(await _cartService.AddItemAsync(userId, dto));
    }

    /// <summary>
    /// Updates quantity for a cart item owned by the logged-in customer.
    /// </summary>
    [HttpPut("items/{cartItemId}")]
    [ProducesResponseType(typeof(CartResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateItemQuantity(string cartItemId, [FromBody] UpdateCartItemQuantityDto dto)
    {
        var userId = GetAuthenticatedUserId();
        return Ok(await _cartService.UpdateItemQuantityAsync(userId, cartItemId, dto));
    }

    /// <summary>
    /// Removes a cart item from the active cart without deleting historical checked-out cart snapshots.
    /// </summary>
    [HttpDelete("items/{cartItemId}")]
    [ProducesResponseType(typeof(CartResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveItem(string cartItemId)
    {
        var userId = GetAuthenticatedUserId();
        return Ok(await _cartService.RemoveItemAsync(userId, cartItemId));
    }

    /// <summary>
    /// Clears all active items and any applied coupon from the active cart.
    /// </summary>
    [HttpDelete("clear")]
    [ProducesResponseType(typeof(CartResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Clear()
    {
        var userId = GetAuthenticatedUserId();
        return Ok(await _cartService.ClearAsync(userId));
    }

    /// <summary>
    /// Applies a coupon preview to the active cart. Coupon is revalidated again during checkout.
    /// </summary>
    [HttpPost("apply-coupon")]
    [ProducesResponseType(typeof(CartResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ApplyCoupon([FromBody] ApplyCartCouponDto dto)
    {
        var userId = GetAuthenticatedUserId();
        return Ok(await _cartService.ApplyCouponAsync(userId, dto));
    }

    /// <summary>
    /// Removes the coupon preview from the active cart.
    /// </summary>
    [HttpDelete("remove-coupon")]
    [ProducesResponseType(typeof(CartResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveCoupon()
    {
        var userId = GetAuthenticatedUserId();
        return Ok(await _cartService.RemoveCouponAsync(userId));
    }

    /// <summary>
    /// Converts the active cart into an order using the existing backend-only order creation calculation flow.
    /// </summary>
    /// <remarks>
    /// Send Idempotency-Key header for retry-safe checkout submissions. Reusing the same key for the same customer returns the existing order instead of creating a duplicate.
    /// </remarks>
    [HttpPost("checkout")]
    [ProducesResponseType(typeof(OrderCreationResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Checkout(
        [FromBody] CartCheckoutRequestDto dto,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey)
    {
        var userId = GetAuthenticatedUserId();
        var result = await _cartService.CheckoutAsync(userId, dto, NormalizeIdempotencyKey(idempotencyKey));
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

    private int GetAuthenticatedUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedException("Invalid authentication token.");

        return userId;
    }
}
