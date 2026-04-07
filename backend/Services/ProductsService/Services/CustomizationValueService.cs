using backend.Data;
using backend.DTOs.CustomizationValueDtos;
using backend.Models.Products;
using backend.Services.ProductsService.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Services.ProductsService.Services;

public class CustomizationValueService : ICustomizationValueService
{
    private readonly AppDbContext _context;

    public CustomizationValueService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CustomizationValueResponseDto>> GetByOptionIdAsync(int customizationOptionId)
    {
        return await _context.CustomizationValues
            .Where(cv => cv.CustomizationOptionId == customizationOptionId)
            .Select(cv => MapToDto(cv))
            .ToListAsync();
    }

    public async Task<CustomizationValueResponseDto?> GetByIdAsync(int id)
    {
        var value = await _context.CustomizationValues.FindAsync(id);
        return value is null ? null : MapToDto(value);
    }

    public async Task<CustomizationValueResponseDto> CreateAsync(CreateCustomizationValueDto dto)
    {
        var optionExists = await _context.CustomizationOptions.AnyAsync(co => co.CustomizationOptionId == dto.CustomizationOptionId);
        if (!optionExists)
            throw new InvalidOperationException("Customization option not found.");

        var value = new CustomizationValue
        {
            CustomizationOptionId = dto.CustomizationOptionId,
            Value = dto.Value,
            AdditionalPrice = dto.AdditionalPrice,
            CreatedAt = DateTime.UtcNow
        };

        _context.CustomizationValues.Add(value);
        await _context.SaveChangesAsync();
        return MapToDto(value);
    }

    public async Task<CustomizationValueResponseDto?> UpdateAsync(int id, UpdateCustomizationValueDto dto)
    {
        var value = await _context.CustomizationValues.FindAsync(id);
        if (value is null) return null;

        value.Value = dto.Value;
        value.AdditionalPrice = dto.AdditionalPrice;

        await _context.SaveChangesAsync();
        return MapToDto(value);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var value = await _context.CustomizationValues.FindAsync(id);
        if (value is null) return false;

        _context.CustomizationValues.Remove(value);
        await _context.SaveChangesAsync();
        return true;
    }

    private static CustomizationValueResponseDto MapToDto(CustomizationValue value)
    {
        return new CustomizationValueResponseDto
        {
            CustomizationValueId = value.CustomizationValueId,
            CustomizationOptionId = value.CustomizationOptionId,
            Value = value.Value,
            AdditionalPrice = value.AdditionalPrice,
            CreatedAt = value.CreatedAt
        };
    }
}
