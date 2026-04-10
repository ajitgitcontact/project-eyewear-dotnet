using backend.DTOs.CustomizationOptionDtos;
using backend.DTOs.ProductImageDtos;

namespace backend.DTOs.ProductDtos;

public class FullProductResponseDto
{
    public int ProductId { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Brand { get; set; }
    public string Category { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public int AvailableQuantity { get; set; }
    public int SoldQuantity { get; set; }
    public int Priority { get; set; }
    public bool HasPrescription { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public List<FullCustomizationOptionResponseDto> CustomizationOptions { get; set; } = new();
    public List<ProductImageResponseDto> Images { get; set; } = new();
}

public class FullCustomizationOptionResponseDto
{
    public int CustomizationOptionId { get; set; }
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<FullCustomizationValueResponseDto> Values { get; set; } = new();
}

public class FullCustomizationValueResponseDto
{
    public int CustomizationValueId { get; set; }
    public int CustomizationOptionId { get; set; }
    public string Value { get; set; } = string.Empty;
    public decimal AdditionalPrice { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<FullCustomizationImageResponseDto> CustomizationImages { get; set; } = new();
}

public class FullCustomizationImageResponseDto
{
    public int CustomizationImageId { get; set; }
    public int ProductId { get; set; }
    public int CustomizationOptionId { get; set; }
    public int CustomizationValueId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
