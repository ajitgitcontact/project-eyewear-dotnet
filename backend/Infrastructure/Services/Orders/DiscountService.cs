using backend.Application.Abstractions.Orders;
using backend.Application.Exceptions;
using backend.Data;
using backend.DTOs.OrderCreationDtos;
using backend.Models.Orders;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Services.Orders;

public class DiscountService : IDiscountService
{
    private readonly AppDbContext _context;
    private readonly ILogger<DiscountService> _logger;

    public DiscountService(AppDbContext context, ILogger<DiscountService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<DiscountCalculationResultDto> ApplyDiscountAsync(DiscountCalculationContext context)
    {
        _logger.LogInformation(
            "Discount calculation started. UserId={UserId}, OriginalSubtotal={Subtotal}, CouponProvided={CouponProvided}, ItemCount={ItemCount}",
            context.UserId,
            context.Subtotal,
            !string.IsNullOrWhiteSpace(context.CouponCode),
            context.Items.Count);

        var now = DateTime.UtcNow;
        var activeDiscounts = await _context.Discounts
            .AsNoTracking()
            .Include(d => d.DiscountProducts)
            .Where(d => d.IsActive)
            .Where(d => d.StartDate == null || d.StartDate <= now)
            .Where(d => d.EndDate == null || d.EndDate >= now)
            .ToListAsync();

        var itemResults = new List<DiscountCalculationItemResultDto>();
        decimal productDiscountTotal = 0;
        foreach (var item in context.Items)
        {
            var applicableDiscounts = activeDiscounts
                .Where(d => d.AppliesTo == DiscountAppliesTo.ALL ||
                    d.DiscountProducts.Any(dp => dp.ProductId == item.ProductId))
                .ToList();

            var bestUnitDiscount = applicableDiscounts
                .Select(d => CalculateDiscountAmount(item.UnitPrice, d.DiscountType, d.DiscountValue))
                .DefaultIfEmpty(0)
                .Max();

            bestUnitDiscount = Math.Min(bestUnitDiscount, item.UnitPrice);
            var finalUnitPrice = item.UnitPrice - bestUnitDiscount;

            if (bestUnitDiscount > 0)
            {
                productDiscountTotal += bestUnitDiscount * item.Quantity;
                _logger.LogInformation(
                    "Admin discount applied. UserId={UserId}, ProductId={ProductId}, UnitDiscount={UnitDiscount}",
                    context.UserId,
                    item.ProductId,
                    bestUnitDiscount);
            }

            itemResults.Add(new DiscountCalculationItemResultDto
            {
                LineNumber = item.LineNumber,
                ProductId = item.ProductId,
                OriginalUnitPrice = item.UnitPrice,
                ProductDiscountAmount = bestUnitDiscount,
                FinalUnitPrice = finalUnitPrice,
                FinalLineTotal = finalUnitPrice * item.Quantity
            });
        }

        var subtotalAfterProductDiscounts = context.Subtotal - productDiscountTotal;

        string? couponId = null;
        string? normalizedCouponCode = null;
        decimal couponDiscountAmount = 0;

        if (!string.IsNullOrWhiteSpace(context.CouponCode))
        {
            normalizedCouponCode = context.CouponCode.Trim().ToUpperInvariant();
            _logger.LogInformation("Coupon validation started. UserId={UserId}, CouponCode={CouponCode}", context.UserId, normalizedCouponCode);

            try
            {
                var coupon = await _context.Coupons
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.CouponCode == normalizedCouponCode);

                if (coupon is null)
                    throw new BadRequestException("Invalid coupon code.");

                ValidateCoupon(coupon, subtotalAfterProductDiscounts, now);
                await ValidateCouponUsageLimitsAsync(coupon, context.UserId);

                couponId = coupon.CouponId;
                couponDiscountAmount = CalculateDiscountAmount(subtotalAfterProductDiscounts, coupon.CouponType, coupon.CouponValue);
                if (coupon.CouponType == DiscountValueType.PERCENTAGE && coupon.MaximumCouponAmount.HasValue)
                    couponDiscountAmount = Math.Min(couponDiscountAmount, coupon.MaximumCouponAmount.Value);

                couponDiscountAmount = Math.Min(couponDiscountAmount, subtotalAfterProductDiscounts);

                _logger.LogInformation(
                    "Coupon validation completed. UserId={UserId}, CouponCode={CouponCode}, CouponDiscountAmount={CouponDiscountAmount}",
                    context.UserId,
                    normalizedCouponCode,
                    couponDiscountAmount);
            }
            catch (BadRequestException ex)
            {
                _logger.LogWarning(ex, "Coupon validation failed. UserId={UserId}, CouponCode={CouponCode}", context.UserId, normalizedCouponCode);
                throw;
            }
        }

        var totalDiscount = productDiscountTotal + couponDiscountAmount;
        var finalAmount = Math.Max(0, context.Subtotal - totalDiscount);

        _logger.LogInformation(
            "Discount calculation completed. UserId={UserId}, OriginalSubtotal={OriginalSubtotal}, ProductDiscountTotal={ProductDiscountTotal}, CouponDiscountAmount={CouponDiscountAmount}, FinalAmount={FinalAmount}",
            context.UserId,
            context.Subtotal,
            productDiscountTotal,
            couponDiscountAmount,
            finalAmount);

        return new DiscountCalculationResultDto
        {
            Subtotal = context.Subtotal,
            OriginalSubtotal = context.Subtotal,
            ProductDiscountTotal = productDiscountTotal,
            CouponId = couponId,
            CouponCode = normalizedCouponCode,
            CouponDiscountAmount = couponDiscountAmount,
            DiscountAmount = totalDiscount,
            FinalAmount = finalAmount,
            Items = itemResults
        };
    }

