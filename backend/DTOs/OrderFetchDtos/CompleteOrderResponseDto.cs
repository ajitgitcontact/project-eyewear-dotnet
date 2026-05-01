using System.Text.Json.Serialization;
using backend.Models.Orders;

namespace backend.DTOs.OrderFetchDtos;

public class CompleteOrderResponseDto
{
    public CompleteOrderSummaryDto Order { get; set; } = new();
    public List<CompleteOrderItemDto> Items { get; set; } = new();
    public List<CompleteOrderAddressDto> Addresses { get; set; } = new();
    public List<CompleteOrderPrescriptionDto> Prescriptions { get; set; } = new();
    public List<CompleteOrderStatusLogDto> StatusLogs { get; set; } = new();

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<CustomerSafePaymentDto>? CustomerPayments { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<AdminPaymentDto>? AdminPayments { get; set; }
}

public class CompleteOrderSummaryDto
{
    public string OrdersId { get; set; } = string.Empty;
    public string CustomerOrderId { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public decimal TotalAmount { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public OrderStatus OrderStatus { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CompleteOrderItemDto
{
    public string OrderItemsId { get; set; } = string.Empty;
    public string CustomerOrderId { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<CompleteOrderCustomizationDto> Customizations { get; set; } = new();
}

public class CompleteOrderCustomizationDto
{
    public string OrderItemCustomizationsId { get; set; } = string.Empty;
    public string OrderItemId { get; set; } = string.Empty;
    public int? CustomizationOptionId { get; set; }
    public int? CustomizationValueId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CompleteOrderAddressDto
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

public class CompleteOrderPrescriptionDto
{
    public string CustomerPrescriptionsId { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string CustomerOrderId { get; set; } = string.Empty;
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
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CustomerSafePaymentDto
{
    public PaymentMethod Method { get; set; }
    public PaymentTxnStatus Status { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AdminPaymentDto
{
    public string PaymentsId { get; set; } = string.Empty;
    public string CustomerOrderId { get; set; } = string.Empty;
    public PaymentMethod Method { get; set; }
    public string? TransactionId { get; set; }
    public decimal Amount { get; set; }
    public PaymentTxnStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CompleteOrderStatusLogDto
{
    public string OrderStatusLogsId { get; set; } = string.Empty;
    public string CustomerOrderId { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
