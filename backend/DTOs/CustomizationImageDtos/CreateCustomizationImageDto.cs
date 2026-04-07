using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.CustomizationImageDtos;

public class CreateCustomizationImageDto
{
    [Required]
    public int ProductId { get; set; }

    [Required]
    public int CustomizationOptionId { get; set; }

    [Required]
    public int CustomizationValueId { get; set; }

    [Required]
    [MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty;
}