    public async Task RecordCouponUsageAsync(string couponId, int userId, string customerOrderId, string couponCode, decimal couponAmount)
    {
        var usage = new CouponUsage
        {
            CouponId = couponId,
            UserId = userId,
            CustomerOrderId = customerOrderId,
            CouponCode = couponCode,
            CouponAmount = couponAmount,
            UsedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.CouponUsages.Add(usage);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Coupon usage created. UserId={UserId}, CustomerOrderId={CustomerOrderId}, CouponCode={CouponCode}, CouponAmount={CouponAmount}", userId, customerOrderId, couponCode, couponAmount);
    }

    private static decimal CalculateDiscountAmount(decimal amount, DiscountValueType type, decimal value)
    {
        var discount = type == DiscountValueType.PERCENTAGE
            ? amount * value / 100
            : value;

        return Math.Round(Math.Clamp(discount, 0, amount), 2, MidpointRounding.AwayFromZero);
    }

    private static void ValidateCoupon(Coupon coupon, decimal subtotalAfterProductDiscounts, DateTime now)
    {
        if (!coupon.IsActive)
            throw new BadRequestException("Coupon is inactive.");

        if (coupon.StartDate.HasValue && coupon.StartDate.Value > now)
            throw new BadRequestException("Coupon is not active yet.");

        if (coupon.EndDate.HasValue && coupon.EndDate.Value < now)
            throw new BadRequestException("Coupon has expired.");

        if (coupon.MinimumOrderAmount.HasValue && subtotalAfterProductDiscounts < coupon.MinimumOrderAmount.Value)
            throw new BadRequestException("Minimum order amount for this coupon is not satisfied.");
    }

    private async Task ValidateCouponUsageLimitsAsync(Coupon coupon, int userId)
    {
        if (coupon.UsageLimit.HasValue)
        {
            var totalUsage = await _context.CouponUsages.CountAsync(cu => cu.CouponId == coupon.CouponId);
            if (totalUsage >= coupon.UsageLimit.Value)
                throw new BadRequestException("Coupon usage limit has been reached.");
        }

        if (coupon.PerUserUsageLimit.HasValue)
        {
            var userUsage = await _context.CouponUsages.CountAsync(cu => cu.CouponId == coupon.CouponId && cu.UserId == userId);
            if (userUsage >= coupon.PerUserUsageLimit.Value)
                throw new BadRequestException("Coupon usage limit for this customer has been reached.");
        }
    }
}
