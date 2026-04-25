using backend.Models.Orders;

namespace backend.DTOs.OrderStatusLogDtos;

public class UpdateOrderStatusLogDto
{
    public OrderStatus Status { get; set; }
    public string? Comment { get; set; }
}
