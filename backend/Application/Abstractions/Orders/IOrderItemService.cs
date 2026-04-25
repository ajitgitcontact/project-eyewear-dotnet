using backend.DTOs.OrderItemDtos;

namespace backend.Application.Abstractions.Orders;

public interface IOrderItemService
{
    Task<IEnumerable<OrderItemResponseDto>> GetByCustomerOrderIdAsync(string customerOrderId);
    Task<OrderItemResponseDto> GetByIdAsync(string id);
    Task<OrderItemResponseDto> CreateAsync(CreateOrderItemDto dto);
    Task<OrderItemResponseDto> UpdateAsync(string id, UpdateOrderItemDto dto);
    Task DeleteAsync(string id);
}
