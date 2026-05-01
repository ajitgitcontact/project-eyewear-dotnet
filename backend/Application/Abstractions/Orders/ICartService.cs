using backend.DTOs.CartDtos;
using backend.DTOs.OrderCreationDtos;

namespace backend.Application.Abstractions.Orders;

public interface ICartService
{
    Task<CartResponseDto> GetActiveCartAsync(int userId);
    Task<CartResponseDto> AddItemAsync(int userId, AddCartItemDto dto);
    Task<CartResponseDto> UpdateItemQuantityAsync(int userId, string cartItemId, UpdateCartItemQuantityDto dto);
    Task<CartResponseDto> RemoveItemAsync(int userId, string cartItemId);
    Task<CartResponseDto> ClearAsync(int userId);
    Task<CartResponseDto> ApplyCouponAsync(int userId, ApplyCartCouponDto dto);
    Task<CartResponseDto> RemoveCouponAsync(int userId);
    Task<OrderCreationResponseDto> CheckoutAsync(int userId, CartCheckoutRequestDto dto);
}
