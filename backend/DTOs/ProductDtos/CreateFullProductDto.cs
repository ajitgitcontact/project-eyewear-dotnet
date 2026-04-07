using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.ProductDtos;

// ─── Composite request: creates product + everything in one call ───

public class CreateFullProductDto
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

    public List<CreateFullCustomizationOptionDto> CustomizationOptions { get; set; } = new();

    public List<CreateFullProductImageDto> Images { get; set; } = new();
}

public class CreateFullCustomizationOptionDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public bool IsRequired { get; set; }

    public int DisplayOrder { get; set; }

    public List<CreateFullCustomizationValueDto> Values { get; set; } = new();
}

public class CreateFullCustomizationValueDto
{
    [Required]
    [MaxLength(100)]
    public string Value { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal AdditionalPrice { get; set; }

    public List<CreateFullCustomizationImageDto> CustomizationImages { get; set; } = new();
}

public class CreateFullProductImageDto
{
    [Required]
    [MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }

    public int DisplayOrder { get; set; }
}

public class CreateFullCustomizationImageDto
{
    [Required]
    [MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty;
}
