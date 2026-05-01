using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend.Models.Products;

namespace backend.Models.Orders;

[Table("OrderItems")]
public class OrderItem
{
    [Key]
    [Column(TypeName = "varchar")]
    public string OrderItemsId { get; set; } = PrefixedId.Create("order_items");

    [Required]
    [MaxLength(50)]
    [Column(TypeName = "varchar(50)")]
    public string CustomerOrderId { get; set; } = string.Empty;

    public int ProductId { get; set; }

    [Required]
    [MaxLength(50)]
    public string SKU { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string ProductName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    [Column(TypeName = "numeric(10,2)")]
    public decimal Price { get; set; }

    [Column(TypeName = "numeric(10,2)")]
    public decimal TotalPrice { get; set; }

    [Column(TypeName = "numeric(10,2)")]
    public decimal OriginalUnitPrice { get; set; }

    [Column(TypeName = "numeric(10,2)")]
    public decimal ProductDiscountAmount { get; set; }

    [Column(TypeName = "numeric(10,2)")]
    public decimal FinalUnitPrice { get; set; }

    [Column(TypeName = "numeric(10,2)")]
    public decimal FinalLineTotal { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Order Order { get; set; } = null!;

    [ForeignKey(nameof(ProductId))]
    public Product Product { get; set; } = null!;

    public ICollection<OrderItemCustomization> Customizations { get; set; } = new List<OrderItemCustomization>();
}
