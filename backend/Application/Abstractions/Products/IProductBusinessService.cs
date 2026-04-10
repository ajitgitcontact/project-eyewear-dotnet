using backend.DTOs.ProductDtos;

namespace backend.Application.Abstractions.Products;

public interface IProductBusinessService
{
    Task<FullProductResponseDto> CreateFullProductAsync(CreateFullProductDto dto);
    Task<FullProductResponseDto> GetFullProductByIdAsync(int productId);
    Task<IEnumerable<FullProductResponseDto>> GetAllFullProductsAsync();
    Task<IEnumerable<FullProductResponseDto>> GetAllFullProductsAsync(string sortOption);
}
