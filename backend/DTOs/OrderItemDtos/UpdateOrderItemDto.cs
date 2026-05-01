using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.OrderItemDtos;

public class UpdateOrderItemDto
{
    public int ProductId { get; set; }

    [Required]
    [MaxLength(50)]
    public string SKU { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string ProductName { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [Range(0, double.MaxValue)]
    public decimal OriginalUnitPrice { get; set; }

    [Range(0, double.MaxValue)]
    public decimal ProductDiscountAmount { get; set; }

    [Range(0, double.MaxValue)]
    public decimal FinalUnitPrice { get; set; }

    [Range(0, double.MaxValue)]
    public decimal FinalLineTotal { get; set; }
}
