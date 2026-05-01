using System.ComponentModel.DataAnnotations;
using backend.DTOs.OrderCreationDtos;
using backend.Models.Carts;

namespace backend.DTOs.CartDtos;

public class AddCartItemDto
{
    public int ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    public List<CartItemCustomizationRequestDto> Customizations { get; set; } = new();

    public string? CustomerPrescriptionsId { get; set; }

    public CartItemPrescriptionRequestDto? Prescription { get; set; }
}

public class CartItemCustomizationRequestDto
{
    public int? CustomizationOptionId { get; set; }
    public int? CustomizationValueId { get; set; }
}

public class CartItemPrescriptionRequestDto
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

public class UpdateCartItemQuantityDto
{
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}

public class ApplyCartCouponDto
{
    [Required]
    [MaxLength(100)]
    public string CouponCode { get; set; } = string.Empty;
}

public class CartCheckoutRequestDto
{
    [Required]
    public OrderCreationCustomerDto Customer { get; set; } = new();

    [Required]
    public OrderCreationAddressDto Address { get; set; } = new();

    [Required]
    public OrderCreationPaymentDto Payment { get; set; } = new();

    public string? Notes { get; set; }
}

public class CartResponseDto
{
    public string? CartId { get; set; }
    public CartStatus CartStatus { get; set; } = CartStatus.ACTIVE;
    public List<CartItemResponseDto> Items { get; set; } = new();
    public string? CouponCode { get; set; }
    public decimal CouponDiscountAmount { get; set; }
    public decimal Subtotal { get; set; }
    public decimal ProductDiscountTotal { get; set; }
    public decimal FinalAmount { get; set; }
}

public class CartItemResponseDto
{
    public string CartItemId { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }
    public string SKU { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal ProductDiscountAmount { get; set; }
    public decimal FinalUnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public bool InStock { get; set; }
    public List<CartItemCustomizationResponseDto> Customizations { get; set; } = new();
    public CartItemPrescriptionResponseDto? Prescription { get; set; }
}

public class CartItemCustomizationResponseDto
{
    public string CartItemCustomizationId { get; set; } = string.Empty;
    public int? CustomizationOptionId { get; set; }
    public int? CustomizationValueId { get; set; }
    public string CustomizationName { get; set; } = string.Empty;
    public string CustomizationValue { get; set; } = string.Empty;
    public decimal ExtraPrice { get; set; }
}

public class CartItemPrescriptionResponseDto
{
    public string CartItemPrescriptionId { get; set; } = string.Empty;
    public string? CustomerPrescriptionsId { get; set; }
    public decimal? RightSphere { get; set; }
    public decimal? RightCylinder { get; set; }
    public int? RightAxis { get; set; }
    public decimal? RightAdd { get; set; }
    public decimal? LeftSphere { get; set; }
    public decimal? LeftCylinder { get; set; }
    public int? LeftAxis { get; set; }
    public decimal? LeftAdd { get; set; }
    public decimal? PD { get; set; }
    public string? Notes { get; set; }
}
