using backend.DTOs.CustomizationImageDtos;

namespace backend.Services.ProductsService.Interfaces;

public interface ICustomizationImageService
{
    Task<IEnumerable<CustomizationImageResponseDto>> GetByProductIdAsync(int productId);
    Task<CustomizationImageResponseDto?> GetByIdAsync(int id);
    Task<CustomizationImageResponseDto> CreateAsync(CreateCustomizationImageDto dto);
    Task<CustomizationImageResponseDto?> UpdateAsync(int id, UpdateCustomizationImageDto dto);
    Task<bool> DeleteAsync(int id);
}
