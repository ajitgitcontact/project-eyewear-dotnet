using backend.DTOs.ProductDtos;

namespace backend.Application.Abstractions.Products;

public interface IProductService
{
    Task<IEnumerable<ProductResponseDto>> GetAllAsync();
    Task<ProductResponseDto> GetByIdAsync(int id);
    Task<ProductResponseDto> GetBySkuAsync(string sku);
    Task<ProductResponseDto> CreateAsync(CreateProductDto dto);
    Task<ProductResponseDto> UpdateAsync(int id, UpdateProductDto dto);
    Task DeleteAsync(int id);
}
