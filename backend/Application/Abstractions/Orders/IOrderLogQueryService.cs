using backend.DTOs.OrderStatusLogDtos;

namespace backend.Application.Abstractions.Orders;

public interface IOrderLogQueryService
{
    Task<IEnumerable<OrderStatusLogResponseDto>> GetLogsForAdminAsync(string customerOrderId);
}
