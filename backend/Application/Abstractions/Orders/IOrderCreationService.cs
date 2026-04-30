using backend.DTOs.OrderCreationDtos;

namespace backend.Application.Abstractions.Orders;

public interface IOrderCreationService
{
    Task<OrderCreationResponseDto> CreateAsync(int userId, OrderCreationRequestDto dto);
}
