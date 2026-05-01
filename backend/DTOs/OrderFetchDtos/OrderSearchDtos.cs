using backend.Models.Orders;

namespace backend.DTOs.OrderFetchDtos;

public class OrderSearchRequestDto
{
    public DateTime? FromCreatedDate { get; set; }
    public DateTime? ToCreatedDate { get; set; }
    public string? OrderStatus { get; set; }
    public string? PaymentStatus { get; set; }
    public string? CustomerOrderId { get; set; }
    public string? Email { get; set; }
    public string? ContactNumber { get; set; }
    public int? UserId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class OrderSearchResponseDto
{
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public IEnumerable<OrderSearchItemDto> Orders { get; set; } = new List<OrderSearchItemDto>();
}

public class OrderSearchItemDto
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
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CustomerOrderListRequestDto
{
    public DateTime? FromCreatedDate { get; set; }
    public DateTime? ToCreatedDate { get; set; }
    public string? OrderStatus { get; set; }
    public string? PaymentStatus { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class CustomerOrderListResponseDto
{
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public IEnumerable<CustomerOrderListItemDto> Orders { get; set; } = new List<CustomerOrderListItemDto>();
}

public class CustomerOrderListItemDto
{
    public string CustomerOrderId { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public OrderStatus OrderStatus { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
