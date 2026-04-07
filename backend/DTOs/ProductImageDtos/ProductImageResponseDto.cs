namespace backend.DTOs.ProductImageDtos;

public class ProductImageResponseDto
{
    public int ProductImageId { get; set; }
    public int ProductId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
}
