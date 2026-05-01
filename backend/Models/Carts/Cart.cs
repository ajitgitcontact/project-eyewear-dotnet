using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend.Models;
using backend.Models.Orders;

namespace backend.Models.Carts;

public enum CartStatus
{
    ACTIVE,
    CHECKED_OUT,
    ABANDONED
}

[Table("Carts")]
public class Cart
{
    [Key]
    [Column(TypeName = "varchar")]
    public string CartId { get; set; } = PrefixedId.Create("carts");

    public int UserId { get; set; }

    public CartStatus CartStatus { get; set; } = CartStatus.ACTIVE;

    [MaxLength(50)]
    [Column(TypeName = "varchar(50)")]
    public string? CustomerOrderId { get; set; }

    public DateTime? CheckedOutAt { get; set; }

    public DateTime? AbandonedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public ICollection<CartCoupon> CartCoupons { get; set; } = new List<CartCoupon>();
}
