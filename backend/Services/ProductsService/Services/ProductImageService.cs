using backend.Data;
using backend.DTOs.ProductImageDtos;
using backend.Models.Products;
using backend.Services.ProductsService.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Services.ProductsService.Services;

public class ProductImageService : IProductImageService
{
    private readonly AppDbContext _context;

    public ProductImageService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProductImageResponseDto>> GetByProductIdAsync(int productId)
    {
        return await _context.ProductImages
            .Where(pi => pi.ProductId == productId)
            .OrderBy(pi => pi.DisplayOrder)
            .Select(pi => MapToDto(pi))
            .ToListAsync();
    }

    public async Task<ProductImageResponseDto?> GetByIdAsync(int id)
    {
        var image = await _context.ProductImages.FindAsync(id);
        return image is null ? null : MapToDto(image);
    }

    public async Task<ProductImageResponseDto> CreateAsync(CreateProductImageDto dto)
    {
        var productExists = await _context.Products.AnyAsync(p => p.ProductId == dto.ProductId);
        if (!productExists)
            throw new InvalidOperationException("Product not found.");

        var image = new ProductImage
        {
            ProductId = dto.ProductId,
            ImageUrl = dto.ImageUrl,
            IsPrimary = dto.IsPrimary,
            DisplayOrder = dto.DisplayOrder,
            CreatedAt = DateTime.UtcNow
        };

        _context.ProductImages.Add(image);
        await _context.SaveChangesAsync();
        return MapToDto(image);
    }

    public async Task<ProductImageResponseDto?> UpdateAsync(int id, UpdateProductImageDto dto)
    {
        var image = await _context.ProductImages.FindAsync(id);
        if (image is null) return null;

        image.ImageUrl = dto.ImageUrl;
        image.IsPrimary = dto.IsPrimary;
        image.DisplayOrder = dto.DisplayOrder;

        await _context.SaveChangesAsync();
        return MapToDto(image);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var image = await _context.ProductImages.FindAsync(id);
        if (image is null) return false;

        _context.ProductImages.Remove(image);
        await _context.SaveChangesAsync();
        return true;
    }

    private static ProductImageResponseDto MapToDto(ProductImage image)
    {
        return new ProductImageResponseDto
        {
            ProductImageId = image.ProductImageId,
            ProductId = image.ProductId,
            ImageUrl = image.ImageUrl,
            IsPrimary = image.IsPrimary,
            DisplayOrder = image.DisplayOrder,
            CreatedAt = image.CreatedAt
        };
    }
}
