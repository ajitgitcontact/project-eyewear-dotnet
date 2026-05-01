using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.OrderItemCustomizationDtos;

public class CreateOrderItemCustomizationDto
{
    [Required]
    public string OrderItemId { get; set; } = string.Empty;

    public int? CustomizationOptionId { get; set; }
    public int? CustomizationValueId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Type { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Value { get; set; } = string.Empty;
}
