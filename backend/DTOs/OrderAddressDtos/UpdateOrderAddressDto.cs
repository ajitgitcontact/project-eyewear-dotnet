using System.ComponentModel.DataAnnotations;
using backend.Models.Orders;

namespace backend.DTOs.OrderAddressDtos;

public class UpdateOrderAddressDto
{
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
}
