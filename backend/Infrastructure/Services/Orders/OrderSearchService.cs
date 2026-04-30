using backend.Application.Abstractions.Orders;
using backend.Application.Exceptions;
using backend.Data;
using backend.DTOs.OrderFetchDtos;
using backend.Models.Orders;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Services.Orders;

public class OrderSearchService : IOrderSearchService
{
    private const int MaxPageSize = 100;

    private readonly AppDbContext _context;
    private readonly ILogger<OrderSearchService> _logger;

    public OrderSearchService(AppDbContext context, ILogger<OrderSearchService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<OrderSearchResponseDto> SearchAsync(OrderSearchRequestDto request)
    {
        NormalizeAndValidate(request);
        _logger.LogInformation(
            "Admin order search started. Filters: FromCreatedDate={FromCreatedDate}, ToCreatedDate={ToCreatedDate}, OrderStatus={OrderStatus}, PaymentStatus={PaymentStatus}, CustomerOrderId={CustomerOrderId}, Email={Email}, ContactNumber={ContactNumber}, UserId={UserId}, PageNumber={PageNumber}, PageSize={PageSize}",
            request.FromCreatedDate,
            request.ToCreatedDate,
            request.OrderStatus,
            request.PaymentStatus,
            request.CustomerOrderId,
            request.Email,
            request.ContactNumber,
            request.UserId,
            request.PageNumber,
            request.PageSize);

        var query = _context.Orders.AsNoTracking().AsQueryable();

        if (request.FromCreatedDate.HasValue)
            query = query.Where(o => o.CreatedAt >= request.FromCreatedDate.Value);

        if (request.ToCreatedDate.HasValue)
            query = query.Where(o => o.CreatedAt <= request.ToCreatedDate.Value);

        if (!string.IsNullOrWhiteSpace(request.OrderStatus))
        {
            var status = ParseOrderStatus(request.OrderStatus);
            query = query.Where(o => o.OrderStatus == status);
        }

        if (!string.IsNullOrWhiteSpace(request.PaymentStatus))
        {
            var status = ParsePaymentStatus(request.PaymentStatus);
            query = query.Where(o => o.PaymentStatus == status);
        }

        if (!string.IsNullOrWhiteSpace(request.CustomerOrderId))
            query = query.Where(o => o.CustomerOrderId == request.CustomerOrderId);

        if (!string.IsNullOrWhiteSpace(request.Email))
            query = query.Where(o => EF.Functions.ILike(o.CustomerEmail, $"%{request.Email}%"));

        if (!string.IsNullOrWhiteSpace(request.ContactNumber))
            query = query.Where(o => o.CustomerPhone != null && EF.Functions.ILike(o.CustomerPhone, $"%{request.ContactNumber}%"));

        if (request.UserId.HasValue)
            query = query.Where(o => o.UserId == request.UserId.Value);

        var totalCount = await query.CountAsync();
        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(o => new OrderSearchItemDto
            {
                OrdersId = o.OrdersId,
                CustomerOrderId = o.CustomerOrderId,
                UserId = o.UserId,
                CustomerName = o.CustomerName,
                CustomerEmail = o.CustomerEmail,
                CustomerPhone = o.CustomerPhone,
                TotalAmount = o.TotalAmount,
                PaymentStatus = o.PaymentStatus,
                OrderStatus = o.OrderStatus,
                CreatedAt = o.CreatedAt,
                UpdatedAt = o.UpdatedAt
            })
            .ToListAsync();

        _logger.LogInformation(
            "Admin order search succeeded. TotalCount={TotalCount}, ReturnedCount={ReturnedCount}, PageNumber={PageNumber}, PageSize={PageSize}",
            totalCount,
            orders.Count,
            request.PageNumber,
            request.PageSize);

        return new OrderSearchResponseDto
        {
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            Orders = orders
        };
    }

    internal static void NormalizeAndValidate(OrderSearchRequestDto request)
    {
        request.OrderStatus = request.OrderStatus?.Trim();
        request.PaymentStatus = request.PaymentStatus?.Trim();
        request.CustomerOrderId = request.CustomerOrderId?.Trim();
        request.Email = request.Email?.Trim();
        request.ContactNumber = request.ContactNumber?.Trim();
        request.FromCreatedDate = NormalizeDateTime(request.FromCreatedDate);
        request.ToCreatedDate = NormalizeDateTime(request.ToCreatedDate, endOfDayWhenDateOnly: true);

        if (request.PageNumber <= 0)
            throw new BadRequestException("PageNumber must be greater than 0.");

        if (request.PageSize <= 0 || request.PageSize > MaxPageSize)
            throw new BadRequestException("PageSize must be between 1 and 100.");

        if (request.FromCreatedDate.HasValue && request.ToCreatedDate.HasValue && request.FromCreatedDate > request.ToCreatedDate)
            throw new BadRequestException("FromCreatedDate cannot be greater than ToCreatedDate.");

        if (!string.IsNullOrWhiteSpace(request.CustomerOrderId) && !System.Text.RegularExpressions.Regex.IsMatch(request.CustomerOrderId, @"^\d{11}$"))
            throw new BadRequestException("CustomerOrderId must match YYMMDDXXXXX format.");
    }

    private static DateTime? NormalizeDateTime(DateTime? value, bool endOfDayWhenDateOnly = false)
    {
        if (!value.HasValue)
            return null;

        var dateTime = value.Value;
        if (endOfDayWhenDateOnly && dateTime.TimeOfDay == TimeSpan.Zero)
            dateTime = dateTime.Date.AddDays(1).AddTicks(-1);

        return dateTime.Kind switch
        {
            DateTimeKind.Utc => dateTime,
            DateTimeKind.Local => dateTime.ToUniversalTime(),
            _ => DateTime.SpecifyKind(dateTime, DateTimeKind.Local).ToUniversalTime()
        };
    }

    internal static OrderStatus ParseOrderStatus(string value)
    {
        var normalized = value.Trim().Replace(" ", "_").Replace("-", "_");
        if (!Enum.TryParse<OrderStatus>(normalized, ignoreCase: true, out var status))
            throw new BadRequestException("Invalid orderStatus.");

        return status;
    }

    internal static PaymentStatus ParsePaymentStatus(string value)
    {
        var normalized = value.Trim().Replace(" ", "_").Replace("-", "_");
        if (!Enum.TryParse<PaymentStatus>(normalized, ignoreCase: true, out var status))
            throw new BadRequestException("Invalid paymentStatus.");

        return status;
    }
}
