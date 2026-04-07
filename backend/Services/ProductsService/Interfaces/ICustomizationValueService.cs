using backend.DTOs.CustomizationValueDtos;

namespace backend.Services.ProductsService.Interfaces;

public interface ICustomizationValueService
{
    Task<IEnumerable<CustomizationValueResponseDto>> GetByOptionIdAsync(int customizationOptionId);
    Task<CustomizationValueResponseDto?> GetByIdAsync(int id);
    Task<CustomizationValueResponseDto> CreateAsync(CreateCustomizationValueDto dto);
    Task<CustomizationValueResponseDto?> UpdateAsync(int id, UpdateCustomizationValueDto dto);
    Task<bool> DeleteAsync(int id);
}
