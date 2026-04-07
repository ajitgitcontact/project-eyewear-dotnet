using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.CustomizationOptionDtos;

public class UpdateCustomizationOptionDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public bool IsRequired { get; set; }

    public int DisplayOrder { get; set; }
}
