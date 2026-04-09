using backend.Data;
using backend.DTOs.ProductImageDtos;
using backend.Models.Products;
using backend.Services.ProductsService.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace backend.Services.ProductsService.Services;

public class ProductImageService : IProductImageService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ProductImageService> _logger;

    public ProductImageService(AppDbContext context, ILogger<ProductImageService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<ProductImageResponseDto>> GetByProductIdAsync(int productId)
    {
        _logger.LogInformation("Fetching product images. Input: ProductId={ProductId}", productId);
        var images = await _context.ProductImages
            .Where(pi => pi.ProductId == productId)
            .OrderBy(pi => pi.DisplayOrder)
            .Select(pi => MapToDto(pi))
            .ToListAsync();
        _logger.LogInformation("Fetched product images. Input: ProductId={ProductId} => Output: Count={Count}", productId, images.Count);
        return images;
    }

    public async Task<ProductImageResponseDto?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Fetching product image by id. Input: ImageId={ImageId}", id);
        var image = await _context.ProductImages.FindAsync(id);
        if (image is null)
        {
            _logger.LogInformation("Get product image by id result. Input: ImageId={ImageId} => Output: Found=false", id);
            return null;
        }
        _logger.LogInformation("Get product image by id result. Input: ImageId={ImageId} => Output: Found=true, ProductId={ProductId}, IsPrimary={IsPrimary}, DisplayOrder={DisplayOrder}", id, image.ProductId, image.IsPrimary, image.DisplayOrder);
        return MapToDto(image);
    }

    public async Task<ProductImageResponseDto> CreateAsync(CreateProductImageDto dto)
    {
        _logger.LogInformation("Creating product image. Input: ProductId={ProductId}, IsPrimary={IsPrimary}, DisplayOrder={DisplayOrder}", dto.ProductId, dto.IsPrimary, dto.DisplayOrder);
        var productExists = await _context.Products.AnyAsync(p => p.ProductId == dto.ProductId);
        if (!productExists)
        {
            _logger.LogError("Create product image blocked. Product not found. Input: ProductId={ProductId}", dto.ProductId);
            throw new InvalidOperationException("Product not found.");
        }

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
        _logger.LogInformation("Product image created. Input: ProductId={ProductId} => Output: ImageId={ImageId}, IsPrimary={IsPrimary}", dto.ProductId, image.ProductImageId, image.IsPrimary);
        return MapToDto(image);
    }

    public async Task<ProductImageResponseDto?> UpdateAsync(int id, UpdateProductImageDto dto)
    {
        _logger.LogInformation("Updating product image. Input: ImageId={ImageId}, IsPrimary={IsPrimary}, DisplayOrder={DisplayOrder}", id, dto.IsPrimary, dto.DisplayOrder);
        var image = await _context.ProductImages.FindAsync(id);
        if (image is null)
        {
            _logger.LogInformation("Update product image result. Input: ImageId={ImageId} => Output: Found=false", id);
            return null;
        }

        image.ImageUrl = dto.ImageUrl;
        image.IsPrimary = dto.IsPrimary;
        image.DisplayOrder = dto.DisplayOrder;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Product image updated. Input: ImageId={ImageId} => Output: IsPrimary={IsPrimary}, DisplayOrder={DisplayOrder}", id, image.IsPrimary, image.DisplayOrder);
        return MapToDto(image);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        _logger.LogInformation("Deleting product image. Input: ImageId={ImageId}", id);
        var image = await _context.ProductImages.FindAsync(id);
        if (image is null)
        {
            _logger.LogInformation("Delete product image result. Input: ImageId={ImageId} => Output: Found=false, Deleted=false", id);
            return false;
        }

        _context.ProductImages.Remove(image);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Delete product image result. Input: ImageId={ImageId}, ProductId={ProductId} => Output: Deleted=true", id, image.ProductId);
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
