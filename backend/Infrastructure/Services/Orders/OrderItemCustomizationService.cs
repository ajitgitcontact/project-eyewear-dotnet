using backend.Application.Abstractions.Orders;
using backend.Application.Exceptions;
using backend.Data;
using backend.DTOs.OrderItemCustomizationDtos;
using backend.Models.Orders;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Services.Orders;

public class OrderItemCustomizationService : IOrderItemCustomizationService
{
    private readonly AppDbContext _context;
    private readonly ILogger<OrderItemCustomizationService> _logger;

    public OrderItemCustomizationService(AppDbContext context, ILogger<OrderItemCustomizationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<OrderItemCustomizationResponseDto>> GetByOrderItemIdAsync(string orderItemId)
    {
        _logger.LogInformation("Fetching order item customizations. Input: OrderItemId={OrderItemId}", orderItemId);
        var customizations = await _context.OrderItemCustomizations
            .Where(oic => oic.OrderItemId == orderItemId)
            .Select(oic => MapToDto(oic))
            .ToListAsync();
        _logger.LogInformation("Fetched order item customizations. Input: OrderItemId={OrderItemId} => Output: Count={Count}", orderItemId, customizations.Count);
        return customizations;
    }

    public async Task<OrderItemCustomizationResponseDto> GetByIdAsync(string id)
    {
        _logger.LogInformation("Fetching order item customization by id. Input: OrderItemCustomizationsId={OrderItemCustomizationsId}", id);
        var customization = await _context.OrderItemCustomizations.FindAsync(id);
        if (customization is null)
            throw new NotFoundException("Order item customization not found.");

        _logger.LogInformation("Get order item customization by id result. Input: OrderItemCustomizationsId={OrderItemCustomizationsId} => Output: Found=true", id);
        return MapToDto(customization);
    }

    public async Task<OrderItemCustomizationResponseDto> CreateAsync(CreateOrderItemCustomizationDto dto)
    {
        _logger.LogInformation("Creating order item customization. Input: OrderItemId={OrderItemId}, Type={Type}, Value={Value}", dto.OrderItemId, dto.Type, dto.Value);
        await ValidateOrderItemExistsAsync(dto.OrderItemId);
        await ValidateCustomizationReferencesAsync(dto.CustomizationOptionId, dto.CustomizationValueId);

        var customization = new OrderItemCustomization
        {
            OrderItemId = dto.OrderItemId,
            CustomizationOptionId = dto.CustomizationOptionId,
            CustomizationValueId = dto.CustomizationValueId,
            Type = dto.Type,
            Value = dto.Value,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.OrderItemCustomizations.Add(customization);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Order item customization created. Output: OrderItemCustomizationsId={OrderItemCustomizationsId}", customization.OrderItemCustomizationsId);
        return MapToDto(customization);
    }

    public async Task<OrderItemCustomizationResponseDto> UpdateAsync(string id, UpdateOrderItemCustomizationDto dto)
    {
        _logger.LogInformation("Updating order item customization. Input: OrderItemCustomizationsId={OrderItemCustomizationsId}, Type={Type}, Value={Value}", id, dto.Type, dto.Value);
        var customization = await _context.OrderItemCustomizations.FindAsync(id);
        if (customization is null)
            throw new NotFoundException("Order item customization not found.");

        await ValidateCustomizationReferencesAsync(dto.CustomizationOptionId, dto.CustomizationValueId);

        customization.CustomizationOptionId = dto.CustomizationOptionId;
        customization.CustomizationValueId = dto.CustomizationValueId;
        customization.Type = dto.Type;
        customization.Value = dto.Value;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Order item customization updated. Input: OrderItemCustomizationsId={OrderItemCustomizationsId}", id);
        return MapToDto(customization);
    }

    public async Task DeleteAsync(string id)
    {
        _logger.LogInformation("Deleting order item customization. Input: OrderItemCustomizationsId={OrderItemCustomizationsId}", id);
        var customization = await _context.OrderItemCustomizations.FindAsync(id);
        if (customization is null)
            throw new NotFoundException("Order item customization not found.");

        _context.OrderItemCustomizations.Remove(customization);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Delete order item customization result. Input: OrderItemCustomizationsId={OrderItemCustomizationsId} => Output: Deleted=true", id);
    }

    private async Task ValidateOrderItemExistsAsync(string orderItemId)
    {
        var exists = await _context.OrderItems.AnyAsync(oi => oi.OrderItemsId == orderItemId);
        if (!exists)
            throw new NotFoundException("Order item not found.");
    }

    private async Task ValidateCustomizationReferencesAsync(int? optionId, int? valueId)
    {
        if (optionId.HasValue)
        {
            var optionExists = await _context.CustomizationOptions.AnyAsync(co => co.CustomizationOptionId == optionId.Value);
            if (!optionExists)
                throw new NotFoundException("Customization option not found.");
        }

        if (valueId.HasValue)
        {
            var value = await _context.CustomizationValues.FirstOrDefaultAsync(cv => cv.CustomizationValueId == valueId.Value);
            if (value is null)
                throw new NotFoundException("Customization value not found.");

            if (optionId.HasValue && value.CustomizationOptionId != optionId.Value)
                throw new BadRequestException("Customization value does not belong to the provided customization option.");
        }
    }

    private static OrderItemCustomizationResponseDto MapToDto(OrderItemCustomization customization)
    {
        return new OrderItemCustomizationResponseDto
        {
            OrderItemCustomizationsId = customization.OrderItemCustomizationsId,
            OrderItemId = customization.OrderItemId,
            CustomizationOptionId = customization.CustomizationOptionId,
            CustomizationValueId = customization.CustomizationValueId,
            Type = customization.Type,
            Value = customization.Value,
            CreatedAt = customization.CreatedAt,
            UpdatedAt = customization.UpdatedAt
        };
    }
}
