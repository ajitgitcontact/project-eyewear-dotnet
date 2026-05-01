using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend.Models;

namespace backend.Models.Orders;

[Table("CustomerPrescriptions")]
public class CustomerPrescription
{
    [Key]
    [Column(TypeName = "varchar")]
    public string CustomerPrescriptionsId { get; set; } = PrefixedId.Create("customer_prescriptions");

    public int UserId { get; set; }

    [Required]
    [MaxLength(50)]
    [Column(TypeName = "varchar(50)")]
    public string CustomerOrderId { get; set; } = string.Empty;

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

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    public Order Order { get; set; } = null!;
}
