using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.CustomizationValueDtos;

public class UpdateCustomizationValueDto
{
    [Required]
    [MaxLength(100)]
    public string Value { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal AdditionalPrice { get; set; }
}
