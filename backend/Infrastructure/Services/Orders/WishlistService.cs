using backend.Application.Abstractions.Orders;
using backend.Application.Exceptions;
using backend.Data;
using backend.DTOs.CartDtos;
using backend.DTOs.WishlistDtos;
using backend.Models.Wishlists;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Services.Orders;

public class WishlistService : IWishlistService
{
    private readonly AppDbContext _context;
    private readonly ICartService _cartService;
    private readonly IDiscountService _discountService;
    private readonly ILogger<WishlistService> _logger;

    public WishlistService(
        AppDbContext context,
        ICartService cartService,
        IDiscountService discountService,
        ILogger<WishlistService> logger)
    {
        _context = context;
        _cartService = cartService;
        _discountService = discountService;
        _logger = logger;
    }

    public async Task<WishlistResponseDto> GetAsync(int userId)
    {
        var wishlist = await LoadWishlistAsync(userId);
        if (wishlist is null)
            return new WishlistResponseDto();

        return await MapWishlistAsync(wishlist);
    }

    public async Task<WishlistResponseDto> AddItemAsync(int userId, AddWishlistItemDto dto)
    {
        _logger.LogInformation("Wishlist item add started. UserId={UserId}, ProductId={ProductId}", userId, dto.ProductId);
        var wishlist = await GetOrCreateWishlistAsync(userId);
        var product = await _context.Products
            .Include(p => p.ProductImages)
            .FirstOrDefaultAsync(p => p.ProductId == dto.ProductId);

        if (product is null)
            throw new NotFoundException("Product not found.");

        if (!product.IsActive)
            throw new BadRequestException("Product is not available.");

        if (wishlist.WishlistItems.Any(i => i.ProductId == dto.ProductId))
        {
            _logger.LogInformation("Duplicate wishlist item attempt. UserId={UserId}, ProductId={ProductId}", userId, dto.ProductId);
            return await MapWishlistAsync(wishlist);
        }

        wishlist.WishlistItems.Add(new WishlistItem
        {
            WishlistId = wishlist.WishlistId,
            ProductId = product.ProductId,
            Product = product
        });

        await _context.SaveChangesAsync();
        _logger.LogInformation("Wishlist item added. UserId={UserId}, ProductId={ProductId}", userId, dto.ProductId);
        return await MapWishlistAsync(wishlist);
    }

    public async Task<WishlistResponseDto> RemoveItemAsync(int userId, string wishlistItemId)
    {
        var wishlist = await LoadWishlistAsync(userId) ?? throw new NotFoundException("Wishlist not found.");
        var item = wishlist.WishlistItems.FirstOrDefault(i => i.WishlistItemId == wishlistItemId)
            ?? throw new NotFoundException("Wishlist item not found.");

        _context.WishlistItems.Remove(item);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Wishlist item removed. UserId={UserId}, WishlistItemId={WishlistItemId}", userId, wishlistItemId);
        return await MapWishlistAsync(wishlist);
    }

    public async Task<MoveWishlistItemToCartResponseDto> MoveToCartAsync(int userId, string wishlistItemId, MoveWishlistItemToCartDto dto)
    {
        _logger.LogInformation("Wishlist move to cart started. UserId={UserId}, WishlistItemId={WishlistItemId}", userId, wishlistItemId);
        var wishlist = await LoadWishlistAsync(userId) ?? throw new NotFoundException("Wishlist not found.");
        var item = wishlist.WishlistItems.FirstOrDefault(i => i.WishlistItemId == wishlistItemId)
            ?? throw new NotFoundException("Wishlist item not found.");

        var product = await _context.Products
            .Include(p => p.CustomizationOptions)
            .FirstOrDefaultAsync(p => p.ProductId == item.ProductId)
            ?? throw new NotFoundException("Product not found.");

        if (!product.IsActive)
            throw new BadRequestException("Product is not available.");

        if (product.AvailableQuantity < dto.Quantity)
            throw new BadRequestException("Insufficient product stock.");

        if (product.HasPrescription || product.CustomizationOptions.Any(o => o.IsRequired))
        {
            _logger.LogInformation("Wishlist move to cart requires product configuration. UserId={UserId}, ProductId={ProductId}", userId, product.ProductId);
            return new MoveWishlistItemToCartResponseDto
            {
                RequiresProductConfiguration = true,
                Message = "Select customization or prescription on the product detail page before adding this product to cart."
            };
        }

        var cart = await _cartService.AddItemAsync(userId, new AddCartItemDto
        {
            ProductId = product.ProductId,
            Quantity = dto.Quantity
        });

        _context.WishlistItems.Remove(item);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Wishlist item moved to cart. UserId={UserId}, WishlistItemId={WishlistItemId}, ProductId={ProductId}", userId, wishlistItemId, product.ProductId);

        return new MoveWishlistItemToCartResponseDto
        {
            RequiresProductConfiguration = false,
            Message = "Wishlist item moved to cart.",
            Cart = cart
        };
    }

    private async Task<Wishlist> GetOrCreateWishlistAsync(int userId)
    {
        var wishlist = await LoadWishlistAsync(userId);
        if (wishlist is not null)
            return wishlist;

        _logger.LogInformation("Wishlist creation started. UserId={UserId}", userId);
        wishlist = new Wishlist { UserId = userId };
        _context.Wishlists.Add(wishlist);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Wishlist creation completed. UserId={UserId}, WishlistId={WishlistId}", userId, wishlist.WishlistId);
        return wishlist;
    }

    private async Task<Wishlist?> LoadWishlistAsync(int userId)
    {
        return await _context.Wishlists
            .Include(w => w.WishlistItems)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.ProductImages)
            .FirstOrDefaultAsync(w => w.UserId == userId);
    }

    private async Task<WishlistResponseDto> MapWishlistAsync(Wishlist wishlist)
    {
        var response = new WishlistResponseDto
        {
            WishlistId = wishlist.WishlistId
        };

        foreach (var item in wishlist.WishlistItems.OrderByDescending(i => i.CreatedAt))
        {
            var discount = await _discountService.ApplyDiscountAsync(new DiscountCalculationContext
            {
                UserId = wishlist.UserId,
                Subtotal = item.Product.BasePrice,
                Items = new List<DiscountCalculationItem>
                {
                    new()
                    {
                        LineNumber = 0,
                        ProductId = item.ProductId,
                        SKU = item.Product.SKU,
                        Quantity = 1,
                        UnitPrice = item.Product.BasePrice,
                        TotalPrice = item.Product.BasePrice
                    }
                }
            });

            response.Items.Add(new WishlistItemResponseDto
            {
                WishlistItemId = item.WishlistItemId,
                ProductId = item.ProductId,
                ProductName = item.Product.Name,
                ProductImageUrl = item.Product.ProductImages.OrderByDescending(img => img.IsPrimary).ThenBy(img => img.DisplayOrder).FirstOrDefault()?.ImageUrl,
                CurrentPrice = item.Product.BasePrice,
                CurrentProductDiscount = discount.ProductDiscountTotal,
                IsAvailable = item.Product.IsActive && item.Product.AvailableQuantity > 0,
                AvailableQuantity = item.Product.AvailableQuantity,
                CreatedAt = item.CreatedAt
            });
        }

        return response;
    }
}
