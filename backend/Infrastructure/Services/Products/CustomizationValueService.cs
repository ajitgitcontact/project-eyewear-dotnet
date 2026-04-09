using backend.Application.Exceptions;
using backend.Application.Abstractions.Products;
using backend.Data;
using backend.DTOs.CustomizationValueDtos;
using backend.Models.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace backend.Infrastructure.Services.Products;

public class CustomizationValueService : ICustomizationValueService
{
    private readonly AppDbContext _context;
    private readonly ILogger<CustomizationValueService> _logger;

    public CustomizationValueService(AppDbContext context, ILogger<CustomizationValueService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<CustomizationValueResponseDto>> GetByOptionIdAsync(int customizationOptionId)
    {
        _logger.LogInformation("Fetching customization values. Input: OptionId={OptionId}", customizationOptionId);
        var values = await _context.CustomizationValues
            .Where(cv => cv.CustomizationOptionId == customizationOptionId)
            .Select(cv => MapToDto(cv))
            .ToListAsync();
        _logger.LogInformation("Fetched customization values. Input: OptionId={OptionId} => Output: Count={Count}", customizationOptionId, values.Count);
        return values;
    }

    public async Task<CustomizationValueResponseDto> GetByIdAsync(int id)
    {
        _logger.LogInformation("Fetching customization value by id. Input: ValueId={ValueId}", id);
        var value = await _context.CustomizationValues.FindAsync(id);
        if (value is null)
        {
            _logger.LogInformation("Get customization value by id result. Input: ValueId={ValueId} => Output: Found=false", id);
            throw new NotFoundException("Customization value not found.");
        }
        _logger.LogInformation("Get customization value by id result. Input: ValueId={ValueId} => Output: Found=true, Value='{Value}', OptionId={OptionId}, AdditionalPrice={AdditionalPrice}", id, value.Value, value.CustomizationOptionId, value.AdditionalPrice);
        return MapToDto(value);
    }

    public async Task<CustomizationValueResponseDto> CreateAsync(CreateCustomizationValueDto dto)
    {
        _logger.LogInformation("Creating customization value. Input: OptionId={OptionId}, Value='{Value}', AdditionalPrice={AdditionalPrice}", dto.CustomizationOptionId, dto.Value, dto.AdditionalPrice);
        var optionExists = await _context.CustomizationOptions.AnyAsync(co => co.CustomizationOptionId == dto.CustomizationOptionId);
        if (!optionExists)
        {
            _logger.LogError("Create customization value blocked. Option not found. Input: OptionId={OptionId}", dto.CustomizationOptionId);
            throw new NotFoundException("Customization option not found.");
        }

        var value = new CustomizationValue
        {
            CustomizationOptionId = dto.CustomizationOptionId,
            Value = dto.Value,
            AdditionalPrice = dto.AdditionalPrice,
            CreatedAt = DateTime.UtcNow
        };

        _context.CustomizationValues.Add(value);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Customization value created. Input: OptionId={OptionId}, Value='{Value}' => Output: ValueId={ValueId}", dto.CustomizationOptionId, dto.Value, value.CustomizationValueId);
        return MapToDto(value);
    }

    public async Task<CustomizationValueResponseDto> UpdateAsync(int id, UpdateCustomizationValueDto dto)
    {
        _logger.LogInformation("Updating customization value. Input: ValueId={ValueId}, Value='{Value}', AdditionalPrice={AdditionalPrice}", id, dto.Value, dto.AdditionalPrice);
        var value = await _context.CustomizationValues.FindAsync(id);
        if (value is null)
        {
            _logger.LogInformation("Update customization value result. Input: ValueId={ValueId} => Output: Found=false", id);
            throw new NotFoundException("Customization value not found.");
        }

        value.Value = dto.Value;
        value.AdditionalPrice = dto.AdditionalPrice;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Customization value updated. Input: ValueId={ValueId} => Output: Value='{Value}', AdditionalPrice={AdditionalPrice}", id, value.Value, value.AdditionalPrice);
        return MapToDto(value);
    }

    public async Task DeleteAsync(int id)
    {
        _logger.LogInformation("Deleting customization value. Input: ValueId={ValueId}", id);
        var value = await _context.CustomizationValues.FindAsync(id);
        if (value is null)
        {
            _logger.LogInformation("Delete customization value result. Input: ValueId={ValueId} => Output: Found=false, Deleted=false", id);
            throw new NotFoundException("Customization value not found.");
        }

        _context.CustomizationValues.Remove(value);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Delete customization value result. Input: ValueId={ValueId}, Value='{Value}' => Output: Deleted=true", id, value.Value);
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
