using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend.Models.Products;

namespace backend.Models.Orders;

public enum DiscountValueType
{
    PERCENTAGE,
    FLAT
}

public enum DiscountAppliesTo
{
    ALL,
    PRODUCT
}

[Table("Discounts")]
public class Discount
{
    [Key]
    [Column(TypeName = "varchar")]
    public string DiscountId { get; set; } = PrefixedId.Create("discounts");

    [Required]
    [MaxLength(150)]
    public string DiscountName { get; set; } = string.Empty;

    public DiscountValueType DiscountType { get; set; }

    [Column(TypeName = "numeric(10,2)")]
    public decimal DiscountValue { get; set; }

    public DiscountAppliesTo AppliesTo { get; set; } = DiscountAppliesTo.ALL;

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<DiscountProduct> DiscountProducts { get; set; } = new List<DiscountProduct>();
}

[Table("DiscountProducts")]
public class DiscountProduct
{
    [Key]
    [Column(TypeName = "varchar")]
    public string DiscountProductId { get; set; } = PrefixedId.Create("discount_products");

    [Required]
    [Column(TypeName = "varchar")]
    public string DiscountId { get; set; } = string.Empty;

    public int ProductId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Discount Discount { get; set; } = null!;

    [ForeignKey(nameof(ProductId))]
    public Product Product { get; set; } = null!;
}
