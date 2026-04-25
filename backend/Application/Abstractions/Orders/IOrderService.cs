using backend.DTOs.OrderDtos;

namespace backend.Application.Abstractions.Orders;

public interface IOrderService
{
    Task<IEnumerable<OrderResponseDto>> GetAllAsync();
    Task<OrderResponseDto> GetByIdAsync(string id);
    Task<OrderResponseDto> GetByCustomerOrderIdAsync(string customerOrderId);
    Task<OrderResponseDto> CreateAsync(CreateOrderDto dto);
    Task<OrderResponseDto> UpdateAsync(string id, UpdateOrderDto dto);
    Task DeleteAsync(string id);
}
