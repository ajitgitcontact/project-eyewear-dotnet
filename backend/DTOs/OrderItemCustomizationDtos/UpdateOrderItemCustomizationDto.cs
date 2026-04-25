using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.OrderItemCustomizationDtos;

public class UpdateOrderItemCustomizationDto
{
    public int? CustomizationOptionId { get; set; }
    public int? CustomizationValueId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Type { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Value { get; set; } = string.Empty;
}
