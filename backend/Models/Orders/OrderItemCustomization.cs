using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend.Models.Products;

namespace backend.Models.Orders;

[Table("OrderItemCustomizations")]
public class OrderItemCustomization
{
    [Key]
    [Column(TypeName = "varchar")]
    public string OrderItemCustomizationsId { get; set; } = PrefixedId.Create("order_item_customizations");

    [Required]
    [Column(TypeName = "varchar")]
    public string OrderItemId { get; set; } = string.Empty;

    public int? CustomizationOptionId { get; set; }

    public int? CustomizationValueId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Type { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Value { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(OrderItemId))]
    public OrderItem OrderItem { get; set; } = null!;

    [ForeignKey(nameof(CustomizationOptionId))]
    public CustomizationOption? CustomizationOption { get; set; }

    [ForeignKey(nameof(CustomizationValueId))]
    public CustomizationValue? CustomizationValue { get; set; }
}
