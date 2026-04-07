using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.ProductImageDtos;

public class UpdateProductImageDto
{
    [Required]
    [MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }

    public int DisplayOrder { get; set; }
}
