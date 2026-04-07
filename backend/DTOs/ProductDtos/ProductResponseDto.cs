namespace backend.DTOs.ProductDtos;

public class ProductResponseDto
{
    public int ProductId { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Brand { get; set; }
    public string Category { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public bool HasPrescription { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
