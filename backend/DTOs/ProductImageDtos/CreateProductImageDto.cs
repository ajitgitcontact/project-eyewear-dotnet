using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.ProductImageDtos;

public class CreateProductImageDto
{
    [Required]
    public int ProductId { get; set; }

    [Required]
    [MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }

    public int DisplayOrder { get; set; }
}
