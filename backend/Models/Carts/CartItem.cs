using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend.Models.Orders;
using backend.Models.Products;

namespace backend.Models.Carts;

[Table("CartItems")]
public class CartItem
{
    [Key]
    [Column(TypeName = "varchar")]
    public string CartItemId { get; set; } = PrefixedId.Create("cart_items");

    [Required]
    [Column(TypeName = "varchar")]
    public string CartId { get; set; } = string.Empty;

    public int ProductId { get; set; }

    [Required]
    [MaxLength(50)]
    public string SKU { get; set; } = string.Empty;

    public int Quantity { get; set; }

    [Column(TypeName = "numeric(10,2)")]
    public decimal UnitPrice { get; set; }

    [Column(TypeName = "numeric(10,2)")]
    public decimal ProductDiscountAmount { get; set; }

    [Column(TypeName = "numeric(10,2)")]
    public decimal FinalUnitPrice { get; set; }

    [Column(TypeName = "numeric(10,2)")]
    public decimal LineTotal { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Cart Cart { get; set; } = null!;

    [ForeignKey(nameof(ProductId))]
    public Product Product { get; set; } = null!;

    public ICollection<CartItemCustomization> Customizations { get; set; } = new List<CartItemCustomization>();

    public CartItemPrescription? Prescription { get; set; }
}
