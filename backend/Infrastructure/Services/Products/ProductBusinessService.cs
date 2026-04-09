using backend.Application.Exceptions;
using backend.Application.Abstractions.Products;
using backend.Data;
using backend.DTOs.ProductDtos;
using backend.DTOs.ProductImageDtos;
using backend.Models.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace backend.Infrastructure.Services.Products;

public class ProductBusinessService : IProductBusinessService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ProductBusinessService> _logger;

    public ProductBusinessService(AppDbContext context, ILogger<ProductBusinessService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<FullProductResponseDto> CreateFullProductAsync(CreateFullProductDto dto)
    {
        _logger.LogInformation(
            "Starting full product creation. Input: SKU={Sku}, Name='{Name}', Brand={Brand}, Category={Category}, BasePrice={BasePrice}, InputImages={InputImageCount}, InputOptions={InputOptionCount}",
            dto.SKU, dto.Name, dto.Brand, dto.Category, dto.BasePrice, dto.Images.Count, dto.CustomizationOptions.Count);

        // Validate SKU uniqueness
        var skuExists = await _context.Products.AnyAsync(p => p.SKU == dto.SKU);
        if (skuExists)
        {
            _logger.LogError("Full product creation blocked. SKU already exists. SKU={Sku}", dto.SKU);
            throw new ConflictException("A product with this SKU already exists.");
        }

        // Use a transaction to ensure all-or-nothing
        await using var transaction = await _context.Database.BeginTransactionAsync();
        _logger.LogInformation("Database transaction started for SKU={Sku}", dto.SKU);

        try
        {
            // 1. Create Product
            var product = new Product
            {
                SKU = dto.SKU,
                Name = dto.Name,
                Description = dto.Description,
                Brand = dto.Brand,
                Category = dto.Category,
                BasePrice = dto.BasePrice,
                HasPrescription = dto.HasPrescription,
                CreatedAt = DateTime.UtcNow
            };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Product created in transaction. ProductId={ProductId}, SKU={Sku}", product.ProductId, product.SKU);

            // 2. Create Product Images
            var createdImages = new List<ProductImage>();
            foreach (var imgDto in dto.Images)
            {
                var image = new ProductImage
                {
                    ProductId = product.ProductId,
                    ImageUrl = imgDto.ImageUrl,
                    IsPrimary = imgDto.IsPrimary,
                    DisplayOrder = imgDto.DisplayOrder,
                    CreatedAt = DateTime.UtcNow
                };
                _context.ProductImages.Add(image);
                createdImages.Add(image);
            }
            if (createdImages.Count > 0)
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Product images created. ProductId={ProductId}, Count={ImageCount}", product.ProductId, createdImages.Count);
            }

            // 3. Create Customization Options → Values → Customization Images
            var optionResponses = new List<FullCustomizationOptionResponseDto>();

            foreach (var optDto in dto.CustomizationOptions)
            {
                var option = new CustomizationOption
                {
                    ProductId = product.ProductId,
                    Name = optDto.Name,
                    IsRequired = optDto.IsRequired,
                    DisplayOrder = optDto.DisplayOrder,
                    CreatedAt = DateTime.UtcNow
                };
                _context.CustomizationOptions.Add(option);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Customization option created. ProductId={ProductId}, OptionId={OptionId}, Name={OptionName}",
                    product.ProductId,
                    option.CustomizationOptionId,
                    option.Name);

                var valueResponses = new List<FullCustomizationValueResponseDto>();

                foreach (var valDto in optDto.Values)
                {
                    var value = new CustomizationValue
                    {
                        CustomizationOptionId = option.CustomizationOptionId,
                        Value = valDto.Value,
                        AdditionalPrice = valDto.AdditionalPrice,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.CustomizationValues.Add(value);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Customization value created. OptionId={OptionId}, ValueId={ValueId}",
                        option.CustomizationOptionId,
                        value.CustomizationValueId);

                    var custImageResponses = new List<FullCustomizationImageResponseDto>();

                    foreach (var ciDto in valDto.CustomizationImages)
                    {
                        var custImage = new CustomizationImage
                        {
                            ProductId = product.ProductId,
                            CustomizationOptionId = option.CustomizationOptionId,
                            CustomizationValueId = value.CustomizationValueId,
                            ImageUrl = ciDto.ImageUrl,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.CustomizationImages.Add(custImage);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("Customization image created. ProductId={ProductId}, OptionId={OptionId}, ValueId={ValueId}, ImageId={ImageId}",
                            product.ProductId,
                            option.CustomizationOptionId,
                            value.CustomizationValueId,
                            custImage.CustomizationImageId);

                        custImageResponses.Add(new FullCustomizationImageResponseDto
                        {
                            CustomizationImageId = custImage.CustomizationImageId,
                            ProductId = custImage.ProductId,
                            CustomizationOptionId = custImage.CustomizationOptionId,
                            CustomizationValueId = custImage.CustomizationValueId,
                            ImageUrl = custImage.ImageUrl,
                            CreatedAt = custImage.CreatedAt
                        });
                    }

                    valueResponses.Add(new FullCustomizationValueResponseDto
                    {
                        CustomizationValueId = value.CustomizationValueId,
                        CustomizationOptionId = value.CustomizationOptionId,
                        Value = value.Value,
                        AdditionalPrice = value.AdditionalPrice,
                        CreatedAt = value.CreatedAt,
                        CustomizationImages = custImageResponses
                    });
                }

                optionResponses.Add(new FullCustomizationOptionResponseDto
                {
                    CustomizationOptionId = option.CustomizationOptionId,
                    ProductId = option.ProductId,
                    Name = option.Name,
                    IsRequired = option.IsRequired,
                    DisplayOrder = option.DisplayOrder,
                    CreatedAt = option.CreatedAt,
                    Values = valueResponses
                });
            }

            await transaction.CommitAsync();
            _logger.LogInformation(
                "Full product creation committed. ProductId={ProductId}, SKU={Sku}, Images={ImageCount}, Options={OptionCount}",
                product.ProductId,
                product.SKU,
                createdImages.Count,
                optionResponses.Count);

            return new FullProductResponseDto
            {
                ProductId = product.ProductId,
                SKU = product.SKU,
                Name = product.Name,
                Description = product.Description,
                Brand = product.Brand,
                Category = product.Category,
                BasePrice = product.BasePrice,
                HasPrescription = product.HasPrescription,
                IsActive = product.IsActive,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                Images = createdImages.Select(i => new ProductImageResponseDto
                {
                    ProductImageId = i.ProductImageId,
                    ProductId = i.ProductId,
                    ImageUrl = i.ImageUrl,
                    IsPrimary = i.IsPrimary,
                    DisplayOrder = i.DisplayOrder,
                    CreatedAt = i.CreatedAt
                }).ToList(),
                CustomizationOptions = optionResponses
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Full product creation rolled back. SKU={Sku}", dto.SKU);
            throw;
        }
    }

    public async Task<FullProductResponseDto> GetFullProductByIdAsync(int productId)
    {
        _logger.LogInformation("Fetching full product by id. Input: ProductId={ProductId}", productId);
        var product = await _context.Products
            .Include(p => p.ProductImages.OrderBy(i => i.DisplayOrder))
            .Include(p => p.CustomizationOptions.OrderBy(o => o.DisplayOrder))
                .ThenInclude(o => o.CustomizationValues)
            .Include(p => p.CustomizationImages)
            .FirstOrDefaultAsync(p => p.ProductId == productId);

        if (product is null)
        {
            _logger.LogInformation("Get full product by id result. Input: ProductId={ProductId} => Output: Found=false", productId);
            throw new NotFoundException("Product not found.");
        }

        _logger.LogInformation(
            "Get full product by id result. Input: ProductId={ProductId} => Output: Found=true, SKU={Sku}, Name='{Name}', Images={ImageCount}, Options={OptionCount}",
            productId, product.SKU, product.Name, product.ProductImages.Count, product.CustomizationOptions.Count);

        return MapToFullDto(product);
    }

    public async Task<IEnumerable<FullProductResponseDto>> GetAllFullProductsAsync()
    {
        _logger.LogInformation("Fetching all full products.");
        var products = await _context.Products
            .Include(p => p.ProductImages.OrderBy(i => i.DisplayOrder))
            .Include(p => p.CustomizationOptions.OrderBy(o => o.DisplayOrder))
                .ThenInclude(o => o.CustomizationValues)
            .Include(p => p.CustomizationImages)
            .ToListAsync();

        _logger.LogInformation("Fetched all full products. Output: Count={Count}", products.Count);

        return products.Select(MapToFullDto);
    }

    private static FullProductResponseDto MapToFullDto(Product product)
    {
        // Group customization images by (OptionId, ValueId) for nesting
        var custImageLookup = product.CustomizationImages
            .GroupBy(ci => new { ci.CustomizationOptionId, ci.CustomizationValueId })
            .ToDictionary(
                g => g.Key,
                g => g.Select(ci => new FullCustomizationImageResponseDto
                {
                    CustomizationImageId = ci.CustomizationImageId,
                    ProductId = ci.ProductId,
                    CustomizationOptionId = ci.CustomizationOptionId,
                    CustomizationValueId = ci.CustomizationValueId,
                    ImageUrl = ci.ImageUrl,
                    CreatedAt = ci.CreatedAt
                }).ToList()
            );

        return new FullProductResponseDto
        {
            ProductId = product.ProductId,
            SKU = product.SKU,
            Name = product.Name,
            Description = product.Description,
            Brand = product.Brand,
            Category = product.Category,
            BasePrice = product.BasePrice,
            HasPrescription = product.HasPrescription,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
            Images = product.ProductImages.Select(i => new ProductImageResponseDto
            {
                ProductImageId = i.ProductImageId,
                ProductId = i.ProductId,
                ImageUrl = i.ImageUrl,
                IsPrimary = i.IsPrimary,
                DisplayOrder = i.DisplayOrder,
                CreatedAt = i.CreatedAt
            }).ToList(),
            CustomizationOptions = product.CustomizationOptions.Select(o => new FullCustomizationOptionResponseDto
            {
                CustomizationOptionId = o.CustomizationOptionId,
                ProductId = o.ProductId,
                Name = o.Name,
                IsRequired = o.IsRequired,
                DisplayOrder = o.DisplayOrder,
                CreatedAt = o.CreatedAt,
                Values = o.CustomizationValues.Select(v => new FullCustomizationValueResponseDto
                {
                    CustomizationValueId = v.CustomizationValueId,
                    CustomizationOptionId = v.CustomizationOptionId,
                    Value = v.Value,
                    AdditionalPrice = v.AdditionalPrice,
                    CreatedAt = v.CreatedAt,
                    CustomizationImages = custImageLookup.TryGetValue(
                        new { CustomizationOptionId = o.CustomizationOptionId, CustomizationValueId = v.CustomizationValueId },
                        out var imgs) ? imgs : new()
                }).ToList()
            }).ToList()
        };
    }
}
