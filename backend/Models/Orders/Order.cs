using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend.Models;

namespace backend.Models.Orders;

[Table("Orders")]
public class Order
{
    [Key]
    [Column(TypeName = "varchar")]
    public string OrdersId { get; set; } = PrefixedId.Create("orders");

    [Required]
    [MaxLength(50)]
    [Column(TypeName = "varchar(50)")]
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

    [Column(TypeName = "numeric(10,2)")]
    public decimal TotalAmount { get; set; }

    [Column(TypeName = "payment_status")]
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.PENDING;

    [Column(TypeName = "order_status")]
    public OrderStatus OrderStatus { get; set; } = OrderStatus.CREATED;

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<OrderAddress> OrderAddresses { get; set; } = new List<OrderAddress>();
    public ICollection<OrderStatusLog> OrderStatusLogs { get; set; } = new List<OrderStatusLog>();
    public ICollection<CustomerPrescription> CustomerPrescriptions { get; set; } = new List<CustomerPrescription>();
}
