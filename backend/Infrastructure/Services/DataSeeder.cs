using backend.Constants;
using backend.Data;
using backend.Models;
using backend.Models.Products;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Services;

/// <summary>
/// Service to seed initial data for development/testing
/// </summary>
public class DataSeeder
{
    private readonly AppDbContext _context;
    private readonly ILogger<DataSeeder> _logger;

    public DataSeeder(AppDbContext context, ILogger<DataSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedProductsAsync()
    {
        try
        {
            // Check if we need to seed (less than 20 products)
            var productCount = await _context.Products.CountAsync();
            if (productCount >= 20)
            {
                _logger.LogInformation("Products already exist in database ({Count} products). Skipping seeding.", productCount);
                return;
            }

            // Clear existing products to start fresh
            if (productCount > 0)
            {
                _logger.LogInformation("Clearing {Count} existing products for fresh seeding.", productCount);
                var existingProducts = await _context.Products.ToListAsync();
                _context.Products.RemoveRange(existingProducts);
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation("Starting product seeding...");

            var products = GenerateDummyProducts();
            await _context.Products.AddRangeAsync(products);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully seeded {Count} products.", products.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding products.");
            throw;
        }
    }

    private List<Product> GenerateDummyProducts()
    {
        var products = new List<Product>();
        var now = DateTime.UtcNow;
        var random = new Random(42); // Fixed seed for reproducibility

        var brands = new[] { "Ray-Ban", "Aviator", "Oakley", "Prada", "Gucci", "Versace", "Calvin Klein", "Fossil" };
        var categories = new[] { "Sunglasses", "Reading Glasses", "Fashion", "Sports", "Vintage" };
        var names = new[] 
        { 
            "Classic Black", "Summer Blue", "Golden Hour", "Night Vision", "Crystal Clear",
            "Urban Style", "Beach Vibes", "Mountain Explorer", "City Lights", "Ocean Breeze",
            "Vintage Charm", "Modern Edge", "Retro Wave", "Timeless Elegance", "Bold Statement",
            "Everyday Essential", "Premium Quality", "Limited Edition", "Exclusive Design", "Trendy Pick"
        };

        for (int i = 1; i <= 20; i++)
        {
            var product = new Product
            {
                SKU = $"PRD-{i:D6}",
                Name = names[(i - 1) % names.Length],
                Description = $"Premium eyewear product #{i} with excellent quality and comfort.",
                Brand = brands[random.Next(brands.Length)],
                Category = categories[random.Next(categories.Length)],
                BasePrice = (decimal)(Math.Round(random.Next(99, 999) + random.NextDouble(), 2)),
                AvailableQuantity = random.Next(5, 100),
                SoldQuantity = random.Next(0, 200), // Various sold quantities for popularity sorting
                Priority = (i - 1) % 5, // Priorities 0-4 for testing default sort
                HasPrescription = random.Next(0, 2) == 0,
                IsActive = true,
                CreatedAt = now.AddDays(-(20 - i)), // Spread created dates over last 20 days for "newest" sort
            };
            products.Add(product);
        }

        return products;
    }

    /// <summary>
    /// Seeds a default SUPER_ADMIN user if no users exist in the database.
    /// Only runs in Development. Credentials are logged to console on first run.
    /// </summary>
    public async Task SeedSuperAdminAsync()
    {
        try
        {
            var adminExists = await _context.Users
                .AnyAsync(u => u.UserRole == Roles.SuperAdmin);
            if (adminExists)
            {
                _logger.LogInformation("SuperAdmin already exists. Skipping SuperAdmin seeding.");
                return;
            }

            const string defaultEmail = "superadmin@eyewear.dev";
            const string defaultPassword = "SuperAdmin@2026!";

            var admin = new User
            {
                FirstName = "Super",
                LastName = "Admin",
                Email = defaultEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(defaultPassword),
                UserRole = Roles.SuperAdmin,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(admin);
            await _context.SaveChangesAsync();

            _logger.LogWarning(
                "DEV ONLY: Seeded default SUPER_ADMIN. Email={Email}. Change the default password before using any shared environment.",
                defaultEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the SuperAdmin user.");
            throw;
        }
    }
}

