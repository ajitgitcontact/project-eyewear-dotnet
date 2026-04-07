using backend.DTOs.ProductDtos;

namespace backend.Services.ProductsService.Interfaces;

public interface IProductService
{
    Task<IEnumerable<ProductResponseDto>> GetAllAsync();
    Task<ProductResponseDto?> GetByIdAsync(int id);
    Task<ProductResponseDto?> GetBySkuAsync(string sku);
    Task<ProductResponseDto> CreateAsync(CreateProductDto dto);
    Task<ProductResponseDto?> UpdateAsync(int id, UpdateProductDto dto);
    Task<bool> DeleteAsync(int id);
}
