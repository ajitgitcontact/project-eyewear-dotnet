using backend.Models;
using backend.Models.Products;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<CustomizationOption> CustomizationOptions => Set<CustomizationOption>();
    public DbSet<CustomizationValue> CustomizationValues => Set<CustomizationValue>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<CustomizationImage> CustomizationImages => Set<CustomizationImage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
        });

        // Product
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(p => p.SKU).IsUnique();
        });

        // CustomizationImage - restrict cascades to avoid multiple cascade paths
        modelBuilder.Entity<CustomizationImage>(entity =>
        {
            entity.HasOne(ci => ci.Product)
                .WithMany(p => p.CustomizationImages)
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ci => ci.CustomizationOption)
                .WithMany()
                .HasForeignKey(ci => ci.CustomizationOptionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(ci => ci.CustomizationValue)
                .WithMany()
                .HasForeignKey(ci => ci.CustomizationValueId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    public override int SaveChanges()
    {
        SetUpdatedAt();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetUpdatedAt();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void SetUpdatedAt()
    {
        var userEntries = ChangeTracker.Entries<User>()
            .Where(e => e.State == EntityState.Modified);
        foreach (var entry in userEntries)
            entry.Entity.UpdatedAt = DateTime.UtcNow;

        var productEntries = ChangeTracker.Entries<Product>()
            .Where(e => e.State == EntityState.Modified);
        foreach (var entry in productEntries)
            entry.Entity.UpdatedAt = DateTime.UtcNow;
    }
}
