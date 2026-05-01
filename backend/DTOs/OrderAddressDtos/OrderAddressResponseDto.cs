using backend.Models.Orders;

namespace backend.DTOs.OrderAddressDtos;

public class OrderAddressResponseDto
{
    public string OrderAddressesId { get; set; } = string.Empty;
    public string CustomerOrderId { get; set; } = string.Empty;
    public AddressType Type { get; set; }
    public string Line1 { get; set; } = string.Empty;
    public string? Line2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Pincode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
