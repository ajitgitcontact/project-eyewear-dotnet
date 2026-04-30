using backend.Application.Abstractions.Orders;
using backend.Application.Exceptions;
using backend.Data;
using backend.DTOs.OrderCreationDtos;
using backend.Models.Products;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Services.Orders;

public class OrderCreationValidatorService : IOrderCreationValidatorService
{
    private readonly AppDbContext _context;
    private readonly ILogger<OrderCreationValidatorService> _logger;

    public OrderCreationValidatorService(AppDbContext context, ILogger<OrderCreationValidatorService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task ValidateAsync(int userId, OrderCreationRequestDto dto)
    {
        _logger.LogInformation("Order creation validation started. UserId={UserId}, ItemCount={ItemCount}", userId, dto.Items?.Count ?? 0);

        if (dto.Customer is null)
        {
            _logger.LogWarning("Order creation validation failed. UserId={UserId}, Reason=MissingCustomer", userId);
            throw new BadRequestException("Customer details are required.");
        }

        if (dto.Address is null)
        {
            _logger.LogWarning("Order creation validation failed. UserId={UserId}, Reason=MissingAddress", userId);
            throw new BadRequestException("Address is required.");
        }

        if (dto.Payment is null)
        {
            _logger.LogWarning("Order creation validation failed. UserId={UserId}, Reason=MissingPayment", userId);
            throw new BadRequestException("Payment details are required.");
        }

        var userExists = await _context.Users.AnyAsync(u => u.Id == userId && u.IsActive);
        if (!userExists)
        {
            _logger.LogWarning("Order creation validation failed. User not found or inactive. UserId={UserId}", userId);
            throw new NotFoundException("User not found.");
        }

        if (dto.Items is null || dto.Items.Count == 0)
        {
            _logger.LogWarning("Order creation validation failed. UserId={UserId}, Reason=NoItems", userId);
            throw new BadRequestException("At least one order item is required.");
        }

        var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _context.Products
            .Include(p => p.CustomizationOptions)
                .ThenInclude(o => o.CustomizationValues)
            .Where(p => productIds.Contains(p.ProductId))
            .ToDictionaryAsync(p => p.ProductId);

        foreach (var item in dto.Items)
        {
            if (!products.TryGetValue(item.ProductId, out var product))
            {
                _logger.LogWarning("Order creation validation failed. Product not found. UserId={UserId}, ProductId={ProductId}", userId, item.ProductId);
                throw new NotFoundException("Product not found.");
            }

            if (!product.IsActive)
            {
                _logger.LogWarning("Order creation validation failed. Product inactive. UserId={UserId}, ProductId={ProductId}", userId, item.ProductId);
                throw new BadRequestException("Product is not available.");
            }

            if (product.AvailableQuantity < item.Quantity)
            {
                _logger.LogWarning(
                    "Order creation validation failed. Insufficient stock. UserId={UserId}, ProductId={ProductId}, Requested={Requested}, Available={Available}",
                    userId,
                    item.ProductId,
                    item.Quantity,
                    product.AvailableQuantity);
                throw new BadRequestException("Insufficient product stock.");
            }

            ValidateCustomizations(userId, item, product);
        }

        var supportsPrescription = products.Values.Any(p => p.HasPrescription);
        if (!supportsPrescription && dto.Prescription is not null)
        {
            _logger.LogWarning("Order creation validation failed. Prescription provided for non-prescription products. UserId={UserId}", userId);
            throw new BadRequestException("Prescription is not supported for the selected products.");
        }

        _logger.LogInformation("Order creation validation completed. UserId={UserId}", userId);
    }

    private void ValidateCustomizations(int userId, OrderCreationItemDto item, Product product)
    {
        if (item.Customizations is null)
        {
            _logger.LogWarning("Order creation validation failed. Customizations cannot be null. UserId={UserId}, ProductId={ProductId}", userId, item.ProductId);
            throw new BadRequestException("Customizations cannot be null.");
        }

        var selectedOptionIds = item.Customizations
            .Where(c => c.CustomizationOptionId.HasValue)
            .Select(c => c.CustomizationOptionId!.Value)
            .ToHashSet();

        foreach (var requiredOption in product.CustomizationOptions.Where(o => o.IsRequired))
        {
            if (!selectedOptionIds.Contains(requiredOption.CustomizationOptionId))
            {
                _logger.LogWarning(
                    "Order creation validation failed. Required customization missing. UserId={UserId}, ProductId={ProductId}, OptionId={OptionId}",
                    userId,
                    product.ProductId,
                    requiredOption.CustomizationOptionId);
                throw new BadRequestException("Required product customization is missing.");
            }
        }

        foreach (var customization in item.Customizations)
        {
            if (!customization.CustomizationOptionId.HasValue && !customization.CustomizationValueId.HasValue)
            {
                _logger.LogWarning("Order creation validation failed. Customization reference missing. UserId={UserId}, ProductId={ProductId}", userId, item.ProductId);
                throw new BadRequestException("Customization option or value is required.");
            }

            var option = customization.CustomizationOptionId.HasValue
                ? product.CustomizationOptions.FirstOrDefault(o => o.CustomizationOptionId == customization.CustomizationOptionId.Value)
                : null;

            if (customization.CustomizationOptionId.HasValue && option is null)
            {
                _logger.LogWarning(
                    "Order creation validation failed. Customization option invalid. UserId={UserId}, ProductId={ProductId}, OptionId={OptionId}",
                    userId,
                    item.ProductId,
                    customization.CustomizationOptionId);
                throw new BadRequestException("Customization option does not belong to the selected product.");
            }

            if (customization.CustomizationValueId.HasValue)
            {
                var value = product.CustomizationOptions
                    .SelectMany(o => o.CustomizationValues.Select(v => new { Option = o, Value = v }))
                    .FirstOrDefault(x => x.Value.CustomizationValueId == customization.CustomizationValueId.Value);

                if (value is null)
                {
                    _logger.LogWarning(
                        "Order creation validation failed. Customization value invalid. UserId={UserId}, ProductId={ProductId}, ValueId={ValueId}",
                        userId,
                        item.ProductId,
                        customization.CustomizationValueId);
                    throw new BadRequestException("Customization value does not belong to the selected product.");
                }

                if (customization.CustomizationOptionId.HasValue && value.Option.CustomizationOptionId != customization.CustomizationOptionId.Value)
                {
                    _logger.LogWarning(
                        "Order creation validation failed. Customization value option mismatch. UserId={UserId}, ProductId={ProductId}, OptionId={OptionId}, ValueId={ValueId}",
                        userId,
                        item.ProductId,
                        customization.CustomizationOptionId,
                        customization.CustomizationValueId);
                    throw new BadRequestException("Customization value does not belong to the provided customization option.");
                }
            }
        }
    }
}
