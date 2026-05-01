using backend.Models.Orders;

namespace backend.DTOs.OrderDtos;

public class OrderResponseDto
{
    public string OrdersId { get; set; } = string.Empty;
    public string CustomerOrderId { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal OriginalSubtotal { get; set; }
    public decimal ProductDiscountTotal { get; set; }
    public string? CouponCode { get; set; }
    public decimal CouponDiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public OrderStatus OrderStatus { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
