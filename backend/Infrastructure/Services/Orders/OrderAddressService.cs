using backend.Application.Abstractions.Orders;
using backend.Application.Exceptions;
using backend.Data;
using backend.DTOs.OrderAddressDtos;
using backend.Models.Orders;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Services.Orders;

public class OrderAddressService : IOrderAddressService
{
    private readonly AppDbContext _context;
    private readonly ILogger<OrderAddressService> _logger;

    public OrderAddressService(AppDbContext context, ILogger<OrderAddressService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<OrderAddressResponseDto>> GetByCustomerOrderIdAsync(string customerOrderId)
    {
        _logger.LogInformation("Fetching order addresses. Input: CustomerOrderId={CustomerOrderId}", customerOrderId);
        var addresses = await _context.OrderAddresses
            .Where(oa => oa.CustomerOrderId == customerOrderId)
            .Select(oa => MapToDto(oa))
            .ToListAsync();
        _logger.LogInformation("Fetched order addresses. Input: CustomerOrderId={CustomerOrderId} => Output: Count={Count}", customerOrderId, addresses.Count);
        return addresses;
    }

    public async Task<OrderAddressResponseDto> GetByIdAsync(string id)
    {
        _logger.LogInformation("Fetching order address by id. Input: OrderAddressesId={OrderAddressesId}", id);
        var address = await _context.OrderAddresses.FindAsync(id);
        if (address is null)
            throw new NotFoundException("Order address not found.");

        return MapToDto(address);
    }

    public async Task<OrderAddressResponseDto> CreateAsync(CreateOrderAddressDto dto)
    {
        _logger.LogInformation("Creating order address. Input: CustomerOrderId={CustomerOrderId}, Type={Type}", dto.CustomerOrderId, dto.Type);
        await ValidateOrderExistsAsync(dto.CustomerOrderId);

        var address = new OrderAddress
        {
            CustomerOrderId = dto.CustomerOrderId,
            Type = dto.Type,
            Line1 = dto.Line1,
            Line2 = dto.Line2,
            City = dto.City,
            State = dto.State,
            Pincode = dto.Pincode,
            Country = dto.Country,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.OrderAddresses.Add(address);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Order address created. Output: OrderAddressesId={OrderAddressesId}", address.OrderAddressesId);
        return MapToDto(address);
    }

    public async Task<OrderAddressResponseDto> UpdateAsync(string id, UpdateOrderAddressDto dto)
    {
        _logger.LogInformation("Updating order address. Input: OrderAddressesId={OrderAddressesId}, Type={Type}", id, dto.Type);
        var address = await _context.OrderAddresses.FindAsync(id);
        if (address is null)
            throw new NotFoundException("Order address not found.");

        address.Type = dto.Type;
        address.Line1 = dto.Line1;
        address.Line2 = dto.Line2;
        address.City = dto.City;
        address.State = dto.State;
        address.Pincode = dto.Pincode;
        address.Country = dto.Country;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Order address updated. Input: OrderAddressesId={OrderAddressesId}", id);
        return MapToDto(address);
    }

    public async Task DeleteAsync(string id)
    {
        _logger.LogInformation("Deleting order address. Input: OrderAddressesId={OrderAddressesId}", id);
        var address = await _context.OrderAddresses.FindAsync(id);
        if (address is null)
            throw new NotFoundException("Order address not found.");

        _context.OrderAddresses.Remove(address);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Delete order address result. Input: OrderAddressesId={OrderAddressesId} => Output: Deleted=true", id);
    }

    private async Task ValidateOrderExistsAsync(string customerOrderId)
    {
        var exists = await _context.Orders.AnyAsync(o => o.CustomerOrderId == customerOrderId);
        if (!exists)
            throw new NotFoundException("Order not found.");
    }

    private static OrderAddressResponseDto MapToDto(OrderAddress address)
    {
        return new OrderAddressResponseDto
        {
            OrderAddressesId = address.OrderAddressesId,
            CustomerOrderId = address.CustomerOrderId,
            Type = address.Type,
            Line1 = address.Line1,
            Line2 = address.Line2,
            City = address.City,
            State = address.State,
            Pincode = address.Pincode,
            Country = address.Country,
            CreatedAt = address.CreatedAt,
            UpdatedAt = address.UpdatedAt
        };
    }
}
