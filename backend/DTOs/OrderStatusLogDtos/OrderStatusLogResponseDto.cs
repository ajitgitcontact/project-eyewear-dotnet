using backend.Models.Orders;

namespace backend.DTOs.OrderStatusLogDtos;

public class OrderStatusLogResponseDto
{
    public string OrderStatusLogsId { get; set; } = string.Empty;
    public string CustomerOrderId { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
