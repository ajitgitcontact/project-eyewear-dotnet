namespace backend.DTOs.CustomizationImageDtos;

public class CustomizationImageResponseDto
{
    public int CustomizationImageId { get; set; }
    public int ProductId { get; set; }
    public int CustomizationOptionId { get; set; }
    public int CustomizationValueId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
