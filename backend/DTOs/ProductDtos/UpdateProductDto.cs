using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.ProductDtos;

public class UpdateProductDto
{
    [Required]
    [MaxLength(50)]
    public string SKU { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [MaxLength(100)]
    public string? Brand { get; set; }

    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal BasePrice { get; set; }

    public bool HasPrescription { get; set; }

    public bool IsActive { get; set; } = true;
}
