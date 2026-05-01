using backend.DTOs.CouponDtos;

namespace backend.Application.Abstractions.Orders;

public interface IAdminCouponService
{
    Task<IEnumerable<CouponResponseDto>> GetAllAsync();
    Task<CouponResponseDto> GetByIdAsync(string id);
    Task<CouponResponseDto> CreateAsync(CreateCouponDto dto);
    Task<CouponResponseDto> UpdateAsync(string id, UpdateCouponDto dto);
    Task DeleteAsync(string id);
}
