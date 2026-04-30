using System.ComponentModel.DataAnnotations;
using backend.Models.Orders;

namespace backend.DTOs.OrderCreationDtos;

public class OrderCreationRequestDto
{
    [Required]
    public OrderCreationCustomerDto Customer { get; set; } = new();

    [Required]
    public OrderCreationAddressDto Address { get; set; } = new();

    [MinLength(1)]
    public List<OrderCreationItemDto> Items { get; set; } = new();

    public OrderCreationPrescriptionDto? Prescription { get; set; }

    [Required]
    public OrderCreationPaymentDto Payment { get; set; } = new();

    public string? Notes { get; set; }

    [MaxLength(100)]
    public string? CouponCode { get; set; }

    [MaxLength(100)]
    public string? DiscountCode { get; set; }
}

public class OrderCreationCustomerDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    [Phone]
    public string? Phone { get; set; }
}

public class OrderCreationAddressDto
{
    public AddressType Type { get; set; } = AddressType.SHIPPING;

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

public class OrderCreationItemDto
{
    public int ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    public List<OrderCreationCustomizationDto> Customizations { get; set; } = new();
}

public class OrderCreationCustomizationDto
{
    public int? CustomizationOptionId { get; set; }

    public int? CustomizationValueId { get; set; }

    [MaxLength(100)]
    public string? Type { get; set; }

    [MaxLength(100)]
    public string? Value { get; set; }
}

public class OrderCreationPrescriptionDto
{
    public decimal? RightSphere { get; set; }
    public decimal? RightCylinder { get; set; }

    [Range(0, 180)]
    public int? RightAxis { get; set; }

    public decimal? RightAdd { get; set; }
    public decimal? LeftSphere { get; set; }
    public decimal? LeftCylinder { get; set; }

    [Range(0, 180)]
    public int? LeftAxis { get; set; }

    public decimal? LeftAdd { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? PD { get; set; }

    public string? Notes { get; set; }
}

public class OrderCreationPaymentDto
{
    public PaymentMethod Method { get; set; }

    [MaxLength(150)]
    public string? TransactionId { get; set; }
}
