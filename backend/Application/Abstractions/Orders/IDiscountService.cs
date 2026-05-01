using backend.DTOs.OrderCreationDtos;

namespace backend.Application.Abstractions.Orders;

public interface IDiscountService
{
    Task<DiscountCalculationResultDto> ApplyDiscountAsync(DiscountCalculationContext context);
}

public class DiscountCalculationContext
{
    public int UserId { get; set; }
    public decimal Subtotal { get; set; }
    public string? CouponCode { get; set; }
    public List<DiscountCalculationItem> Items { get; set; } = new();
}

public class DiscountCalculationItem
{
    public int ProductId { get; set; }
    public string SKU { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}
