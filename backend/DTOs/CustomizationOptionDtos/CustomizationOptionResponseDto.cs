namespace backend.DTOs.CustomizationOptionDtos;

public class CustomizationOptionResponseDto
{
    public int CustomizationOptionId { get; set; }
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
}
