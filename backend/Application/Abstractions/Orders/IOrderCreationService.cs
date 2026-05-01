using backend.DTOs.OrderCreationDtos;

namespace backend.Application.Abstractions.Orders;

public interface IOrderCreationService
{
    Task<OrderCreationResponseDto> CreateAsync(int userId, OrderCreationRequestDto dto, string? idempotencyKey = null, Func<string, Task>? beforeCommitAsync = null);
    Task<OrderCreationResponseDto?> GetByIdempotencyKeyAsync(int userId, string idempotencyKey);
}
