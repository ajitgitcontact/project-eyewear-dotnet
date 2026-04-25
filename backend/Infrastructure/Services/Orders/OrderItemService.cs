using backend.Application.Abstractions.Orders;
using backend.Application.Exceptions;
using backend.Data;
using backend.DTOs.OrderItemDtos;
using backend.Models.Orders;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Services.Orders;

public class OrderItemService : IOrderItemService
{
    private readonly AppDbContext _context;
    private readonly ILogger<OrderItemService> _logger;

    public OrderItemService(AppDbContext context, ILogger<OrderItemService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<OrderItemResponseDto>> GetByCustomerOrderIdAsync(string customerOrderId)
    {
        _logger.LogInformation("Fetching order items. Input: CustomerOrderId={CustomerOrderId}", customerOrderId);
        var items = await _context.OrderItems
            .Where(oi => oi.CustomerOrderId == customerOrderId)
            .Select(oi => MapToDto(oi))
            .ToListAsync();
        _logger.LogInformation("Fetched order items. Input: CustomerOrderId={CustomerOrderId} => Output: Count={Count}", customerOrderId, items.Count);
        return items;
    }

    public async Task<OrderItemResponseDto> GetByIdAsync(string id)
    {
        _logger.LogInformation("Fetching order item by id. Input: OrderItemsId={OrderItemsId}", id);
        var item = await _context.OrderItems.FindAsync(id);
        if (item is null)
        {
            _logger.LogInformation("Get order item by id result. Input: OrderItemsId={OrderItemsId} => Output: Found=false", id);
            throw new NotFoundException("Order item not found.");
        }

        _logger.LogInformation("Get order item by id result. Input: OrderItemsId={OrderItemsId} => Output: Found=true, SKU={Sku}", id, item.SKU);
        return MapToDto(item);
    }

    public async Task<OrderItemResponseDto> CreateAsync(CreateOrderItemDto dto)
    {
        _logger.LogInformation("Creating order item. Input: CustomerOrderId={CustomerOrderId}, ProductId={ProductId}, SKU={Sku}, Quantity={Quantity}", dto.CustomerOrderId, dto.ProductId, dto.SKU, dto.Quantity);

        await ValidateOrderExistsAsync(dto.CustomerOrderId);
        await ValidateProductExistsAsync(dto.ProductId);
        await EnsureSkuAvailableAsync(dto.SKU);

        var item = new OrderItem
        {
            CustomerOrderId = dto.CustomerOrderId,
            ProductId = dto.ProductId,
            SKU = dto.SKU,
            ProductName = dto.ProductName,
            Quantity = dto.Quantity,
            Price = dto.Price,
            TotalPrice = dto.Quantity * dto.Price,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.OrderItems.Add(item);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Order item created. Input: SKU={Sku} => Output: OrderItemsId={OrderItemsId}", item.SKU, item.OrderItemsId);
        return MapToDto(item);
    }

    public async Task<OrderItemResponseDto> UpdateAsync(string id, UpdateOrderItemDto dto)
    {
        _logger.LogInformation("Updating order item. Input: OrderItemsId={OrderItemsId}, ProductId={ProductId}, SKU={Sku}, Quantity={Quantity}", id, dto.ProductId, dto.SKU, dto.Quantity);
        var item = await _context.OrderItems.FindAsync(id);
        if (item is null)
        {
            _logger.LogInformation("Update order item result. Input: OrderItemsId={OrderItemsId} => Output: Found=false", id);
            throw new NotFoundException("Order item not found.");
        }

        await ValidateProductExistsAsync(dto.ProductId);
        if (!string.Equals(item.SKU, dto.SKU, StringComparison.Ordinal))
            await EnsureSkuAvailableAsync(dto.SKU);

        item.ProductId = dto.ProductId;
        item.SKU = dto.SKU;
        item.ProductName = dto.ProductName;
        item.Quantity = dto.Quantity;
        item.Price = dto.Price;
        item.TotalPrice = dto.Quantity * dto.Price;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Order item updated. Input: OrderItemsId={OrderItemsId} => Output: SKU={Sku}", id, item.SKU);
        return MapToDto(item);
    }

    public async Task DeleteAsync(string id)
    {
        _logger.LogInformation("Deleting order item. Input: OrderItemsId={OrderItemsId}", id);
        var item = await _context.OrderItems.FindAsync(id);
        if (item is null)
        {
            _logger.LogInformation("Delete order item result. Input: OrderItemsId={OrderItemsId} => Output: Found=false, Deleted=false", id);
            throw new NotFoundException("Order item not found.");
        }

        _context.OrderItems.Remove(item);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Delete order item result. Input: OrderItemsId={OrderItemsId}, SKU={Sku} => Output: Deleted=true", id, item.SKU);
    }

    private async Task ValidateOrderExistsAsync(string customerOrderId)
    {
        var exists = await _context.Orders.AnyAsync(o => o.CustomerOrderId == customerOrderId);
        if (!exists)
            throw new NotFoundException("Order not found.");
    }

    private async Task ValidateProductExistsAsync(int productId)
    {
        var exists = await _context.Products.AnyAsync(p => p.ProductId == productId);
        if (!exists)
            throw new NotFoundException("Product not found.");
    }

    private async Task EnsureSkuAvailableAsync(string sku)
    {
        var exists = await _context.OrderItems.AnyAsync(oi => oi.SKU == sku);
        if (exists)
            throw new ConflictException("An order item with this SKU already exists.");
    }

    private static OrderItemResponseDto MapToDto(OrderItem item)
    {
        return new OrderItemResponseDto
        {
            OrderItemsId = item.OrderItemsId,
            CustomerOrderId = item.CustomerOrderId,
            ProductId = item.ProductId,
            SKU = item.SKU,
            ProductName = item.ProductName,
            Quantity = item.Quantity,
            Price = item.Price,
            TotalPrice = item.TotalPrice,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt
        };
    }
}
