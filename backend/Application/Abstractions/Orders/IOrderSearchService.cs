using backend.DTOs.OrderFetchDtos;

namespace backend.Application.Abstractions.Orders;

public interface IOrderSearchService
{
    Task<OrderSearchResponseDto> SearchAsync(OrderSearchRequestDto request);
}
