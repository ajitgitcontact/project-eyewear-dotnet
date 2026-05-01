using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models.Orders;

[Table("OrderAddresses")]
public class OrderAddress
{
    [Key]
    [Column(TypeName = "varchar")]
    public string OrderAddressesId { get; set; } = PrefixedId.Create("order_addresses");

    [Required]
    [MaxLength(50)]
    [Column(TypeName = "varchar(50)")]
    public string CustomerOrderId { get; set; } = string.Empty;

    [Column(TypeName = "address_type")]
    public AddressType Type { get; set; }

    [Required]
    [MaxLength(250)]
    public string Line1 { get; set; } = string.Empty;

    [MaxLength(250)]
    public string? Line2 { get; set; }

    [Required]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string State { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Pincode { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Country { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Order Order { get; set; } = null!;
}
