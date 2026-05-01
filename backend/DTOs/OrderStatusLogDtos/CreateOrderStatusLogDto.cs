using System.ComponentModel.DataAnnotations;
using backend.Models.Orders;

namespace backend.DTOs.OrderStatusLogDtos;

public class CreateOrderStatusLogDto
{
    [Required]
    [MaxLength(50)]
    public string CustomerOrderId { get; set; } = string.Empty;

    public OrderStatus Status { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }

    [Required]
    [MaxLength(100)]
    public string EventType { get; set; } = "ORDER_STATUS_UPDATED";

    public string? Comment { get; set; }
    public string? LogMessage { get; set; }
    public int? CreatedByUserId { get; set; }
}
