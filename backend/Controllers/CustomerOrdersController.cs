using System.Security.Claims;
using backend.Application.Abstractions.Orders;
using backend.Application.Exceptions;
using backend.Constants;
using backend.DTOs.OrderFetchDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/customer/orders")]
[Tags("Customer Orders")]
[Produces("application/json")]
public class CustomerOrdersController : ControllerBase
{
    private readonly ICustomerOrderListService _customerOrderListService;
    private readonly ILogger<CustomerOrdersController> _logger;

    public CustomerOrdersController(ICustomerOrderListService customerOrderListService, ILogger<CustomerOrdersController> logger)
    {
        _customerOrderListService = customerOrderListService;
        _logger = logger;
    }

    /// <summary>
    /// Lists orders for the authenticated customer.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = Roles.Customer)]
    [ProducesResponseType(typeof(CustomerOrderListResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMyOrders([FromQuery] CustomerOrderListRequestDto request)
    {
        var userId = GetAuthenticatedUserId();
        _logger.LogInformation(
            "Customer order list request received. UserId={UserId}, FromCreatedDate={FromCreatedDate}, ToCreatedDate={ToCreatedDate}, OrderStatus={OrderStatus}, PaymentStatus={PaymentStatus}, PageNumber={PageNumber}, PageSize={PageSize}",
            userId,
            request.FromCreatedDate,
            request.ToCreatedDate,
            request.OrderStatus,
            request.PaymentStatus,
            request.PageNumber,
            request.PageSize);

        var result = await _customerOrderListService.GetForCustomerAsync(userId, request);
        _logger.LogInformation("Customer order list request completed. UserId={UserId}, TotalCount={TotalCount}", userId, result.TotalCount);
        return Ok(result);
    }

    private int GetAuthenticatedUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
        {
            _logger.LogWarning("Customer order list blocked. Missing or invalid UserId claim.");
            throw new UnauthorizedException("Invalid authentication token.");
        }

        return userId;
    }
}
