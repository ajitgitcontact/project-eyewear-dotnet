using System.ComponentModel.DataAnnotations;
using backend.Models.Orders;

namespace backend.DTOs.OrderStatusLogDtos;

public class CreateOrderStatusLogDto
{
    [Required]
    [MaxLength(50)]
    public string CustomerOrderId { get; set; } = string.Empty;

    public OrderStatus Status { get; set; }
    public string? Comment { get; set; }
}
