using backend.DTOs.OrderStatusLogDtos;

namespace backend.Application.Abstractions.Orders;

public interface IOrderStatusLogService
{
    Task<IEnumerable<OrderStatusLogResponseDto>> GetByCustomerOrderIdAsync(string customerOrderId);
    Task<OrderStatusLogResponseDto> GetByIdAsync(string id);
    Task<OrderStatusLogResponseDto> CreateAsync(CreateOrderStatusLogDto dto);
    Task<OrderStatusLogResponseDto> UpdateAsync(string id, UpdateOrderStatusLogDto dto);
    Task DeleteAsync(string id);
}
