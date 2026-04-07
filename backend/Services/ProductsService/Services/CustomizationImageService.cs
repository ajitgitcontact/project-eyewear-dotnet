using backend.Data;
using backend.DTOs.CustomizationImageDtos;
using backend.Models.Products;
using backend.Services.ProductsService.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Services.ProductsService.Services;

public class CustomizationImageService : ICustomizationImageService
{
    private readonly AppDbContext _context;

    public CustomizationImageService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CustomizationImageResponseDto>> GetByProductIdAsync(int productId)
    {
        return await _context.CustomizationImages
            .Where(ci => ci.ProductId == productId)
            .Select(ci => MapToDto(ci))
            .ToListAsync();
    }

    public async Task<CustomizationImageResponseDto?> GetByIdAsync(int id)
    {
        var image = await _context.CustomizationImages.FindAsync(id);
        return image is null ? null : MapToDto(image);
    }

    public async Task<CustomizationImageResponseDto> CreateAsync(CreateCustomizationImageDto dto)
    {
        var productExists = await _context.Products.AnyAsync(p => p.ProductId == dto.ProductId);
        if (!productExists)
            throw new InvalidOperationException("Product not found.");

        var optionExists = await _context.CustomizationOptions.AnyAsync(co => co.CustomizationOptionId == dto.CustomizationOptionId);
        if (!optionExists)
            throw new InvalidOperationException("Customization option not found.");

        var valueExists = await _context.CustomizationValues.AnyAsync(cv => cv.CustomizationValueId == dto.CustomizationValueId);
        if (!valueExists)
            throw new InvalidOperationException("Customization value not found.");

        var image = new CustomizationImage
        {
            ProductId = dto.ProductId,
            CustomizationOptionId = dto.CustomizationOptionId,
            CustomizationValueId = dto.CustomizationValueId,
            ImageUrl = dto.ImageUrl,
            CreatedAt = DateTime.UtcNow
        };

        _context.CustomizationImages.Add(image);
        await _context.SaveChangesAsync();
        return MapToDto(image);
    }

    public async Task<CustomizationImageResponseDto?> UpdateAsync(int id, UpdateCustomizationImageDto dto)
    {
        var image = await _context.CustomizationImages.FindAsync(id);
        if (image is null) return null;

        image.ImageUrl = dto.ImageUrl;

        await _context.SaveChangesAsync();
        return MapToDto(image);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var image = await _context.CustomizationImages.FindAsync(id);
        if (image is null) return false;

        _context.CustomizationImages.Remove(image);
        await _context.SaveChangesAsync();
        return true;
    }

    private static CustomizationImageResponseDto MapToDto(CustomizationImage image)
    {
        return new CustomizationImageResponseDto
        {
            CustomizationImageId = image.CustomizationImageId,
            ProductId = image.ProductId,
            CustomizationOptionId = image.CustomizationOptionId,
            CustomizationValueId = image.CustomizationValueId,
            ImageUrl = image.ImageUrl,
            CreatedAt = image.CreatedAt
        };
    }
}
