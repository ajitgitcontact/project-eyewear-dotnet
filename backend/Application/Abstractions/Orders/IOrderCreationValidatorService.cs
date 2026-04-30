using backend.DTOs.OrderCreationDtos;

namespace backend.Application.Abstractions.Orders;

public interface IOrderCreationValidatorService
{
    Task ValidateAsync(int userId, OrderCreationRequestDto dto);
}
