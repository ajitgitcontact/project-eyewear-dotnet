using System.Security.Claims;
using backend.Application.Abstractions.Orders;
using backend.Application.Exceptions;
using backend.Constants;
using backend.DTOs.CartDtos;
using backend.DTOs.WishlistDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/wishlist")]
[Tags("Wishlist")]
[Authorize(Roles = Roles.Customer)]
[Produces("application/json")]
public class WishlistController : ControllerBase
{
    private readonly IWishlistService _wishlistService;
    private readonly ILogger<WishlistController> _logger;

    public WishlistController(IWishlistService wishlistService, ILogger<WishlistController> logger)
    {
        _wishlistService = wishlistService;
        _logger = logger;
    }

    /// <summary>
    /// Returns the logged-in customer's wishlist.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(WishlistResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Get()
    {
        var userId = GetAuthenticatedUserId();
        _logger.LogInformation("Wishlist fetch request received. UserId={UserId}", userId);
        return Ok(await _wishlistService.GetAsync(userId));
    }

    /// <summary>
    /// Adds a product to the logged-in customer's wishlist.
    /// </summary>
    [HttpPost("items")]
    [ProducesResponseType(typeof(WishlistResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddItem([FromBody] AddWishlistItemDto dto)
    {
        var userId = GetAuthenticatedUserId();
        _logger.LogInformation("Wishlist add item request received. UserId={UserId}, ProductId={ProductId}", userId, dto.ProductId);
        return Ok(await _wishlistService.AddItemAsync(userId, dto));
    }

    /// <summary>
    /// Removes a wishlist item owned by the logged-in customer.
    /// </summary>
    [HttpDelete("items/{wishlistItemId}")]
    [ProducesResponseType(typeof(WishlistResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveItem(string wishlistItemId)
    {
        var userId = GetAuthenticatedUserId();
        return Ok(await _wishlistService.RemoveItemAsync(userId, wishlistItemId));
    }

    /// <summary>
    /// Moves a wishlist item to cart when the product does not require extra configuration.
    /// </summary>
    [HttpPost("items/{wishlistItemId}/move-to-cart")]
    [ProducesResponseType(typeof(MoveWishlistItemToCartResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> MoveToCart(string wishlistItemId, [FromBody] MoveWishlistItemToCartDto dto)
    {
        var userId = GetAuthenticatedUserId();
        return Ok(await _wishlistService.MoveToCartAsync(userId, wishlistItemId, dto));
    }

    private int GetAuthenticatedUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedException("Invalid authentication token.");

        return userId;
    }
}
