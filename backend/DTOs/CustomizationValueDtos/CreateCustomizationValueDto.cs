using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.CustomizationValueDtos;

public class CreateCustomizationValueDto
{
    [Required]
    public int CustomizationOptionId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Value { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal AdditionalPrice { get; set; }
}
