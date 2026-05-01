using System.Security.Claims;
using backend.Application.Abstractions.Orders;
using backend.Application.Exceptions;
using backend.Constants;
using backend.DTOs.OrderFetchDtos;
using backend.DTOs.OrderStatusLogDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/orders")]
[Tags("Orders")]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    private readonly IFetchCompleteOrderService _fetchCompleteOrderService;
    private readonly IOrderSearchService _orderSearchService;
    private readonly IOrderLogQueryService _orderLogQueryService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        IFetchCompleteOrderService fetchCompleteOrderService,
        IOrderSearchService orderSearchService,
        IOrderLogQueryService orderLogQueryService,
        ILogger<OrdersController> logger)
    {
        _fetchCompleteOrderService = fetchCompleteOrderService;
        _orderSearchService = orderSearchService;
        _orderLogQueryService = orderLogQueryService;
        _logger = logger;
    }

    /// <summary>
    /// Searches orders for admin order management.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = Roles.Admin + "," + Roles.SuperAdmin)]
    [ProducesResponseType(typeof(OrderSearchResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SearchOrders([FromQuery] OrderSearchRequestDto request)
    {
        var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
        _logger.LogInformation("Admin order search request received. Role={Role}", role);

        var result = await _orderSearchService.SearchAsync(request);
        _logger.LogInformation("Admin order search request completed. Role={Role}, TotalCount={TotalCount}, PageNumber={PageNumber}, PageSize={PageSize}", role, result.TotalCount, result.PageNumber, result.PageSize);
        return Ok(result);
    }

    /// <summary>
    /// Fetches one complete order by CustomerOrderId.
    /// </summary>
    /// <remarks>
    /// CUSTOMER users can fetch only their own orders. ADMIN and SUPER_ADMIN can fetch any order.
    /// Customer responses include limited payment details only.
    /// </remarks>
    [HttpGet("{customerOrderId}")]
    [Authorize(Roles = Roles.Customer + "," + Roles.Admin + "," + Roles.SuperAdmin)]
    [ProducesResponseType(typeof(CompleteOrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCompleteOrderByCustomerOrderId(string customerOrderId)
    {
        var userId = GetAuthenticatedUserId();
        var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        _logger.LogInformation("Complete order request received. CustomerOrderId={CustomerOrderId}, UserId={UserId}, Role={Role}", customerOrderId, userId, role);
        var result = await _fetchCompleteOrderService.GetByCustomerOrderIdAsync(customerOrderId, userId, role);
        _logger.LogInformation("Complete order request completed. CustomerOrderId={CustomerOrderId}, UserId={UserId}, Role={Role}", result.Order.CustomerOrderId, userId, role);

        return Ok(result);
    }

    /// <summary>
    /// Fetches complete order journey logs for Admin and Super Admin users.
    /// </summary>
    [HttpGet("{customerOrderId}/logs")]
    [Authorize(Roles = Roles.Admin + "," + Roles.SuperAdmin)]
    [ProducesResponseType(typeof(IEnumerable<OrderStatusLogResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrderLogs(string customerOrderId)
    {
        var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
        _logger.LogInformation("Order logs request received. CustomerOrderId={CustomerOrderId}, Role={Role}", customerOrderId, role);
        var logs = await _orderLogQueryService.GetLogsForAdminAsync(customerOrderId);
        return Ok(logs);
    }

    private int GetAuthenticatedUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
        {
            _logger.LogWarning("Order request blocked. Missing or invalid UserId claim.");
            throw new UnauthorizedException("Invalid authentication token.");
        }

        return userId;
    }
}
