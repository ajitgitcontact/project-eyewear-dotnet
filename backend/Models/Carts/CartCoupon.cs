using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend.Models.Orders;

namespace backend.Models.Carts;

[Table("CartCoupons")]
public class CartCoupon
{
    [Key]
    [Column(TypeName = "varchar")]
    public string CartCouponId { get; set; } = PrefixedId.Create("cart_coupons");

    [Required]
    [Column(TypeName = "varchar")]
    public string CartId { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "varchar")]
    public string CouponId { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string CouponCode { get; set; } = string.Empty;

    [Column(TypeName = "numeric(10,2)")]
    public decimal CouponDiscountAmount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Cart Cart { get; set; } = null!;

    public Coupon Coupon { get; set; } = null!;
}
