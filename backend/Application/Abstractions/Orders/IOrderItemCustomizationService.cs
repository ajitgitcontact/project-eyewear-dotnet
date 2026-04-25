using backend.DTOs.OrderItemCustomizationDtos;

namespace backend.Application.Abstractions.Orders;

public interface IOrderItemCustomizationService
{
    Task<IEnumerable<OrderItemCustomizationResponseDto>> GetByOrderItemIdAsync(string orderItemId);
    Task<OrderItemCustomizationResponseDto> GetByIdAsync(string id);
    Task<OrderItemCustomizationResponseDto> CreateAsync(CreateOrderItemCustomizationDto dto);
    Task<OrderItemCustomizationResponseDto> UpdateAsync(string id, UpdateOrderItemCustomizationDto dto);
    Task DeleteAsync(string id);
}
