namespace backend.DTOs.OrderItemCustomizationDtos;

public class OrderItemCustomizationResponseDto
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
