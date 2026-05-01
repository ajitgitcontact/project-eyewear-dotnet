using backend.Application.Abstractions.Orders;
using backend.DTOs.OrderCreationDtos;

namespace backend.Infrastructure.Services.Orders;

public class DiscountService : IDiscountService
{
    private readonly ILogger<DiscountService> _logger;

    public DiscountService(ILogger<DiscountService> logger)
    {
        _logger = logger;
    }

    public Task<DiscountCalculationResultDto> ApplyDiscountAsync(DiscountCalculationContext context)
    {
        _logger.LogInformation(
            "Applying discounts. UserId={UserId}, Subtotal={Subtotal}, CouponProvided={CouponProvided}, ItemCount={ItemCount}",
            context.UserId,
            context.Subtotal,
            !string.IsNullOrWhiteSpace(context.CouponCode),
            context.Items.Count);

        // TODO: Replace this placeholder when Coupon/DiscountRule/ProductDiscount
        // tables exist. This service is the single boundary for coupon, product
        // discount, order-value offer, tax, delivery charge, and related pricing rules.
        var result = new DiscountCalculationResultDto
        {
            Subtotal = context.Subtotal,
            DiscountAmount = 0,
            FinalAmount = context.Subtotal,
            CouponCode = context.CouponCode
        };

        _logger.LogInformation(
            "Discount application completed. Subtotal={Subtotal}, DiscountAmount={DiscountAmount}, FinalAmount={FinalAmount}",
            result.Subtotal,
            result.DiscountAmount,
            result.FinalAmount);

        return Task.FromResult(result);
    }
}
