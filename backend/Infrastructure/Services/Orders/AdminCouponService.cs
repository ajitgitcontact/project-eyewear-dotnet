using backend.Application.Abstractions.Orders;
using backend.Application.Exceptions;
using backend.Data;
using backend.DTOs.CouponDtos;
using backend.Models.Orders;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Services.Orders;

public class AdminCouponService : IAdminCouponService
{
    private readonly AppDbContext _context;
    private readonly ILogger<AdminCouponService> _logger;

    public AdminCouponService(AppDbContext context, ILogger<AdminCouponService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<CouponResponseDto>> GetAllAsync()
    {
        return await _context.Coupons
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => MapToDto(c))
            .ToListAsync();
    }

    public async Task<CouponResponseDto> GetByIdAsync(string id)
    {
        var coupon = await GetEntityAsync(id);
        return MapToDto(coupon);
    }

    public async Task<CouponResponseDto> CreateAsync(CreateCouponDto dto)
    {
        ValidateDto(dto);
        var code = dto.CouponCode.Trim().ToUpperInvariant();

        if (await _context.Coupons.AnyAsync(c => c.CouponCode == code))
            throw new ConflictException("Coupon code already exists.");

        var coupon = new Coupon
        {
            CouponCode = code,
            CouponName = dto.CouponName.Trim(),
            CouponType = dto.CouponType,
            CouponValue = dto.CouponValue,
            MinimumOrderAmount = dto.MinimumOrderAmount,
            MaximumCouponAmount = dto.MaximumCouponAmount,
            UsageLimit = dto.UsageLimit,
            PerUserUsageLimit = dto.PerUserUsageLimit,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Coupons.Add(coupon);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Coupon created. CouponId={CouponId}, CouponCode={CouponCode}", coupon.CouponId, coupon.CouponCode);
        return MapToDto(coupon);
    }

    public async Task<CouponResponseDto> UpdateAsync(string id, UpdateCouponDto dto)
    {
        ValidateDto(dto);
        var coupon = await GetEntityAsync(id);
        var code = dto.CouponCode.Trim().ToUpperInvariant();

        if (await _context.Coupons.AnyAsync(c => c.CouponCode == code && c.CouponId != id))
            throw new ConflictException("Coupon code already exists.");

        coupon.CouponCode = code;
        coupon.CouponName = dto.CouponName.Trim();
        coupon.CouponType = dto.CouponType;
        coupon.CouponValue = dto.CouponValue;
        coupon.MinimumOrderAmount = dto.MinimumOrderAmount;
        coupon.MaximumCouponAmount = dto.MaximumCouponAmount;
        coupon.UsageLimit = dto.UsageLimit;
        coupon.PerUserUsageLimit = dto.PerUserUsageLimit;
        coupon.StartDate = dto.StartDate;
        coupon.EndDate = dto.EndDate;
        coupon.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Coupon updated. CouponId={CouponId}, CouponCode={CouponCode}", coupon.CouponId, coupon.CouponCode);
        return MapToDto(coupon);
    }

    public async Task DeleteAsync(string id)
    {
        var coupon = await GetEntityAsync(id);
        _context.Coupons.Remove(coupon);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Coupon deleted. CouponId={CouponId}", id);
    }

    private async Task<Coupon> GetEntityAsync(string id)
    {
        var coupon = await _context.Coupons.FindAsync(id);
        return coupon ?? throw new NotFoundException("Coupon not found.");
    }

    private static void ValidateDto(CreateCouponDto dto)
    {
        dto.CouponCode = dto.CouponCode.Trim();
        dto.CouponName = dto.CouponName.Trim();

        if (string.IsNullOrWhiteSpace(dto.CouponCode))
            throw new BadRequestException("CouponCode is required.");

        if (string.IsNullOrWhiteSpace(dto.CouponName))
            throw new BadRequestException("CouponName is required.");

        if (dto.CouponValue <= 0)
            throw new BadRequestException("CouponValue must be greater than 0.");

        if (dto.CouponType == DiscountValueType.PERCENTAGE && dto.CouponValue > 100)
            throw new BadRequestException("Percentage coupon cannot exceed 100.");

        if (dto.StartDate.HasValue && dto.EndDate.HasValue && dto.StartDate > dto.EndDate)
            throw new BadRequestException("StartDate cannot be greater than EndDate.");
    }

    private static CouponResponseDto MapToDto(Coupon coupon)
    {
        return new CouponResponseDto
        {
            CouponId = coupon.CouponId,
            CouponCode = coupon.CouponCode,
            CouponName = coupon.CouponName,
            CouponType = coupon.CouponType,
            CouponValue = coupon.CouponValue,
            MinimumOrderAmount = coupon.MinimumOrderAmount,
            MaximumCouponAmount = coupon.MaximumCouponAmount,
            UsageLimit = coupon.UsageLimit,
            PerUserUsageLimit = coupon.PerUserUsageLimit,
            StartDate = coupon.StartDate,
            EndDate = coupon.EndDate,
            IsActive = coupon.IsActive,
            CreatedAt = coupon.CreatedAt,
            UpdatedAt = coupon.UpdatedAt
        };
    }
}
