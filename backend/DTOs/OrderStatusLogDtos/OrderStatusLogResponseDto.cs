using backend.Models.Orders;

namespace backend.DTOs.OrderStatusLogDtos;

public class OrderStatusLogResponseDto
{
    public string OrderStatusLogsId { get; set; } = string.Empty;
    public string CustomerOrderId { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public string? LogMessage { get; set; }
    public int? CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
