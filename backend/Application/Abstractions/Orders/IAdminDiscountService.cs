using backend.DTOs.DiscountDtos;

namespace backend.Application.Abstractions.Orders;

public interface IAdminDiscountService
{
    Task<IEnumerable<DiscountResponseDto>> GetAllAsync();
    Task<DiscountResponseDto> GetByIdAsync(string id);
    Task<DiscountResponseDto> CreateAsync(CreateDiscountDto dto);
    Task<DiscountResponseDto> UpdateAsync(string id, UpdateDiscountDto dto);
    Task DeleteAsync(string id);
}
