using backend.DTOs.ProductDtos;
using backend.DTOs.CustomizationOptionDtos;
using backend.DTOs.CustomizationValueDtos;
using backend.DTOs.ProductImageDtos;
using backend.DTOs.CustomizationImageDtos;
using backend.Services.ProductsService.Interfaces;

namespace backend.Tests;

public static class ServiceTester
{
    public static async Task RunAllTestsAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;

        var productService = sp.GetRequiredService<IProductService>();
        var optionService = sp.GetRequiredService<ICustomizationOptionService>();
        var valueService = sp.GetRequiredService<ICustomizationValueService>();
        var imageService = sp.GetRequiredService<IProductImageService>();
        var custImageService = sp.GetRequiredService<ICustomizationImageService>();

        int passed = 0, failed = 0;

        void Assert(bool condition, string testName)
        {
            if (condition)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"  PASS: {testName}");
                passed++;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  FAIL: {testName}");
                failed++;
            }
            Console.ResetColor();
        }

        Console.WriteLine("\n========================================");
        Console.WriteLine("  SERVICE TESTER - ALL CRUD OPERATIONS");
        Console.WriteLine("========================================\n");

        // ─── 1. PRODUCT SERVICE ─────────────────────────────────
        Console.WriteLine("--- Product Service ---");

        // CREATE
        var product = await productService.CreateAsync(new CreateProductDto
        {
            SKU = "TEST-SKU-001",
            Name = "Test Aviator Sunglasses",
            Description = "Premium test aviator sunglasses",
            Brand = "TestBrand",
            Category = "Sunglasses",
            BasePrice = 149.99m,
            HasPrescription = false
        });
        Assert(product != null && product.ProductId > 0, "Product CREATE");
        Console.WriteLine($"    Created ProductId: {product.ProductId}");

        // READ by ID
        var productById = await productService.GetByIdAsync(product.ProductId);
        Assert(productById != null && productById.Name == "Test Aviator Sunglasses", "Product GET by ID");

        // READ by SKU
        var productBySku = await productService.GetBySkuAsync("TEST-SKU-001");
        Assert(productBySku != null && productBySku.SKU == "TEST-SKU-001", "Product GET by SKU");

        // READ all
        var allProducts = await productService.GetAllAsync();
        Assert(allProducts.Any(p => p.ProductId == product.ProductId), "Product GET ALL contains test product");

        // UPDATE
        var updatedProduct = await productService.UpdateAsync(product.ProductId, new UpdateProductDto
        {
            SKU = "TEST-SKU-001-UPD",
            Name = "Updated Aviator Sunglasses",
            Description = "Updated description",
            Brand = "UpdatedBrand",
            Category = "Sunglasses",
            BasePrice = 199.99m,
            HasPrescription = true,
            IsActive = true
        });
        Assert(updatedProduct != null && updatedProduct.Name == "Updated Aviator Sunglasses" && updatedProduct.BasePrice == 199.99m, "Product UPDATE");

        // ─── 2. CUSTOMIZATION OPTION SERVICE ────────────────────
        Console.WriteLine("\n--- CustomizationOption Service ---");

        // CREATE
        var option = await optionService.CreateAsync(new CreateCustomizationOptionDto
        {
            ProductId = product.ProductId,
            Name = "Lens Color",
            IsRequired = true,
            DisplayOrder = 1
        });
        Assert(option != null && option.CustomizationOptionId > 0, "CustomizationOption CREATE");
        Console.WriteLine($"    Created CustomizationOptionId: {option.CustomizationOptionId}");

        // READ by ID
        var optionById = await optionService.GetByIdAsync(option.CustomizationOptionId);
        Assert(optionById != null && optionById.Name == "Lens Color", "CustomizationOption GET by ID");

        // READ by ProductId
        var optionsByProduct = await optionService.GetByProductIdAsync(product.ProductId);
        Assert(optionsByProduct.Any(o => o.CustomizationOptionId == option.CustomizationOptionId), "CustomizationOption GET by ProductId");

        // UPDATE
        var updatedOption = await optionService.UpdateAsync(option.CustomizationOptionId, new UpdateCustomizationOptionDto
        {
            Name = "Updated Lens Color",
            IsRequired = false,
            DisplayOrder = 2
        });
        Assert(updatedOption != null && updatedOption.Name == "Updated Lens Color" && updatedOption.DisplayOrder == 2, "CustomizationOption UPDATE");

        // ─── 3. CUSTOMIZATION VALUE SERVICE ─────────────────────
        Console.WriteLine("\n--- CustomizationValue Service ---");

        // CREATE
        var value = await valueService.CreateAsync(new CreateCustomizationValueDto
        {
            CustomizationOptionId = option.CustomizationOptionId,
            Value = "Blue Tint",
            AdditionalPrice = 25.00m
        });
        Assert(value != null && value.CustomizationValueId > 0, "CustomizationValue CREATE");
        Console.WriteLine($"    Created CustomizationValueId: {value.CustomizationValueId}");

        // READ by ID
        var valueById = await valueService.GetByIdAsync(value.CustomizationValueId);
        Assert(valueById != null && valueById.Value == "Blue Tint", "CustomizationValue GET by ID");

        // READ by OptionId
        var valuesByOption = await valueService.GetByOptionIdAsync(option.CustomizationOptionId);
        Assert(valuesByOption.Any(v => v.CustomizationValueId == value.CustomizationValueId), "CustomizationValue GET by OptionId");

        // UPDATE
        var updatedValue = await valueService.UpdateAsync(value.CustomizationValueId, new UpdateCustomizationValueDto
        {
            Value = "Green Tint",
            AdditionalPrice = 30.00m
        });
        Assert(updatedValue != null && updatedValue.Value == "Green Tint" && updatedValue.AdditionalPrice == 30.00m, "CustomizationValue UPDATE");

        // ─── 4. PRODUCT IMAGE SERVICE ───────────────────────────
        Console.WriteLine("\n--- ProductImage Service ---");

        // CREATE
        var image = await imageService.CreateAsync(new CreateProductImageDto
        {
            ProductId = product.ProductId,
            ImageUrl = "/images/products/test-aviator-front.jpg",
            IsPrimary = true,
            DisplayOrder = 1
        });
        Assert(image != null && image.ProductImageId > 0, "ProductImage CREATE");
        Console.WriteLine($"    Created ProductImageId: {image.ProductImageId}");

        // READ by ID
        var imageById = await imageService.GetByIdAsync(image.ProductImageId);
        Assert(imageById != null && imageById.ImageUrl == "/images/products/test-aviator-front.jpg", "ProductImage GET by ID");

        // READ by ProductId
        var imagesByProduct = await imageService.GetByProductIdAsync(product.ProductId);
        Assert(imagesByProduct.Any(i => i.ProductImageId == image.ProductImageId), "ProductImage GET by ProductId");

        // UPDATE
        var updatedImage = await imageService.UpdateAsync(image.ProductImageId, new UpdateProductImageDto
        {
            ImageUrl = "/images/products/test-aviator-side.jpg",
            IsPrimary = false,
            DisplayOrder = 2
        });
        Assert(updatedImage != null && updatedImage.ImageUrl == "/images/products/test-aviator-side.jpg" && !updatedImage.IsPrimary, "ProductImage UPDATE");

        // ─── 5. CUSTOMIZATION IMAGE SERVICE ─────────────────────
        Console.WriteLine("\n--- CustomizationImage Service ---");

        // CREATE
        var custImage = await custImageService.CreateAsync(new CreateCustomizationImageDto
        {
            ProductId = product.ProductId,
            CustomizationOptionId = option.CustomizationOptionId,
            CustomizationValueId = value.CustomizationValueId,
            ImageUrl = "/images/products/test-aviator-blue-tint.jpg"
        });
        Assert(custImage != null && custImage.CustomizationImageId > 0, "CustomizationImage CREATE");
        Console.WriteLine($"    Created CustomizationImageId: {custImage.CustomizationImageId}");

        // READ by ID
        var custImageById = await custImageService.GetByIdAsync(custImage.CustomizationImageId);
        Assert(custImageById != null && custImageById.ImageUrl == "/images/products/test-aviator-blue-tint.jpg", "CustomizationImage GET by ID");

        // READ by ProductId
        var custImagesByProduct = await custImageService.GetByProductIdAsync(product.ProductId);
        Assert(custImagesByProduct.Any(ci => ci.CustomizationImageId == custImage.CustomizationImageId), "CustomizationImage GET by ProductId");

        // UPDATE
        var updatedCustImage = await custImageService.UpdateAsync(custImage.CustomizationImageId, new UpdateCustomizationImageDto
        {
            ImageUrl = "/images/products/test-aviator-green-tint.jpg"
        });
        Assert(updatedCustImage != null && updatedCustImage.ImageUrl == "/images/products/test-aviator-green-tint.jpg", "CustomizationImage UPDATE");

        // ─── DELETE ALL (reverse order to respect FK constraints) ─
        Console.WriteLine("\n--- DELETE Operations (reverse FK order) ---");

        var delCustImage = await custImageService.DeleteAsync(custImage.CustomizationImageId);
        Assert(delCustImage, "CustomizationImage DELETE");

        var delImage = await imageService.DeleteAsync(image.ProductImageId);
        Assert(delImage, "ProductImage DELETE");

        var delValue = await valueService.DeleteAsync(value.CustomizationValueId);
        Assert(delValue, "CustomizationValue DELETE");

        var delOption = await optionService.DeleteAsync(option.CustomizationOptionId);
        Assert(delOption, "CustomizationOption DELETE");

        var delProduct = await productService.DeleteAsync(product.ProductId);
        Assert(delProduct, "Product DELETE");

        // Verify deletes
        var verifyProduct = await productService.GetByIdAsync(product.ProductId);
        Assert(verifyProduct == null, "Product verified deleted (GET returns null)");

        var verifyOption = await optionService.GetByIdAsync(option.CustomizationOptionId);
        Assert(verifyOption == null, "CustomizationOption verified deleted");

        var verifyValue = await valueService.GetByIdAsync(value.CustomizationValueId);
        Assert(verifyValue == null, "CustomizationValue verified deleted");

        var verifyImage = await imageService.GetByIdAsync(image.ProductImageId);
        Assert(verifyImage == null, "ProductImage verified deleted");

        var verifyCustImage = await custImageService.GetByIdAsync(custImage.CustomizationImageId);
        Assert(verifyCustImage == null, "CustomizationImage verified deleted");

        // ─── SUMMARY ────────────────────────────────────────────
        Console.WriteLine("\n========================================");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"  RESULTS: {passed} passed, {failed} failed, {passed + failed} total");
        Console.ResetColor();
        Console.WriteLine("========================================\n");
    }
}
