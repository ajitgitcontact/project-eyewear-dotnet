using backend.DTOs.ProductDtos;

namespace backend.Services.ProductsService.Interfaces;

public interface IProductBusinessService
{
    Task<FullProductResponseDto> CreateFullProductAsync(CreateFullProductDto dto);
    Task<FullProductResponseDto?> GetFullProductByIdAsync(int productId);
    Task<IEnumerable<FullProductResponseDto>> GetAllFullProductsAsync();
}
