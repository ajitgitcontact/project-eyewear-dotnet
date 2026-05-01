using backend.DTOs.OrderFetchDtos;

namespace backend.Application.Abstractions.Orders;

public interface ICustomerOrderListService
{
    Task<CustomerOrderListResponseDto> GetForCustomerAsync(int userId, CustomerOrderListRequestDto request);
}
