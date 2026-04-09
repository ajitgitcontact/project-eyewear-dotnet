using backend.Application.Exceptions;
using backend.Application.Abstractions.Products;
using backend.Data;
using backend.DTOs.CustomizationImageDtos;
using backend.Models.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace backend.Infrastructure.Services.Products;

public class CustomizationImageService : ICustomizationImageService
{
    private readonly AppDbContext _context;
    private readonly ILogger<CustomizationImageService> _logger;

    public CustomizationImageService(AppDbContext context, ILogger<CustomizationImageService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<CustomizationImageResponseDto>> GetByProductIdAsync(int productId)
    {
        _logger.LogInformation("Fetching customization images. Input: ProductId={ProductId}", productId);
        var images = await _context.CustomizationImages
            .Where(ci => ci.ProductId == productId)
            .Select(ci => MapToDto(ci))
            .ToListAsync();
        _logger.LogInformation("Fetched customization images. Input: ProductId={ProductId} => Output: Count={Count}", productId, images.Count);
        return images;
    }

    public async Task<CustomizationImageResponseDto> GetByIdAsync(int id)
    {
        _logger.LogInformation("Fetching customization image by id. Input: ImageId={ImageId}", id);
        var image = await _context.CustomizationImages.FindAsync(id);
        if (image is null)
        {
            _logger.LogInformation("Get customization image by id result. Input: ImageId={ImageId} => Output: Found=false", id);
            throw new NotFoundException("Customization image not found.");
        }
        _logger.LogInformation("Get customization image by id result. Input: ImageId={ImageId} => Output: Found=true, ProductId={ProductId}, OptionId={OptionId}, ValueId={ValueId}", id, image.ProductId, image.CustomizationOptionId, image.CustomizationValueId);
        return MapToDto(image);
    }

    public async Task<CustomizationImageResponseDto> CreateAsync(CreateCustomizationImageDto dto)
    {
        _logger.LogInformation("Creating customization image. Input: ProductId={ProductId}, OptionId={OptionId}, ValueId={ValueId}", dto.ProductId, dto.CustomizationOptionId, dto.CustomizationValueId);
        var productExists = await _context.Products.AnyAsync(p => p.ProductId == dto.ProductId);
        if (!productExists)
        {
            _logger.LogError("Create customization image blocked. Product not found. Input: ProductId={ProductId}", dto.ProductId);
            throw new NotFoundException("Product not found.");
        }

        var optionExists = await _context.CustomizationOptions.AnyAsync(co => co.CustomizationOptionId == dto.CustomizationOptionId);
        if (!optionExists)
        {
            _logger.LogError("Create customization image blocked. Option not found. Input: OptionId={OptionId}", dto.CustomizationOptionId);
            throw new NotFoundException("Customization option not found.");
        }

        var valueExists = await _context.CustomizationValues.AnyAsync(cv => cv.CustomizationValueId == dto.CustomizationValueId);
        if (!valueExists)
        {
            _logger.LogError("Create customization image blocked. Value not found. Input: ValueId={ValueId}", dto.CustomizationValueId);
            throw new NotFoundException("Customization value not found.");
        }

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
        _logger.LogInformation("Customization image created. Input: ProductId={ProductId}, OptionId={OptionId}, ValueId={ValueId} => Output: ImageId={ImageId}", dto.ProductId, dto.CustomizationOptionId, dto.CustomizationValueId, image.CustomizationImageId);
        return MapToDto(image);
    }

    public async Task<CustomizationImageResponseDto> UpdateAsync(int id, UpdateCustomizationImageDto dto)
    {
        _logger.LogInformation("Updating customization image. Input: ImageId={ImageId}", id);
        var image = await _context.CustomizationImages.FindAsync(id);
        if (image is null)
        {
            _logger.LogInformation("Update customization image result. Input: ImageId={ImageId} => Output: Found=false", id);
            throw new NotFoundException("Customization image not found.");
        }

        image.ImageUrl = dto.ImageUrl;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Customization image updated. Input: ImageId={ImageId} => Output: ProductId={ProductId}, OptionId={OptionId}, ValueId={ValueId}", id, image.ProductId, image.CustomizationOptionId, image.CustomizationValueId);
        return MapToDto(image);
    }

    public async Task DeleteAsync(int id)
    {
        _logger.LogInformation("Deleting customization image. Input: ImageId={ImageId}", id);
        var image = await _context.CustomizationImages.FindAsync(id);
        if (image is null)
        {
            _logger.LogInformation("Delete customization image result. Input: ImageId={ImageId} => Output: Found=false, Deleted=false", id);
            throw new NotFoundException("Customization image not found.");
        }

        _context.CustomizationImages.Remove(image);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Delete customization image result. Input: ImageId={ImageId}, ProductId={ProductId}, OptionId={OptionId}, ValueId={ValueId} => Output: Deleted=true", id, image.ProductId, image.CustomizationOptionId, image.CustomizationValueId);
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
