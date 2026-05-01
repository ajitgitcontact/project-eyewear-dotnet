using backend.Models;
using backend.Models.Orders;
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
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderItemCustomization> OrderItemCustomizations => Set<OrderItemCustomization>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<OrderAddress> OrderAddresses => Set<OrderAddress>();
    public DbSet<OrderStatusLog> OrderStatusLogs => Set<OrderStatusLog>();
    public DbSet<CustomerPrescription> CustomerPrescriptions => Set<CustomerPrescription>();
    public DbSet<OrderNumberSequence> OrderNumberSequences => Set<OrderNumberSequence>();
    public DbSet<Discount> Discounts => Set<Discount>();
    public DbSet<DiscountProduct> DiscountProducts => Set<DiscountProduct>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<CouponUsage> CouponUsages => Set<CouponUsage>();

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

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("Orders", table =>
            {
                table.HasCheckConstraint("CK_Orders_TotalAmount_NonNegative", "\"TotalAmount\" >= 0");
                table.HasCheckConstraint("CK_Orders_OriginalSubtotal_NonNegative", "\"OriginalSubtotal\" >= 0");
                table.HasCheckConstraint("CK_Orders_ProductDiscountTotal_NonNegative", "\"ProductDiscountTotal\" >= 0");
                table.HasCheckConstraint("CK_Orders_CouponDiscountAmount_NonNegative", "\"CouponDiscountAmount\" >= 0");
                table.HasCheckConstraint("CK_Orders_FinalAmount_NonNegative", "\"FinalAmount\" >= 0");
            });

            entity.HasKey(o => o.OrdersId);
            entity.HasIndex(o => o.CustomerOrderId).IsUnique();
            entity.HasIndex(o => o.UserId);

            entity.Property(o => o.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(o => o.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("OrderItems", table =>
            {
                table.HasCheckConstraint("CK_OrderItems_Quantity_Positive", "\"Quantity\" > 0");
                table.HasCheckConstraint("CK_OrderItems_Price_NonNegative", "\"Price\" >= 0");
                table.HasCheckConstraint("CK_OrderItems_TotalPrice_NonNegative", "\"TotalPrice\" >= 0");
                table.HasCheckConstraint("CK_OrderItems_OriginalUnitPrice_NonNegative", "\"OriginalUnitPrice\" >= 0");
                table.HasCheckConstraint("CK_OrderItems_ProductDiscountAmount_NonNegative", "\"ProductDiscountAmount\" >= 0");
                table.HasCheckConstraint("CK_OrderItems_FinalUnitPrice_NonNegative", "\"FinalUnitPrice\" >= 0");
                table.HasCheckConstraint("CK_OrderItems_FinalLineTotal_NonNegative", "\"FinalLineTotal\" >= 0");
            });

            entity.HasKey(oi => oi.OrderItemsId);
            entity.HasIndex(oi => oi.CustomerOrderId);
            entity.HasIndex(oi => oi.ProductId);
            entity.HasIndex(oi => oi.SKU);

            entity.Property(oi => oi.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(oi => oi.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasPrincipalKey(o => o.CustomerOrderId)
                .HasForeignKey(oi => oi.CustomerOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrderItemCustomization>(entity =>
        {
            entity.HasKey(oic => oic.OrderItemCustomizationsId);
            entity.HasIndex(oic => oic.OrderItemId);
            entity.HasIndex(oic => oic.CustomizationOptionId);
            entity.HasIndex(oic => oic.CustomizationValueId);

            entity.Property(oic => oic.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(oic => oic.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(oic => oic.OrderItem)
                .WithMany(oi => oi.Customizations)
                .HasForeignKey(oic => oic.OrderItemId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(oic => oic.CustomizationOption)
                .WithMany()
                .HasForeignKey(oic => oic.CustomizationOptionId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(oic => oic.CustomizationValue)
                .WithMany()
                .HasForeignKey(oic => oic.CustomizationValueId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("Payments", table =>
            {
                table.HasCheckConstraint("CK_Payments_Amount_NonNegative", "\"Amount\" >= 0");
            });

            entity.HasKey(p => p.PaymentsId);
            entity.HasIndex(p => p.CustomerOrderId);

            entity.Property(p => p.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(p => p.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(p => p.Order)
                .WithMany(o => o.Payments)
                .HasPrincipalKey(o => o.CustomerOrderId)
                .HasForeignKey(p => p.CustomerOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderAddress>(entity =>
        {
            entity.HasKey(oa => oa.OrderAddressesId);
            entity.HasIndex(oa => oa.CustomerOrderId);

            entity.Property(oa => oa.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(oa => oa.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(oa => oa.Order)
                .WithMany(o => o.OrderAddresses)
                .HasPrincipalKey(o => o.CustomerOrderId)
                .HasForeignKey(oa => oa.CustomerOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderStatusLog>(entity =>
        {
            entity.HasKey(osl => osl.OrderStatusLogsId);
            entity.HasIndex(osl => osl.CustomerOrderId);

            entity.Property(osl => osl.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(osl => osl.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(osl => osl.Order)
                .WithMany(o => o.OrderStatusLogs)
                .HasPrincipalKey(o => o.CustomerOrderId)
                .HasForeignKey(osl => osl.CustomerOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Discount>(entity =>
        {
            entity.ToTable("Discounts", table =>
            {
                table.HasCheckConstraint("CK_Discounts_DiscountValue_NonNegative", "\"DiscountValue\" >= 0");
            });

            entity.HasKey(d => d.DiscountId);
            entity.HasIndex(d => d.IsActive);
            entity.Property(d => d.DiscountType).HasConversion<string>().HasMaxLength(20);
            entity.Property(d => d.AppliesTo).HasConversion<string>().HasMaxLength(20);
            entity.Property(d => d.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(d => d.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<DiscountProduct>(entity =>
        {
            entity.HasKey(dp => dp.DiscountProductId);
            entity.HasIndex(dp => new { dp.DiscountId, dp.ProductId }).IsUnique();
            entity.HasIndex(dp => dp.ProductId);
            entity.Property(dp => dp.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(dp => dp.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(dp => dp.Discount)
                .WithMany(d => d.DiscountProducts)
                .HasForeignKey(dp => dp.DiscountId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(dp => dp.Product)
                .WithMany()
                .HasForeignKey(dp => dp.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Coupon>(entity =>
        {
            entity.ToTable("Coupons", table =>
            {
                table.HasCheckConstraint("CK_Coupons_CouponValue_NonNegative", "\"CouponValue\" >= 0");
                table.HasCheckConstraint("CK_Coupons_MinimumOrderAmount_NonNegative", "\"MinimumOrderAmount\" IS NULL OR \"MinimumOrderAmount\" >= 0");
                table.HasCheckConstraint("CK_Coupons_MaximumCouponAmount_NonNegative", "\"MaximumCouponAmount\" IS NULL OR \"MaximumCouponAmount\" >= 0");
            });

            entity.HasKey(c => c.CouponId);
            entity.HasIndex(c => c.CouponCode).IsUnique();
            entity.HasIndex(c => c.IsActive);
            entity.Property(c => c.CouponType).HasConversion<string>().HasMaxLength(20);
            entity.Property(c => c.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(c => c.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<CouponUsage>(entity =>
        {
            entity.HasKey(cu => cu.CouponUsageId);
            entity.HasIndex(cu => cu.CouponId);
            entity.HasIndex(cu => cu.UserId);
            entity.HasIndex(cu => cu.CustomerOrderId);
            entity.Property(cu => cu.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(cu => cu.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(cu => cu.Coupon)
                .WithMany(c => c.CouponUsages)
                .HasForeignKey(cu => cu.CouponId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(cu => cu.User)
                .WithMany()
                .HasForeignKey(cu => cu.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(cu => cu.Order)
                .WithMany()
                .HasPrincipalKey(o => o.CustomerOrderId)
                .HasForeignKey(cu => cu.CustomerOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CustomerPrescription>(entity =>
        {
            entity.ToTable("CustomerPrescriptions", table =>
            {
                table.HasCheckConstraint("CK_CustomerPrescriptions_RightAxis_Range", "\"RightAxis\" IS NULL OR (\"RightAxis\" >= 0 AND \"RightAxis\" <= 180)");
                table.HasCheckConstraint("CK_CustomerPrescriptions_LeftAxis_Range", "\"LeftAxis\" IS NULL OR (\"LeftAxis\" >= 0 AND \"LeftAxis\" <= 180)");
                table.HasCheckConstraint("CK_CustomerPrescriptions_PD_NonNegative", "\"PD\" IS NULL OR \"PD\" >= 0");
            });

            entity.HasKey(cp => cp.CustomerPrescriptionsId);
            entity.HasIndex(cp => cp.CustomerOrderId);
            entity.HasIndex(cp => cp.UserId);

            entity.Property(cp => cp.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(cp => cp.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(cp => cp.User)
                .WithMany()
                .HasForeignKey(cp => cp.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(cp => cp.Order)
                .WithMany(o => o.CustomerPrescriptions)
                .HasPrincipalKey(o => o.CustomerOrderId)
                .HasForeignKey(cp => cp.CustomerOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderNumberSequence>(entity =>
        {
            entity.HasKey(ons => ons.OrderNumberSequencesId);
            entity.HasIndex(ons => ons.SequenceDate).IsUnique();

            entity.Property(ons => ons.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(ons => ons.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
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

        var orderEntries = ChangeTracker.Entries<Order>()
            .Where(e => e.State == EntityState.Modified);
        foreach (var entry in orderEntries)
            entry.Entity.UpdatedAt = DateTime.UtcNow;

        var orderItemEntries = ChangeTracker.Entries<OrderItem>()
            .Where(e => e.State == EntityState.Modified);
        foreach (var entry in orderItemEntries)
            entry.Entity.UpdatedAt = DateTime.UtcNow;

        var orderItemCustomizationEntries = ChangeTracker.Entries<OrderItemCustomization>()
            .Where(e => e.State == EntityState.Modified);
        foreach (var entry in orderItemCustomizationEntries)
            entry.Entity.UpdatedAt = DateTime.UtcNow;

        var paymentEntries = ChangeTracker.Entries<Payment>()
            .Where(e => e.State == EntityState.Modified);
        foreach (var entry in paymentEntries)
            entry.Entity.UpdatedAt = DateTime.UtcNow;

        var orderAddressEntries = ChangeTracker.Entries<OrderAddress>()
            .Where(e => e.State == EntityState.Modified);
        foreach (var entry in orderAddressEntries)
            entry.Entity.UpdatedAt = DateTime.UtcNow;

        var orderStatusLogEntries = ChangeTracker.Entries<OrderStatusLog>()
            .Where(e => e.State == EntityState.Modified);
        foreach (var entry in orderStatusLogEntries)
            entry.Entity.UpdatedAt = DateTime.UtcNow;

        var customerPrescriptionEntries = ChangeTracker.Entries<CustomerPrescription>()
            .Where(e => e.State == EntityState.Modified);
        foreach (var entry in customerPrescriptionEntries)
            entry.Entity.UpdatedAt = DateTime.UtcNow;

        var orderNumberSequenceEntries = ChangeTracker.Entries<OrderNumberSequence>()
            .Where(e => e.State == EntityState.Modified);
        foreach (var entry in orderNumberSequenceEntries)
            entry.Entity.UpdatedAt = DateTime.UtcNow;

        var discountEntries = ChangeTracker.Entries<Discount>()
            .Where(e => e.State == EntityState.Modified);
        foreach (var entry in discountEntries)
            entry.Entity.UpdatedAt = DateTime.UtcNow;

        var discountProductEntries = ChangeTracker.Entries<DiscountProduct>()
            .Where(e => e.State == EntityState.Modified);
        foreach (var entry in discountProductEntries)
            entry.Entity.UpdatedAt = DateTime.UtcNow;

        var couponEntries = ChangeTracker.Entries<Coupon>()
            .Where(e => e.State == EntityState.Modified);
        foreach (var entry in couponEntries)
            entry.Entity.UpdatedAt = DateTime.UtcNow;

        var couponUsageEntries = ChangeTracker.Entries<CouponUsage>()
            .Where(e => e.State == EntityState.Modified);
        foreach (var entry in couponUsageEntries)
            entry.Entity.UpdatedAt = DateTime.UtcNow;
    }
}
