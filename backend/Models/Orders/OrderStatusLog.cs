using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models.Orders;

[Table("OrderStatusLogs")]
public class OrderStatusLog
{
    [Key]
    [Column(TypeName = "varchar")]
    public string OrderStatusLogsId { get; set; } = PrefixedId.Create("order_status_logs");

    [Required]
    [MaxLength(50)]
    [Column(TypeName = "varchar(50)")]
    public string CustomerOrderId { get; set; } = string.Empty;

    [Column(TypeName = "order_status")]
    public OrderStatus Status { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Order Order { get; set; } = null!;
}
