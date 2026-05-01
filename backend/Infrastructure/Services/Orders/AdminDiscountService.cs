using backend.Application.Abstractions.Orders;
using backend.Application.Exceptions;
using backend.Data;
using backend.DTOs.DiscountDtos;
using backend.Models.Orders;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Services.Orders;

public class AdminDiscountService : IAdminDiscountService
{
    private readonly AppDbContext _context;
    private readonly ILogger<AdminDiscountService> _logger;

    public AdminDiscountService(AppDbContext context, ILogger<AdminDiscountService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<DiscountResponseDto>> GetAllAsync()
    {
        var discounts = await _context.Discounts
            .Include(d => d.DiscountProducts)
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => MapToDto(d))
            .ToListAsync();
        return discounts;
    }

    public async Task<DiscountResponseDto> GetByIdAsync(string id)
    {
        var discount = await GetEntityAsync(id);
        return MapToDto(discount);
    }

    public async Task<DiscountResponseDto> CreateAsync(CreateDiscountDto dto)
    {
        ValidateDto(dto);
        await ValidateProductsAsync(dto);

        var discount = new Discount
        {
            DiscountName = dto.DiscountName.Trim(),
            DiscountType = dto.DiscountType,
            DiscountValue = dto.DiscountValue,
            AppliesTo = dto.AppliesTo,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        foreach (var productId in dto.ProductIds.Distinct())
            discount.DiscountProducts.Add(new DiscountProduct { ProductId = productId });

        _context.Discounts.Add(discount);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Admin discount created. DiscountId={DiscountId}, AppliesTo={AppliesTo}", discount.DiscountId, discount.AppliesTo);
        return MapToDto(discount);
    }

    public async Task<DiscountResponseDto> UpdateAsync(string id, UpdateDiscountDto dto)
    {
        ValidateDto(dto);
        await ValidateProductsAsync(dto);

        var discount = await GetEntityAsync(id);
        discount.DiscountName = dto.DiscountName.Trim();
        discount.DiscountType = dto.DiscountType;
        discount.DiscountValue = dto.DiscountValue;
        discount.AppliesTo = dto.AppliesTo;
        discount.StartDate = dto.StartDate;
        discount.EndDate = dto.EndDate;
        discount.IsActive = dto.IsActive;

        _context.DiscountProducts.RemoveRange(discount.DiscountProducts);
        foreach (var productId in dto.ProductIds.Distinct())
            discount.DiscountProducts.Add(new DiscountProduct { DiscountId = discount.DiscountId, ProductId = productId });

        await _context.SaveChangesAsync();
        _logger.LogInformation("Admin discount updated. DiscountId={DiscountId}, AppliesTo={AppliesTo}", discount.DiscountId, discount.AppliesTo);
        return MapToDto(discount);
    }

    public async Task DeleteAsync(string id)
    {
        var discount = await GetEntityAsync(id);
        _context.Discounts.Remove(discount);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Admin discount deleted. DiscountId={DiscountId}", id);
    }

    private async Task<Discount> GetEntityAsync(string id)
    {
        var discount = await _context.Discounts
            .Include(d => d.DiscountProducts)
            .FirstOrDefaultAsync(d => d.DiscountId == id);
        return discount ?? throw new NotFoundException("Discount not found.");
    }

    private async Task ValidateProductsAsync(CreateDiscountDto dto)
    {
        if (dto.AppliesTo == DiscountAppliesTo.PRODUCT && dto.ProductIds.Count == 0)
            throw new BadRequestException("Product discount requires at least one product.");

        if (dto.ProductIds.Count == 0)
            return;

        var distinctIds = dto.ProductIds.Distinct().ToList();
        var foundCount = await _context.Products.CountAsync(p => distinctIds.Contains(p.ProductId));
        if (foundCount != distinctIds.Count)
            throw new BadRequestException("One or more discount products are invalid.");
    }

    private static void ValidateDto(CreateDiscountDto dto)
    {
        dto.DiscountName = dto.DiscountName.Trim();
        if (string.IsNullOrWhiteSpace(dto.DiscountName))
            throw new BadRequestException("DiscountName is required.");

        if (dto.DiscountValue <= 0)
            throw new BadRequestException("DiscountValue must be greater than 0.");

        if (dto.DiscountType == DiscountValueType.PERCENTAGE && dto.DiscountValue > 100)
            throw new BadRequestException("Percentage discount cannot exceed 100.");

        if (dto.StartDate.HasValue && dto.EndDate.HasValue && dto.StartDate > dto.EndDate)
            throw new BadRequestException("StartDate cannot be greater than EndDate.");
    }

    private static DiscountResponseDto MapToDto(Discount discount)
    {
        return new DiscountResponseDto
        {
            DiscountId = discount.DiscountId,
            DiscountName = discount.DiscountName,
            DiscountType = discount.DiscountType,
            DiscountValue = discount.DiscountValue,
            AppliesTo = discount.AppliesTo,
            StartDate = discount.StartDate,
            EndDate = discount.EndDate,
            IsActive = discount.IsActive,
            ProductIds = discount.DiscountProducts.Select(dp => dp.ProductId).ToList(),
            CreatedAt = discount.CreatedAt,
            UpdatedAt = discount.UpdatedAt
        };
    }
}
