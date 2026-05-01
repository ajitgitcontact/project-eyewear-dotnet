using backend.DTOs.CustomerPrescriptionDtos;
using backend.DTOs.OrderAddressDtos;
using backend.DTOs.OrderDtos;
using backend.DTOs.OrderItemCustomizationDtos;
using backend.DTOs.OrderItemDtos;
using backend.DTOs.OrderStatusLogDtos;
using backend.DTOs.PaymentDtos;

namespace backend.DTOs.OrderCreationDtos;

public class OrderCreationResponseDto
{
    public string CustomerOrderId { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public OrderResponseDto Order { get; set; } = new();
    public OrderAddressResponseDto Address { get; set; } = new();
    public List<OrderCreationItemResponseDto> Items { get; set; } = new();
    public CustomerPrescriptionResponseDto? Prescription { get; set; }
    public PaymentResponseDto Payment { get; set; } = new();
    public OrderStatusLogResponseDto StatusLog { get; set; } = new();
}

public class OrderCreationItemResponseDto
{
    public OrderItemResponseDto Item { get; set; } = new();
    public List<OrderItemCustomizationResponseDto> Customizations { get; set; } = new();
}

public class DiscountCalculationResultDto
{
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public string? CouponCode { get; set; }
}
