using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend.Models.Orders;
using backend.Models.Products;

namespace backend.Models.Carts;

[Table("CartItemCustomizations")]
public class CartItemCustomization
{
    [Key]
    [Column(TypeName = "varchar")]
    public string CartItemCustomizationId { get; set; } = PrefixedId.Create("cart_item_customizations");

    [Required]
    [Column(TypeName = "varchar")]
    public string CartItemId { get; set; } = string.Empty;

    public int? CustomizationOptionId { get; set; }

    public int? CustomizationValueId { get; set; }

    [Required]
    [MaxLength(100)]
    public string CustomizationName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string CustomizationValue { get; set; } = string.Empty;

    [Column(TypeName = "numeric(10,2)")]
    public decimal ExtraPrice { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public CartItem CartItem { get; set; } = null!;

    [ForeignKey(nameof(CustomizationOptionId))]
    public CustomizationOption? CustomizationOption { get; set; }

    [ForeignKey(nameof(CustomizationValueId))]
    public CustomizationValue? CustomizationValueEntity { get; set; }
}
