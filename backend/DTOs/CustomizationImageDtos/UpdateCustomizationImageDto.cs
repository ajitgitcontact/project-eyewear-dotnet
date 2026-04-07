using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.CustomizationImageDtos;

public class UpdateCustomizationImageDto
{
    [Required]
    [MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty;
}
