using backend.Data;
using backend.DTOs.ProductDtos;
using backend.Models.Products;
using backend.Services.ProductsService.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace backend.Services.ProductsService.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ProductService> _logger;

    public ProductService(AppDbContext context, ILogger<ProductService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<ProductResponseDto>> GetAllAsync()
    {
        _logger.LogInformation("Fetching all products.");
        var products = await _context.Products
            .Select(p => MapToDto(p))
            .ToListAsync();
        _logger.LogInformation("Fetched all products. Output: Count={Count}", products.Count);
        return products;
    }

    public async Task<ProductResponseDto?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Fetching product by id. Input: ProductId={ProductId}", id);
        var product = await _context.Products.FindAsync(id);
        if (product is null)
        {
            _logger.LogInformation("Get product by id result. Input: ProductId={ProductId} => Output: Found=false", id);
            return null;
        }
        _logger.LogInformation("Get product by id result. Input: ProductId={ProductId} => Output: Found=true, SKU={Sku}, Name='{Name}'", id, product.SKU, product.Name);
        return MapToDto(product);
    }

    public async Task<ProductResponseDto?> GetBySkuAsync(string sku)
    {
        _logger.LogInformation("Fetching product by SKU. Input: SKU={Sku}", sku);
        var product = await _context.Products.FirstOrDefaultAsync(p => p.SKU == sku);
        if (product is null)
        {
            _logger.LogInformation("Get product by SKU result. Input: SKU={Sku} => Output: Found=false", sku);
            return null;
        }
        _logger.LogInformation("Get product by SKU result. Input: SKU={Sku} => Output: Found=true, ProductId={ProductId}, Name='{Name}'", sku, product.ProductId, product.Name);
        return MapToDto(product);
    }

    public async Task<ProductResponseDto> CreateAsync(CreateProductDto dto)
    {
        _logger.LogInformation("Creating product. Input: SKU={Sku}, Name='{Name}', Brand={Brand}, Category={Category}, BasePrice={BasePrice}", dto.SKU, dto.Name, dto.Brand, dto.Category, dto.BasePrice);
        var skuExists = await _context.Products.AnyAsync(p => p.SKU == dto.SKU);
        if (skuExists)
        {
            _logger.LogError("Create product blocked. SKU already exists. SKU={Sku}", dto.SKU);
            throw new InvalidOperationException("A product with this SKU already exists.");
        }

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
        _logger.LogInformation("Product created. Input: SKU={Sku} => Output: ProductId={ProductId}, Name='{Name}'", product.SKU, product.ProductId, product.Name);
        return MapToDto(product);
    }

    public async Task<ProductResponseDto?> UpdateAsync(int id, UpdateProductDto dto)
    {
        _logger.LogInformation("Updating product. Input: ProductId={ProductId}, SKU={Sku}, Name='{Name}', IsActive={IsActive}", id, dto.SKU, dto.Name, dto.IsActive);
        var product = await _context.Products.FindAsync(id);
        if (product is null)
        {
            _logger.LogInformation("Update product not found. ProductId={ProductId}", id);
            return null;
        }

        if (product.SKU != dto.SKU)
        {
            var skuTaken = await _context.Products.AnyAsync(p => p.SKU == dto.SKU);
            if (skuTaken)
            {
                _logger.LogError("Update product blocked. SKU already exists. ProductId={ProductId}, SKU={Sku}", id, dto.SKU);
                throw new InvalidOperationException("A product with this SKU already exists.");
            }
        }

        product.SKU = dto.SKU;
        product.Name = dto.Name;
        product.Description = dto.Description;
        product.Brand = dto.Brand;
        product.Category = dto.Category;
        product.BasePrice = dto.BasePrice;
        product.HasPrescription = dto.HasPrescription;
        product.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Product updated. Input: ProductId={ProductId} => Output: SKU={Sku}, Name='{Name}', IsActive={IsActive}", product.ProductId, product.SKU, product.Name, product.IsActive);
        return MapToDto(product);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        _logger.LogInformation("Deleting product. Input: ProductId={ProductId}", id);
        var product = await _context.Products.FindAsync(id);
        if (product is null)
        {
            _logger.LogInformation("Delete product result. Input: ProductId={ProductId} => Output: Found=false, Deleted=false", id);
            return false;
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Delete product result. Input: ProductId={ProductId}, SKU={Sku} => Output: Deleted=true", id, product.SKU);
        return true;
    }

    private static ProductResponseDto MapToDto(Product product)
    {
        return new ProductResponseDto
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
            UpdatedAt = product.UpdatedAt
        };
    }
}
