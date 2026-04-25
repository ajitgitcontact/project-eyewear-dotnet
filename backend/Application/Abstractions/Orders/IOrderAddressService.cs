using backend.DTOs.OrderAddressDtos;

namespace backend.Application.Abstractions.Orders;

public interface IOrderAddressService
{
    Task<IEnumerable<OrderAddressResponseDto>> GetByCustomerOrderIdAsync(string customerOrderId);
    Task<OrderAddressResponseDto> GetByIdAsync(string id);
    Task<OrderAddressResponseDto> CreateAsync(CreateOrderAddressDto dto);
    Task<OrderAddressResponseDto> UpdateAsync(string id, UpdateOrderAddressDto dto);
    Task DeleteAsync(string id);
}
