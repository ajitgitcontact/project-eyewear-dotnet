using backend.Application.Abstractions.Orders;
using backend.Application.Exceptions;
using backend.Data;
using backend.DTOs.OrderFetchDtos;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Services.Orders;

public class CustomerOrderListService : ICustomerOrderListService
{
    private const int MaxPageSize = 100;

    private readonly AppDbContext _context;
    private readonly ILogger<CustomerOrderListService> _logger;

    public CustomerOrderListService(AppDbContext context, ILogger<CustomerOrderListService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CustomerOrderListResponseDto> GetForCustomerAsync(int userId, CustomerOrderListRequestDto request)
    {
        NormalizeAndValidate(request);
        _logger.LogInformation(
            "Customer order list started. UserId={UserId}, FromCreatedDate={FromCreatedDate}, ToCreatedDate={ToCreatedDate}, OrderStatus={OrderStatus}, PaymentStatus={PaymentStatus}, PageNumber={PageNumber}, PageSize={PageSize}",
            userId,
            request.FromCreatedDate,
            request.ToCreatedDate,
            request.OrderStatus,
            request.PaymentStatus,
            request.PageNumber,
            request.PageSize);

        var query = _context.Orders.AsNoTracking().Where(o => o.UserId == userId);

        if (request.FromCreatedDate.HasValue)
            query = query.Where(o => o.CreatedAt >= request.FromCreatedDate.Value);

        if (request.ToCreatedDate.HasValue)
            query = query.Where(o => o.CreatedAt <= request.ToCreatedDate.Value);

        if (!string.IsNullOrWhiteSpace(request.OrderStatus))
        {
            var status = OrderSearchService.ParseOrderStatus(request.OrderStatus);
            query = query.Where(o => o.OrderStatus == status);
        }

        if (!string.IsNullOrWhiteSpace(request.PaymentStatus))
        {
            var status = OrderSearchService.ParsePaymentStatus(request.PaymentStatus);
            query = query.Where(o => o.PaymentStatus == status);
        }

        var totalCount = await query.CountAsync();
        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(o => new CustomerOrderListItemDto
            {
                CustomerOrderId = o.CustomerOrderId,
                TotalAmount = o.TotalAmount,
                PaymentStatus = o.PaymentStatus,
                OrderStatus = o.OrderStatus,
                CreatedAt = o.CreatedAt,
                UpdatedAt = o.UpdatedAt
            })
            .ToListAsync();

        _logger.LogInformation(
            "Customer order list succeeded. UserId={UserId}, TotalCount={TotalCount}, ReturnedCount={ReturnedCount}, PageNumber={PageNumber}, PageSize={PageSize}",
            userId,
            totalCount,
            orders.Count,
            request.PageNumber,
            request.PageSize);

        return new CustomerOrderListResponseDto
        {
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            Orders = orders
        };
    }

    private static void NormalizeAndValidate(CustomerOrderListRequestDto request)
    {
        request.OrderStatus = request.OrderStatus?.Trim();
        request.PaymentStatus = request.PaymentStatus?.Trim();
        request.FromCreatedDate = NormalizeDateTime(request.FromCreatedDate);
        request.ToCreatedDate = NormalizeDateTime(request.ToCreatedDate, endOfDayWhenDateOnly: true);

        if (request.PageNumber <= 0)
            throw new BadRequestException("PageNumber must be greater than 0.");

        if (request.PageSize <= 0 || request.PageSize > MaxPageSize)
            throw new BadRequestException("PageSize must be between 1 and 100.");

        if (request.FromCreatedDate.HasValue && request.ToCreatedDate.HasValue && request.FromCreatedDate > request.ToCreatedDate)
            throw new BadRequestException("FromCreatedDate cannot be greater than ToCreatedDate.");
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
}
