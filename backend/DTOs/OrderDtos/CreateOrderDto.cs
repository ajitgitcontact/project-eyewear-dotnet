using System.ComponentModel.DataAnnotations;
using backend.Models.Orders;

namespace backend.DTOs.OrderDtos;

public class CreateOrderDto
{
    [Required]
    [MaxLength(50)]
    public string CustomerOrderId { get; set; } = string.Empty;

    public int UserId { get; set; }

    [Required]
    [MaxLength(200)]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    [EmailAddress]
    public string CustomerEmail { get; set; } = string.Empty;

    [MaxLength(20)]
    [Phone]
    public string? CustomerPhone { get; set; }

    [Range(0, double.MaxValue)]
    public decimal TotalAmount { get; set; }

    [Range(0, double.MaxValue)]
    public decimal OriginalSubtotal { get; set; }

    [Range(0, double.MaxValue)]
    public decimal ProductDiscountTotal { get; set; }

    [MaxLength(100)]
    public string? CouponCode { get; set; }

    [MaxLength(100)]
    public string? IdempotencyKey { get; set; }

    [Range(0, double.MaxValue)]
    public decimal CouponDiscountAmount { get; set; }

    [Range(0, double.MaxValue)]
    public decimal FinalAmount { get; set; }

    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.PENDING;
    public OrderStatus OrderStatus { get; set; } = OrderStatus.CREATED;
    public string? Notes { get; set; }
}
