using backend.DTOs.CustomizationImageDtos;

namespace backend.Application.Abstractions.Products;

public interface ICustomizationImageService
{
    Task<IEnumerable<CustomizationImageResponseDto>> GetByProductIdAsync(int productId);
    Task<CustomizationImageResponseDto> GetByIdAsync(int id);
    Task<CustomizationImageResponseDto> CreateAsync(CreateCustomizationImageDto dto);
    Task<CustomizationImageResponseDto> UpdateAsync(int id, UpdateCustomizationImageDto dto);
    Task DeleteAsync(int id);
}
