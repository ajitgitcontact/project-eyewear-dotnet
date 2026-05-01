using backend.Application.Abstractions.Orders;
using backend.Application.Exceptions;
using backend.Data;
using backend.DTOs.OrderStatusLogDtos;
using backend.Models.Orders;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Services.Orders;

public class OrderStatusLogService : IOrderStatusLogService
{
    private readonly AppDbContext _context;
    private readonly ILogger<OrderStatusLogService> _logger;

    public OrderStatusLogService(AppDbContext context, ILogger<OrderStatusLogService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<OrderStatusLogResponseDto>> GetByCustomerOrderIdAsync(string customerOrderId)
    {
        _logger.LogInformation("Fetching order status logs. Input: CustomerOrderId={CustomerOrderId}", customerOrderId);
        var logs = await _context.OrderStatusLogs
            .Where(osl => osl.CustomerOrderId == customerOrderId)
            .OrderBy(osl => osl.CreatedAt)
            .Select(osl => MapToDto(osl))
            .ToListAsync();
        _logger.LogInformation("Fetched order status logs. Input: CustomerOrderId={CustomerOrderId} => Output: Count={Count}", customerOrderId, logs.Count);
        return logs;
    }

    public async Task<OrderStatusLogResponseDto> GetByIdAsync(string id)
    {
        _logger.LogInformation("Fetching order status log by id. Input: OrderStatusLogsId={OrderStatusLogsId}", id);
        var log = await _context.OrderStatusLogs.FindAsync(id);
        if (log is null)
            throw new NotFoundException("Order status log not found.");

        return MapToDto(log);
    }

    public async Task<OrderStatusLogResponseDto> CreateAsync(CreateOrderStatusLogDto dto)
    {
        _logger.LogInformation("Creating order status log. Input: CustomerOrderId={CustomerOrderId}, Status={Status}", dto.CustomerOrderId, dto.Status);
        await ValidateOrderExistsAsync(dto.CustomerOrderId);

        var log = new OrderStatusLog
        {
            CustomerOrderId = dto.CustomerOrderId,
            Status = dto.Status,
            PaymentStatus = dto.PaymentStatus,
            EventType = dto.EventType,
            Comment = dto.Comment,
            LogMessage = dto.LogMessage,
            CreatedByUserId = dto.CreatedByUserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.OrderStatusLogs.Add(log);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Order status log created. Output: OrderStatusLogsId={OrderStatusLogsId}", log.OrderStatusLogsId);
        return MapToDto(log);
    }

    public async Task<OrderStatusLogResponseDto> UpdateAsync(string id, UpdateOrderStatusLogDto dto)
    {
        _logger.LogInformation("Updating order status log. Input: OrderStatusLogsId={OrderStatusLogsId}, Status={Status}", id, dto.Status);
        var log = await _context.OrderStatusLogs.FindAsync(id);
        if (log is null)
            throw new NotFoundException("Order status log not found.");

        log.Status = dto.Status;
        log.PaymentStatus = dto.PaymentStatus;
        log.EventType = dto.EventType;
        log.Comment = dto.Comment;
        log.LogMessage = dto.LogMessage;
        log.CreatedByUserId = dto.CreatedByUserId;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Order status log updated. Input: OrderStatusLogsId={OrderStatusLogsId}", id);
        return MapToDto(log);
    }

    public async Task DeleteAsync(string id)
    {
        _logger.LogInformation("Deleting order status log. Input: OrderStatusLogsId={OrderStatusLogsId}", id);
        var log = await _context.OrderStatusLogs.FindAsync(id);
        if (log is null)
            throw new NotFoundException("Order status log not found.");

        _context.OrderStatusLogs.Remove(log);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Delete order status log result. Input: OrderStatusLogsId={OrderStatusLogsId} => Output: Deleted=true", id);
    }

    private async Task ValidateOrderExistsAsync(string customerOrderId)
    {
        var exists = await _context.Orders.AnyAsync(o => o.CustomerOrderId == customerOrderId);
        if (!exists)
            throw new NotFoundException("Order not found.");
    }

    private static OrderStatusLogResponseDto MapToDto(OrderStatusLog log)
    {
        return new OrderStatusLogResponseDto
        {
            OrderStatusLogsId = log.OrderStatusLogsId,
            CustomerOrderId = log.CustomerOrderId,
            Status = log.Status,
            PaymentStatus = log.PaymentStatus,
            EventType = log.EventType,
            Comment = log.Comment,
            LogMessage = log.LogMessage,
            CreatedByUserId = log.CreatedByUserId,
            CreatedAt = log.CreatedAt,
            UpdatedAt = log.UpdatedAt
        };
    }
}
