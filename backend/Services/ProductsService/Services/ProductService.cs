using backend.Data;
using backend.DTOs.ProductDtos;
using backend.Models.Products;
using backend.Services.ProductsService.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Services.ProductsService.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _context;

    public ProductService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProductResponseDto>> GetAllAsync()
    {
        return await _context.Products
            .Select(p => MapToDto(p))
            .ToListAsync();
    }

    public async Task<ProductResponseDto?> GetByIdAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        return product is null ? null : MapToDto(product);
    }

    public async Task<ProductResponseDto?> GetBySkuAsync(string sku)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.SKU == sku);
        return product is null ? null : MapToDto(product);
    }

    public async Task<ProductResponseDto> CreateAsync(CreateProductDto dto)
    {
        var skuExists = await _context.Products.AnyAsync(p => p.SKU == dto.SKU);
        if (skuExists)
            throw new InvalidOperationException("A product with this SKU already exists.");

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
        return MapToDto(product);
    }

    public async Task<ProductResponseDto?> UpdateAsync(int id, UpdateProductDto dto)
    {
        var product = await _context.Products.FindAsync(id);
        if (product is null) return null;

        if (product.SKU != dto.SKU)
        {
            var skuTaken = await _context.Products.AnyAsync(p => p.SKU == dto.SKU);
            if (skuTaken)
                throw new InvalidOperationException("A product with this SKU already exists.");
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
        return MapToDto(product);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product is null) return false;

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
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
