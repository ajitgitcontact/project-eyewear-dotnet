using backend.DTOs.ProductImageDtos;

namespace backend.Services.ProductsService.Interfaces;

public interface IProductImageService
{
    Task<IEnumerable<ProductImageResponseDto>> GetByProductIdAsync(int productId);
    Task<ProductImageResponseDto?> GetByIdAsync(int id);
    Task<ProductImageResponseDto> CreateAsync(CreateProductImageDto dto);
    Task<ProductImageResponseDto?> UpdateAsync(int id, UpdateProductImageDto dto);
    Task<bool> DeleteAsync(int id);
}
