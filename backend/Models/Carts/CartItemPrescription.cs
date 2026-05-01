using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend.Models;
using backend.Models.Orders;

namespace backend.Models.Carts;

[Table("CartItemPrescriptions")]
public class CartItemPrescription
{
    [Key]
    [Column(TypeName = "varchar")]
    public string CartItemPrescriptionId { get; set; } = PrefixedId.Create("cart_item_prescriptions");

    [Required]
    [Column(TypeName = "varchar")]
    public string CartItemId { get; set; } = string.Empty;

    public int UserId { get; set; }

    [Column(TypeName = "varchar")]
    public string? CustomerPrescriptionsId { get; set; }

    [Column(TypeName = "numeric(5,2)")]
    public decimal? RightSphere { get; set; }

    [Column(TypeName = "numeric(5,2)")]
    public decimal? RightCylinder { get; set; }

    public int? RightAxis { get; set; }

    [Column(TypeName = "numeric(5,2)")]
    public decimal? RightAdd { get; set; }

    [Column(TypeName = "numeric(5,2)")]
    public decimal? LeftSphere { get; set; }

    [Column(TypeName = "numeric(5,2)")]
    public decimal? LeftCylinder { get; set; }

    public int? LeftAxis { get; set; }

    [Column(TypeName = "numeric(5,2)")]
    public decimal? LeftAdd { get; set; }

    [Column(TypeName = "numeric(5,2)")]
    public decimal? PD { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public CartItem CartItem { get; set; } = null!;

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    [ForeignKey(nameof(CustomerPrescriptionsId))]
    public CustomerPrescription? CustomerPrescription { get; set; }
}
