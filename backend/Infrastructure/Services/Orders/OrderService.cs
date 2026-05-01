using backend.Application.Abstractions.Orders;
using backend.Application.Exceptions;
using backend.Data;
using backend.DTOs.OrderDtos;
using backend.Models.Orders;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Services.Orders;

public class OrderService : IOrderService
{
    private readonly AppDbContext _context;
    private readonly ILogger<OrderService> _logger;

    public OrderService(AppDbContext context, ILogger<OrderService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<OrderResponseDto>> GetAllAsync()
    {
        _logger.LogInformation("Fetching all orders.");
        var orders = await _context.Orders
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => MapToDto(o))
            .ToListAsync();
        _logger.LogInformation("Fetched all orders. Output: Count={Count}", orders.Count);
        return orders;
    }

    public async Task<OrderResponseDto> GetByIdAsync(string id)
    {
        _logger.LogInformation("Fetching order by id. Input: OrdersId={OrdersId}", id);
        var order = await _context.Orders.FindAsync(id);
        if (order is null)
        {
            _logger.LogInformation("Get order by id result. Input: OrdersId={OrdersId} => Output: Found=false", id);
            throw new NotFoundException("Order not found.");
        }

        _logger.LogInformation("Get order by id result. Input: OrdersId={OrdersId} => Output: Found=true, CustomerOrderId={CustomerOrderId}", id, order.CustomerOrderId);
        return MapToDto(order);
    }

    public async Task<OrderResponseDto> GetByCustomerOrderIdAsync(string customerOrderId)
    {
        _logger.LogInformation("Fetching order by customer order id. Input: CustomerOrderId={CustomerOrderId}", customerOrderId);
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.CustomerOrderId == customerOrderId);
        if (order is null)
        {
            _logger.LogInformation("Get order by customer order id result. Input: CustomerOrderId={CustomerOrderId} => Output: Found=false", customerOrderId);
            throw new NotFoundException("Order not found.");
        }

        _logger.LogInformation("Get order by customer order id result. Input: CustomerOrderId={CustomerOrderId} => Output: Found=true, OrdersId={OrdersId}", customerOrderId, order.OrdersId);
        return MapToDto(order);
    }

    public async Task<OrderResponseDto> CreateAsync(CreateOrderDto dto)
    {
        _logger.LogInformation("Creating order. Input: CustomerOrderId={CustomerOrderId}, UserId={UserId}, TotalAmount={TotalAmount}", dto.CustomerOrderId, dto.UserId, dto.TotalAmount);

        var userExists = await _context.Users.AnyAsync(u => u.Id == dto.UserId);
        if (!userExists)
        {
            _logger.LogError("Create order blocked. User not found. UserId={UserId}", dto.UserId);
            throw new NotFoundException("User not found.");
        }

        var orderExists = await _context.Orders.AnyAsync(o => o.CustomerOrderId == dto.CustomerOrderId);
        if (orderExists)
        {
            _logger.LogError("Create order blocked. CustomerOrderId already exists. CustomerOrderId={CustomerOrderId}", dto.CustomerOrderId);
            throw new ConflictException("An order with this customer order id already exists.");
        }

        var order = new Order
        {
            CustomerOrderId = dto.CustomerOrderId,
            UserId = dto.UserId,
            CustomerName = dto.CustomerName,
            CustomerEmail = dto.CustomerEmail,
            CustomerPhone = dto.CustomerPhone,
            TotalAmount = dto.TotalAmount,
            OriginalSubtotal = dto.OriginalSubtotal,
            ProductDiscountTotal = dto.ProductDiscountTotal,
            CouponCode = dto.CouponCode,
            CouponDiscountAmount = dto.CouponDiscountAmount,
            FinalAmount = dto.FinalAmount,
            PaymentStatus = dto.PaymentStatus,
            OrderStatus = dto.OrderStatus,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Order created. Input: CustomerOrderId={CustomerOrderId} => Output: OrdersId={OrdersId}", order.CustomerOrderId, order.OrdersId);
        return MapToDto(order);
    }

    public async Task<OrderResponseDto> UpdateAsync(string id, UpdateOrderDto dto)
    {
        _logger.LogInformation("Updating order. Input: OrdersId={OrdersId}, TotalAmount={TotalAmount}, OrderStatus={OrderStatus}", id, dto.TotalAmount, dto.OrderStatus);
        var order = await _context.Orders.FindAsync(id);
        if (order is null)
        {
            _logger.LogInformation("Update order result. Input: OrdersId={OrdersId} => Output: Found=false", id);
            throw new NotFoundException("Order not found.");
        }

        order.CustomerName = dto.CustomerName;
        order.CustomerEmail = dto.CustomerEmail;
        order.CustomerPhone = dto.CustomerPhone;
        order.TotalAmount = dto.TotalAmount;
        order.OriginalSubtotal = dto.OriginalSubtotal;
        order.ProductDiscountTotal = dto.ProductDiscountTotal;
        order.CouponCode = dto.CouponCode;
        order.CouponDiscountAmount = dto.CouponDiscountAmount;
        order.FinalAmount = dto.FinalAmount;
        order.PaymentStatus = dto.PaymentStatus;
        order.OrderStatus = dto.OrderStatus;
        order.Notes = dto.Notes;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Order updated. Input: OrdersId={OrdersId} => Output: CustomerOrderId={CustomerOrderId}", id, order.CustomerOrderId);
        return MapToDto(order);
    }

    public async Task DeleteAsync(string id)
    {
        _logger.LogInformation("Deleting order. Input: OrdersId={OrdersId}", id);
        var order = await _context.Orders.FindAsync(id);
        if (order is null)
        {
            _logger.LogInformation("Delete order result. Input: OrdersId={OrdersId} => Output: Found=false, Deleted=false", id);
            throw new NotFoundException("Order not found.");
        }

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Delete order result. Input: OrdersId={OrdersId}, CustomerOrderId={CustomerOrderId} => Output: Deleted=true", id, order.CustomerOrderId);
    }

    private static OrderResponseDto MapToDto(Order order)
    {
        return new OrderResponseDto
        {
            OrdersId = order.OrdersId,
            CustomerOrderId = order.CustomerOrderId,
            UserId = order.UserId,
            CustomerName = order.CustomerName,
            CustomerEmail = order.CustomerEmail,
            CustomerPhone = order.CustomerPhone,
            TotalAmount = order.TotalAmount,
            OriginalSubtotal = order.OriginalSubtotal,
            ProductDiscountTotal = order.ProductDiscountTotal,
            CouponCode = order.CouponCode,
            CouponDiscountAmount = order.CouponDiscountAmount,
            FinalAmount = order.FinalAmount,
            PaymentStatus = order.PaymentStatus,
            OrderStatus = order.OrderStatus,
            Notes = order.Notes,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt
        };
    }
}
