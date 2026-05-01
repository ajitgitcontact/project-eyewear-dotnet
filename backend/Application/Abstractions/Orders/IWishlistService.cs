using backend.DTOs.WishlistDtos;

namespace backend.Application.Abstractions.Orders;

public interface IWishlistService
{
    Task<WishlistResponseDto> GetAsync(int userId);
    Task<WishlistResponseDto> AddItemAsync(int userId, AddWishlistItemDto dto);
    Task<WishlistResponseDto> RemoveItemAsync(int userId, string wishlistItemId);
    Task<MoveWishlistItemToCartResponseDto> MoveToCartAsync(int userId, string wishlistItemId, MoveWishlistItemToCartDto dto);
}
