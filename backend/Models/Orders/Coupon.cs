using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend.Models;

namespace backend.Models.Orders;

[Table("Coupons")]
public class Coupon
{
    [Key]
    [Column(TypeName = "varchar")]
    public string CouponId { get; set; } = PrefixedId.Create("coupons");

    [Required]
    [MaxLength(100)]
    public string CouponCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string CouponName { get; set; } = string.Empty;

    public DiscountValueType CouponType { get; set; }

    [Column(TypeName = "numeric(10,2)")]
    public decimal CouponValue { get; set; }

    [Column(TypeName = "numeric(10,2)")]
    public decimal? MinimumOrderAmount { get; set; }

    [Column(TypeName = "numeric(10,2)")]
    public decimal? MaximumCouponAmount { get; set; }

    public int? UsageLimit { get; set; }

    public int? PerUserUsageLimit { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<CouponUsage> CouponUsages { get; set; } = new List<CouponUsage>();
}

[Table("CouponUsages")]
public class CouponUsage
{
    [Key]
    [Column(TypeName = "varchar")]
    public string CouponUsageId { get; set; } = PrefixedId.Create("coupon_usages");

    [Required]
    [Column(TypeName = "varchar")]
    public string CouponId { get; set; } = string.Empty;

    public int UserId { get; set; }

    [Required]
    [MaxLength(50)]
    [Column(TypeName = "varchar(50)")]
    public string CustomerOrderId { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string CouponCode { get; set; } = string.Empty;

    [Column(TypeName = "numeric(10,2)")]
    public decimal CouponAmount { get; set; }

    public DateTime UsedAt { get; set; } = DateTime.UtcNow;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Coupon Coupon { get; set; } = null!;

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    public Order Order { get; set; } = null!;
}
