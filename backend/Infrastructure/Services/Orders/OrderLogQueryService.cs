using System.Text.RegularExpressions;
using backend.Application.Abstractions.Orders;
using backend.Application.Exceptions;
using backend.DTOs.OrderStatusLogDtos;

namespace backend.Infrastructure.Services.Orders;

public class OrderLogQueryService : IOrderLogQueryService
{
    private static readonly Regex CustomerOrderIdRegex = new(@"^\d{11}$", RegexOptions.Compiled);
    private readonly IOrderService _orderService;
    private readonly IOrderStatusLogService _orderStatusLogService;
    private readonly ILogger<OrderLogQueryService> _logger;

    public OrderLogQueryService(IOrderService orderService, IOrderStatusLogService orderStatusLogService, ILogger<OrderLogQueryService> logger)
    {
        _orderService = orderService;
        _orderStatusLogService = orderStatusLogService;
        _logger = logger;
    }

    public async Task<IEnumerable<OrderStatusLogResponseDto>> GetLogsForAdminAsync(string customerOrderId)
    {
        var normalized = customerOrderId?.Trim() ?? string.Empty;
        if (!CustomerOrderIdRegex.IsMatch(normalized))
            throw new BadRequestException("CustomerOrderId must match YYMMDDXXXXX format.");

        await _orderService.GetByCustomerOrderIdAsync(normalized);
        var logs = await _orderStatusLogService.GetByCustomerOrderIdAsync(normalized);
        _logger.LogInformation("Order journey logs fetched. CustomerOrderId={CustomerOrderId}, Count={Count}", normalized, logs.Count());
        return logs;
    }
}
