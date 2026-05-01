using backend.Models.Orders;

namespace backend.DTOs.OrderStatusLogDtos;

public class UpdateOrderStatusLogDto
{
    public OrderStatus Status { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }
    public string EventType { get; set; } = "ORDER_STATUS_UPDATED";
    public string? Comment { get; set; }
    public string? LogMessage { get; set; }
    public int? CreatedByUserId { get; set; }
}
