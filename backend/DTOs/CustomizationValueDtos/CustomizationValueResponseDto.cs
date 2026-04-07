namespace backend.DTOs.CustomizationValueDtos;

public class CustomizationValueResponseDto
{
    public int CustomizationValueId { get; set; }
    public int CustomizationOptionId { get; set; }
    public string Value { get; set; } = string.Empty;
    public decimal AdditionalPrice { get; set; }
    public DateTime CreatedAt { get; set; }
}
