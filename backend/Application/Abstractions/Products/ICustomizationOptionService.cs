using backend.DTOs.CustomizationOptionDtos;

namespace backend.Application.Abstractions.Products;

public interface ICustomizationOptionService
{
    Task<IEnumerable<CustomizationOptionResponseDto>> GetByProductIdAsync(int productId);
    Task<CustomizationOptionResponseDto> GetByIdAsync(int id);
    Task<CustomizationOptionResponseDto> CreateAsync(CreateCustomizationOptionDto dto);
    Task<CustomizationOptionResponseDto> UpdateAsync(int id, UpdateCustomizationOptionDto dto);
    Task DeleteAsync(int id);
}
