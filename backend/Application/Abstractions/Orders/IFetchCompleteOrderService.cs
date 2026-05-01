using backend.DTOs.OrderFetchDtos;

namespace backend.Application.Abstractions.Orders;

public interface IFetchCompleteOrderService
{
    Task<CompleteOrderResponseDto> GetByCustomerOrderIdAsync(string customerOrderId, int userId, string role);
}
