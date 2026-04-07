using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.CustomizationOptionDtos;

public class CreateCustomizationOptionDto
{
    [Required]
    public int ProductId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public bool IsRequired { get; set; }

    public int DisplayOrder { get; set; }
}
