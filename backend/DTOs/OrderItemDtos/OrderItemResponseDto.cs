namespace backend.DTOs.OrderItemDtos;

public class OrderItemResponseDto
{
    public string OrderItemsId { get; set; } = string.Empty;
    public string CustomerOrderId { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal OriginalUnitPrice { get; set; }
    public decimal ProductDiscountAmount { get; set; }
    public decimal FinalUnitPrice { get; set; }
    public decimal FinalLineTotal { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
