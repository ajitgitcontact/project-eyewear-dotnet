using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.ProductDtos;

public class CreateProductDto
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

    [Range(0, int.MaxValue)]
    public int AvailableQuantity { get; set; } = 0;

    [Range(0, int.MaxValue)]
    public int SoldQuantity { get; set; } = 0;

    [Range(0, int.MaxValue)]
    public int Priority { get; set; } = 0;

    public bool HasPrescription { get; set; }
}
