using backend.DTOs.CustomizationValueDtos;

namespace backend.Application.Abstractions.Products;

public interface ICustomizationValueService
{
    Task<IEnumerable<CustomizationValueResponseDto>> GetByOptionIdAsync(int customizationOptionId);
    Task<CustomizationValueResponseDto> GetByIdAsync(int id);
    Task<CustomizationValueResponseDto> CreateAsync(CreateCustomizationValueDto dto);
    Task<CustomizationValueResponseDto> UpdateAsync(int id, UpdateCustomizationValueDto dto);
    Task DeleteAsync(int id);
}
