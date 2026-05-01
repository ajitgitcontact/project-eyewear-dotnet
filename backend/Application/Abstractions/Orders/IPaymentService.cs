using backend.DTOs.PaymentDtos;

namespace backend.Application.Abstractions.Orders;

public interface IPaymentService
{
    Task<IEnumerable<PaymentResponseDto>> GetByCustomerOrderIdAsync(string customerOrderId);
    Task<PaymentResponseDto> GetByIdAsync(string id);
    Task<PaymentResponseDto> CreateAsync(CreatePaymentDto dto);
    Task<PaymentResponseDto> UpdateAsync(string id, UpdatePaymentDto dto);
    Task DeleteAsync(string id);
}
