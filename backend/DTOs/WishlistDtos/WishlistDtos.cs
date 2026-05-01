using System.ComponentModel.DataAnnotations;
using backend.DTOs.CartDtos;

namespace backend.DTOs.WishlistDtos;

public class AddWishlistItemDto
{
    public int ProductId { get; set; }
}

public class MoveWishlistItemToCartDto
{
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; } = 1;
}

public class WishlistResponseDto
{
    public string? WishlistId { get; set; }
    public List<WishlistItemResponseDto> Items { get; set; } = new();
}

public class WishlistItemResponseDto
{
    public string WishlistItemId { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal CurrentProductDiscount { get; set; }
    public bool IsAvailable { get; set; }
    public int AvailableQuantity { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class MoveWishlistItemToCartResponseDto
{
    public bool RequiresProductConfiguration { get; set; }
    public string? Message { get; set; }
    public CartResponseDto? Cart { get; set; }
}
