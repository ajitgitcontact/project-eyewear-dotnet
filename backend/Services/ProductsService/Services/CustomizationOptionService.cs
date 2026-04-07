using backend.Data;
using backend.DTOs.CustomizationOptionDtos;
using backend.Models.Products;
using backend.Services.ProductsService.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Services.ProductsService.Services;

public class CustomizationOptionService : ICustomizationOptionService
{
    private readonly AppDbContext _context;

    public CustomizationOptionService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CustomizationOptionResponseDto>> GetByProductIdAsync(int productId)
    {
        return await _context.CustomizationOptions
            .Where(co => co.ProductId == productId)
            .OrderBy(co => co.DisplayOrder)
            .Select(co => MapToDto(co))
            .ToListAsync();
    }

    public async Task<CustomizationOptionResponseDto?> GetByIdAsync(int id)
    {
        var option = await _context.CustomizationOptions.FindAsync(id);
        return option is null ? null : MapToDto(option);
    }

    public async Task<CustomizationOptionResponseDto> CreateAsync(CreateCustomizationOptionDto dto)
    {
        var productExists = await _context.Products.AnyAsync(p => p.ProductId == dto.ProductId);
        if (!productExists)
            throw new InvalidOperationException("Product not found.");

        var option = new CustomizationOption
        {
            ProductId = dto.ProductId,
            Name = dto.Name,
            IsRequired = dto.IsRequired,
            DisplayOrder = dto.DisplayOrder,
            CreatedAt = DateTime.UtcNow
        };

        _context.CustomizationOptions.Add(option);
        await _context.SaveChangesAsync();
        return MapToDto(option);
    }

    public async Task<CustomizationOptionResponseDto?> UpdateAsync(int id, UpdateCustomizationOptionDto dto)
    {
        var option = await _context.CustomizationOptions.FindAsync(id);
        if (option is null) return null;

        option.Name = dto.Name;
        option.IsRequired = dto.IsRequired;
        option.DisplayOrder = dto.DisplayOrder;

        await _context.SaveChangesAsync();
        return MapToDto(option);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var option = await _context.CustomizationOptions.FindAsync(id);
        if (option is null) return false;

        _context.CustomizationOptions.Remove(option);
        await _context.SaveChangesAsync();
        return true;
    }

    private static CustomizationOptionResponseDto MapToDto(CustomizationOption option)
    {
        return new CustomizationOptionResponseDto
        {
            CustomizationOptionId = option.CustomizationOptionId,
            ProductId = option.ProductId,
            Name = option.Name,
            IsRequired = option.IsRequired,
            DisplayOrder = option.DisplayOrder,
            CreatedAt = option.CreatedAt
        };
    }
}
