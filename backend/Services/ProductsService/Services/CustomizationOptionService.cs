using backend.Data;
using backend.DTOs.CustomizationOptionDtos;
using backend.Models.Products;
using backend.Services.ProductsService.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace backend.Services.ProductsService.Services;

public class CustomizationOptionService : ICustomizationOptionService
{
    private readonly AppDbContext _context;
    private readonly ILogger<CustomizationOptionService> _logger;

    public CustomizationOptionService(AppDbContext context, ILogger<CustomizationOptionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<CustomizationOptionResponseDto>> GetByProductIdAsync(int productId)
    {
        _logger.LogInformation("Fetching customization options. Input: ProductId={ProductId}", productId);
        var options = await _context.CustomizationOptions
            .Where(co => co.ProductId == productId)
            .OrderBy(co => co.DisplayOrder)
            .Select(co => MapToDto(co))
            .ToListAsync();
        _logger.LogInformation("Fetched customization options. Input: ProductId={ProductId} => Output: Count={Count}", productId, options.Count);
        return options;
    }

    public async Task<CustomizationOptionResponseDto?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Fetching customization option by id. Input: OptionId={OptionId}", id);
        var option = await _context.CustomizationOptions.FindAsync(id);
        if (option is null)
        {
            _logger.LogInformation("Get customization option by id result. Input: OptionId={OptionId} => Output: Found=false", id);
            return null;
        }
        _logger.LogInformation("Get customization option by id result. Input: OptionId={OptionId} => Output: Found=true, Name='{Name}', ProductId={ProductId}", id, option.Name, option.ProductId);
        return MapToDto(option);
    }

    public async Task<CustomizationOptionResponseDto> CreateAsync(CreateCustomizationOptionDto dto)
    {
        _logger.LogInformation("Creating customization option. Input: ProductId={ProductId}, Name='{Name}', IsRequired={IsRequired}, DisplayOrder={DisplayOrder}", dto.ProductId, dto.Name, dto.IsRequired, dto.DisplayOrder);
        var productExists = await _context.Products.AnyAsync(p => p.ProductId == dto.ProductId);
        if (!productExists)
        {
            _logger.LogError("Create customization option blocked. Product not found. Input: ProductId={ProductId}", dto.ProductId);
            throw new InvalidOperationException("Product not found.");
        }

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
        _logger.LogInformation("Customization option created. Input: ProductId={ProductId}, Name='{Name}' => Output: OptionId={OptionId}", dto.ProductId, dto.Name, option.CustomizationOptionId);
        return MapToDto(option);
    }

    public async Task<CustomizationOptionResponseDto?> UpdateAsync(int id, UpdateCustomizationOptionDto dto)
    {
        _logger.LogInformation("Updating customization option. Input: OptionId={OptionId}, Name='{Name}', IsRequired={IsRequired}, DisplayOrder={DisplayOrder}", id, dto.Name, dto.IsRequired, dto.DisplayOrder);
        var option = await _context.CustomizationOptions.FindAsync(id);
        if (option is null)
        {
            _logger.LogInformation("Update customization option result. Input: OptionId={OptionId} => Output: Found=false", id);
            return null;
        }

        option.Name = dto.Name;
        option.IsRequired = dto.IsRequired;
        option.DisplayOrder = dto.DisplayOrder;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Customization option updated. Input: OptionId={OptionId} => Output: Name='{Name}', IsRequired={IsRequired}", id, option.Name, option.IsRequired);
        return MapToDto(option);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        _logger.LogInformation("Deleting customization option. Input: OptionId={OptionId}", id);
        var option = await _context.CustomizationOptions.FindAsync(id);
        if (option is null)
        {
            _logger.LogInformation("Delete customization option result. Input: OptionId={OptionId} => Output: Found=false, Deleted=false", id);
            return false;
        }

        _context.CustomizationOptions.Remove(option);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Delete customization option result. Input: OptionId={OptionId}, Name='{Name}' => Output: Deleted=true", id, option.Name);
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
