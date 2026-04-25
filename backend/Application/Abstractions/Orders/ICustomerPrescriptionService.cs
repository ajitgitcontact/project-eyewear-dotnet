using backend.DTOs.CustomerPrescriptionDtos;

namespace backend.Application.Abstractions.Orders;

public interface ICustomerPrescriptionService
{
    Task<IEnumerable<CustomerPrescriptionResponseDto>> GetByCustomerOrderIdAsync(string customerOrderId);
    Task<IEnumerable<CustomerPrescriptionResponseDto>> GetByUserIdAsync(int userId);
    Task<CustomerPrescriptionResponseDto> GetByIdAsync(string id);
    Task<CustomerPrescriptionResponseDto> CreateAsync(CreateCustomerPrescriptionDto dto);
    Task<CustomerPrescriptionResponseDto> UpdateAsync(string id, UpdateCustomerPrescriptionDto dto);
    Task DeleteAsync(string id);
}
