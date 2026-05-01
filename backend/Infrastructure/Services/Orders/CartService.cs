using backend.Application.Abstractions.Orders;
using backend.Application.Exceptions;
using backend.Data;
using backend.DTOs.CartDtos;
using backend.DTOs.OrderCreationDtos;
using backend.DTOs.OrderStatusLogDtos;
using backend.Models.Carts;
using backend.Models.Orders;
using backend.Models.Products;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Services.Orders;

public class CartService : ICartService
{
    private readonly AppDbContext _context;
    private readonly IDiscountService _discountService;
    private readonly IOrderCreationService _orderCreationService;
    private readonly IOrderStatusLogService _orderStatusLogService;
    private readonly ILogger<CartService> _logger;

    public CartService(
        AppDbContext context,
        IDiscountService discountService,
        IOrderCreationService orderCreationService,
        IOrderStatusLogService orderStatusLogService,
        ILogger<CartService> logger)
    {
        _context = context;
        _discountService = discountService;
        _orderCreationService = orderCreationService;
        _orderStatusLogService = orderStatusLogService;
        _logger = logger;
    }

    public async Task<CartResponseDto> GetActiveCartAsync(int userId)
    {
        var cart = await LoadActiveCartAsync(userId);
        if (cart is null)
            return new CartResponseDto();

        await RecalculateCartPreviewAsync(cart);
        return MapCart(cart);
    }

    public async Task<CartResponseDto> AddItemAsync(int userId, AddCartItemDto dto)
    {
        _logger.LogInformation("Add to cart started. UserId={UserId}, ProductId={ProductId}, Quantity={Quantity}", userId, dto.ProductId, dto.Quantity);
        var cart = await GetOrCreateActiveCartAsync(userId);
        EnsureActive(cart);

        var product = await LoadProductAsync(dto.ProductId);
        ValidateProductForCart(product, dto.Quantity);
        var preparedCustomizations = PrepareCustomizations(product, dto.Customizations);
        var prescription = await PreparePrescriptionAsync(userId, product, dto.CustomerPrescriptionsId, dto.Prescription);

        var signature = BuildSignature(dto.ProductId, preparedCustomizations, prescription);
        var existing = cart.CartItems.FirstOrDefault(i => i.IsActive && BuildSignature(i) == signature);
        if (existing is not null)
        {
            if (product.AvailableQuantity < existing.Quantity + dto.Quantity)
                throw new BadRequestException("Insufficient product stock.");

            existing.Quantity += dto.Quantity;
            _logger.LogInformation("Existing cart item quantity increased. UserId={UserId}, CartItemId={CartItemId}, Quantity={Quantity}", userId, existing.CartItemId, existing.Quantity);
        }
        else
        {
            var cartItem = new CartItem
            {
                CartId = cart.CartId,
                ProductId = product.ProductId,
                Product = product,
                SKU = product.SKU,
                Quantity = dto.Quantity,
                UnitPrice = product.BasePrice + preparedCustomizations.Sum(c => c.ExtraPrice),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            foreach (var customization in preparedCustomizations)
                cartItem.Customizations.Add(customization);

            if (prescription is not null)
                cartItem.Prescription = prescription;

            cart.CartItems.Add(cartItem);
            _context.CartItems.Add(cartItem);
        }

        await _context.SaveChangesAsync();
        await RecalculateCartPreviewAsync(cart);
        _logger.LogInformation("Add to cart completed. UserId={UserId}, CartId={CartId}", userId, cart.CartId);
        return MapCart(cart);
    }

    public async Task<CartResponseDto> UpdateItemQuantityAsync(int userId, string cartItemId, UpdateCartItemQuantityDto dto)
    {
        var cart = await LoadActiveCartAsync(userId) ?? throw new NotFoundException("Active cart not found.");
        EnsureActive(cart);
        var item = cart.CartItems.FirstOrDefault(i => i.CartItemId == cartItemId && i.IsActive) ?? throw new NotFoundException("Cart item not found.");

        if (item.Product.AvailableQuantity < dto.Quantity)
            throw new BadRequestException("Insufficient product stock.");

        item.Quantity = dto.Quantity;
        await _context.SaveChangesAsync();
        await RecalculateCartPreviewAsync(cart);
        _logger.LogInformation("Cart item quantity updated. UserId={UserId}, CartItemId={CartItemId}, Quantity={Quantity}", userId, cartItemId, dto.Quantity);
        return MapCart(cart);
    }

    public async Task<CartResponseDto> RemoveItemAsync(int userId, string cartItemId)
    {
        var cart = await LoadActiveCartAsync(userId) ?? throw new NotFoundException("Active cart not found.");
        EnsureActive(cart);
        var item = cart.CartItems.FirstOrDefault(i => i.CartItemId == cartItemId && i.IsActive) ?? throw new NotFoundException("Cart item not found.");
        item.IsActive = false;
        await _context.SaveChangesAsync();
        await RecalculateCartPreviewAsync(cart);
        _logger.LogInformation("Cart item removed. UserId={UserId}, CartItemId={CartItemId}", userId, cartItemId);
        return MapCart(cart);
    }

    public async Task<CartResponseDto> ClearAsync(int userId)
    {
        var cart = await LoadActiveCartAsync(userId);
        if (cart is null)
            return new CartResponseDto();

        EnsureActive(cart);
        foreach (var item in cart.CartItems.Where(i => i.IsActive))
            item.IsActive = false;

        _context.CartCoupons.RemoveRange(cart.CartCoupons);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Cart cleared. UserId={UserId}, CartId={CartId}", userId, cart.CartId);
        return MapCart(cart);
    }

    public async Task<CartResponseDto> ApplyCouponAsync(int userId, ApplyCartCouponDto dto)
    {
        var cart = await LoadActiveCartAsync(userId) ?? throw new BadRequestException("Active cart is empty.");
        EnsureActive(cart);
        if (!cart.CartItems.Any(i => i.IsActive))
            throw new BadRequestException("Cart is empty.");

        dto.CouponCode = dto.CouponCode.Trim().ToUpperInvariant();
        _context.CartCoupons.RemoveRange(cart.CartCoupons);
        cart.CartCoupons.Clear();

        var discount = await CalculateCartDiscountAsync(cart, dto.CouponCode);
        if (string.IsNullOrWhiteSpace(discount.CouponId) || string.IsNullOrWhiteSpace(discount.CouponCode))
            throw new BadRequestException("Invalid coupon code.");

        var coupon = new CartCoupon
        {
            CartId = cart.CartId,
            CouponId = discount.CouponId,
            CouponCode = discount.CouponCode,
            CouponDiscountAmount = discount.CouponDiscountAmount
        };
        cart.CartCoupons.Add(coupon);
        _context.CartCoupons.Add(coupon);

        await _context.SaveChangesAsync();
        await RecalculateCartPreviewAsync(cart);
        _logger.LogInformation("Coupon applied to cart. UserId={UserId}, CartId={CartId}, CouponCode={CouponCode}", userId, cart.CartId, dto.CouponCode);
        return MapCart(cart);
    }

    public async Task<CartResponseDto> RemoveCouponAsync(int userId)
    {
        var cart = await LoadActiveCartAsync(userId);
        if (cart is null)
            return new CartResponseDto();

        EnsureActive(cart);
        _context.CartCoupons.RemoveRange(cart.CartCoupons);
        cart.CartCoupons.Clear();
        await _context.SaveChangesAsync();
        await RecalculateCartPreviewAsync(cart);
        _logger.LogInformation("Coupon removed from cart. UserId={UserId}, CartId={CartId}", userId, cart.CartId);
        return MapCart(cart);
    }

    public async Task<OrderCreationResponseDto> CheckoutAsync(int userId, CartCheckoutRequestDto dto)
    {
        _logger.LogInformation("Cart checkout started. UserId={UserId}", userId);
        var cart = await LoadActiveCartAsync(userId) ?? throw new BadRequestException("Cart is empty.");
        EnsureActive(cart);
        var activeItems = cart.CartItems.Where(i => i.IsActive).ToList();
        if (activeItems.Count == 0)
            throw new BadRequestException("Cart is empty.");

        var prescriptionSignatures = activeItems
            .Where(i => i.Prescription is not null)
            .Select(i => BuildPrescriptionSignature(i.Prescription!))
            .Distinct()
            .ToList();
        if (prescriptionSignatures.Count > 1)
            throw new BadRequestException("Cart checkout currently supports one prescription snapshot per order.");

        var orderRequest = new OrderCreationRequestDto
        {
            Customer = dto.Customer,
            Address = dto.Address,
            Payment = dto.Payment,
            Notes = dto.Notes,
            CouponCode = cart.CartCoupons.OrderByDescending(c => c.CreatedAt).FirstOrDefault()?.CouponCode,
            Items = activeItems.Select(i => new OrderCreationItemDto
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                Customizations = i.Customizations.Select(c => new OrderCreationCustomizationDto
                {
                    CustomizationOptionId = c.CustomizationOptionId,
                    CustomizationValueId = c.CustomizationValueId,
                    Type = c.CustomizationName,
                    Value = c.CustomizationValue
                }).ToList()
            }).ToList(),
            Prescription = activeItems.Select(i => i.Prescription).FirstOrDefault(p => p is not null) is { } prescription
                ? new OrderCreationPrescriptionDto
                {
                    RightSphere = prescription.RightSphere,
                    RightCylinder = prescription.RightCylinder,
                    RightAxis = prescription.RightAxis,
                    RightAdd = prescription.RightAdd,
                    LeftSphere = prescription.LeftSphere,
                    LeftCylinder = prescription.LeftCylinder,
                    LeftAxis = prescription.LeftAxis,
                    LeftAdd = prescription.LeftAdd,
                    PD = prescription.PD,
                    Notes = prescription.Notes
                }
                : null
        };

        try
        {
            var order = await _orderCreationService.CreateAsync(userId, orderRequest, async customerOrderId =>
            {
                await AddOrderLogAsync(customerOrderId, userId, "CART_CHECKOUT_STARTED", "Cart checkout started.");
                await AddOrderLogAsync(customerOrderId, userId, "CART_PRODUCTS_VALIDATED", "Cart products validated.");
                await AddOrderLogAsync(customerOrderId, userId, "CART_STOCK_VALIDATED", "Cart stock validated.");
                await AddOrderLogAsync(customerOrderId, userId, "CART_DISCOUNTS_CALCULATED", "Cart discounts calculated.");
                if (cart.CartCoupons.Any())
                    await AddOrderLogAsync(customerOrderId, userId, "CART_COUPON_APPLIED", "Cart coupon applied.");
                await AddOrderLogAsync(customerOrderId, userId, "ORDER_CREATED_FROM_CART", "Order created from cart.");
                await AddOrderLogAsync(customerOrderId, userId, "ORDER_ITEMS_CREATED_FROM_CART", "Order items created from cart.");
                await AddOrderLogAsync(customerOrderId, userId, "PAYMENT_CREATED_FROM_CART", "Payment created from cart.");
                await AddOrderLogAsync(customerOrderId, userId, "STOCK_UPDATED_FROM_CART", "Stock updated from cart checkout.");

                cart.CartStatus = CartStatus.CHECKED_OUT;
                cart.CustomerOrderId = customerOrderId;
                cart.CheckedOutAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                await AddOrderLogAsync(customerOrderId, userId, "CART_MARKED_CHECKED_OUT", "Cart marked checked out.");
                await AddOrderLogAsync(customerOrderId, userId, "CART_CHECKOUT_COMPLETED", "Cart checkout completed.");
            });
            _logger.LogInformation("Cart checkout completed. UserId={UserId}, CartId={CartId}, CustomerOrderId={CustomerOrderId}", userId, cart.CartId, order.CustomerOrderId);
            return order;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cart checkout failed. UserId={UserId}, CartId={CartId}", userId, cart.CartId);
            throw;
        }
    }

    private async Task<Cart> GetOrCreateActiveCartAsync(int userId)
    {
        var cart = await LoadActiveCartAsync(userId);
        if (cart is not null)
            return cart;

        _logger.LogInformation("Cart creation started. UserId={UserId}", userId);
        cart = new Cart { UserId = userId, CartStatus = CartStatus.ACTIVE };
        _context.Carts.Add(cart);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Cart creation completed. UserId={UserId}, CartId={CartId}", userId, cart.CartId);
        return cart;
    }

    private async Task<Cart?> LoadActiveCartAsync(int userId)
    {
        return await _context.Carts
            .Include(c => c.CartItems.Where(i => i.IsActive))
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.ProductImages)
            .Include(c => c.CartItems.Where(i => i.IsActive))
                .ThenInclude(i => i.Customizations)
            .Include(c => c.CartItems.Where(i => i.IsActive))
                .ThenInclude(i => i.Prescription)
            .Include(c => c.CartCoupons)
            .FirstOrDefaultAsync(c => c.UserId == userId && c.CartStatus == CartStatus.ACTIVE);
    }

    private async Task<Product> LoadProductAsync(int productId)
    {
        var product = await _context.Products
            .Include(p => p.CustomizationOptions)
                .ThenInclude(o => o.CustomizationValues)
            .Include(p => p.ProductImages)
            .FirstOrDefaultAsync(p => p.ProductId == productId);

        return product ?? throw new NotFoundException("Product not found.");
    }

    private static void ValidateProductForCart(Product product, int quantity)
    {
        if (!product.IsActive)
            throw new BadRequestException("Product is not available.");

        if (product.AvailableQuantity < quantity)
            throw new BadRequestException("Insufficient product stock.");
    }

    private List<CartItemCustomization> PrepareCustomizations(Product product, List<CartItemCustomizationRequestDto> requests)
    {
        requests ??= new();
        var selectedOptionIds = requests.Where(c => c.CustomizationOptionId.HasValue).Select(c => c.CustomizationOptionId!.Value).ToHashSet();
        foreach (var required in product.CustomizationOptions.Where(o => o.IsRequired))
        {
            if (!selectedOptionIds.Contains(required.CustomizationOptionId))
                throw new BadRequestException("Required product customization is missing.");
        }

        var prepared = new List<CartItemCustomization>();
        foreach (var request in requests)
        {
            var option = request.CustomizationOptionId.HasValue
                ? product.CustomizationOptions.FirstOrDefault(o => o.CustomizationOptionId == request.CustomizationOptionId.Value)
                : null;
            if (request.CustomizationOptionId.HasValue && option is null)
                throw new BadRequestException("Customization option does not belong to the selected product.");

            var value = request.CustomizationValueId.HasValue
                ? product.CustomizationOptions.SelectMany(o => o.CustomizationValues.Select(v => new { Option = o, Value = v }))
                    .FirstOrDefault(x => x.Value.CustomizationValueId == request.CustomizationValueId.Value)
                : null;
            if (request.CustomizationValueId.HasValue && value is null)
                throw new BadRequestException("Customization value does not belong to the selected product.");

            if (option is not null && value is not null && value.Option.CustomizationOptionId != option.CustomizationOptionId)
                throw new BadRequestException("Customization value does not belong to the provided customization option.");

            prepared.Add(new CartItemCustomization
            {
                CustomizationOptionId = option?.CustomizationOptionId ?? value?.Option.CustomizationOptionId,
                CustomizationValueId = value?.Value.CustomizationValueId,
                CustomizationName = option?.Name ?? value?.Option.Name ?? "Customization",
                CustomizationValue = value?.Value.Value ?? "Selected",
                ExtraPrice = value?.Value.AdditionalPrice ?? 0
            });
        }

        return prepared;
    }

    private async Task<CartItemPrescription?> PreparePrescriptionAsync(int userId, Product product, string? prescriptionId, CartItemPrescriptionRequestDto? request)
    {
        if (!product.HasPrescription && (request is not null || !string.IsNullOrWhiteSpace(prescriptionId)))
            throw new BadRequestException("Prescription is not supported for this product.");

        if (!string.IsNullOrWhiteSpace(prescriptionId))
        {
            var existing = await _context.CustomerPrescriptions.FirstOrDefaultAsync(p => p.CustomerPrescriptionsId == prescriptionId && p.UserId == userId);
            if (existing is null)
                throw new BadRequestException("Invalid prescription.");

            return new CartItemPrescription
            {
                UserId = userId,
                CustomerPrescriptionsId = existing.CustomerPrescriptionsId,
                RightSphere = existing.RightSphere,
                RightCylinder = existing.RightCylinder,
                RightAxis = existing.RightAxis,
                RightAdd = existing.RightAdd,
                LeftSphere = existing.LeftSphere,
                LeftCylinder = existing.LeftCylinder,
                LeftAxis = existing.LeftAxis,
                LeftAdd = existing.LeftAdd,
                PD = existing.PD,
                Notes = existing.Notes
            };
        }

        if (request is null)
            return null;

        return new CartItemPrescription
        {
            UserId = userId,
            RightSphere = request.RightSphere,
            RightCylinder = request.RightCylinder,
            RightAxis = request.RightAxis,
            RightAdd = request.RightAdd,
            LeftSphere = request.LeftSphere,
            LeftCylinder = request.LeftCylinder,
            LeftAxis = request.LeftAxis,
            LeftAdd = request.LeftAdd,
            PD = request.PD,
            Notes = request.Notes
        };
    }

    private async Task RecalculateCartPreviewAsync(Cart cart)
    {
        if (!cart.CartItems.Any(i => i.IsActive))
        {
            if (cart.CartCoupons.Count > 0)
            {
                _context.CartCoupons.RemoveRange(cart.CartCoupons);
                cart.CartCoupons.Clear();
            }

            await _context.SaveChangesAsync();
            return;
        }

        var couponCode = cart.CartCoupons.OrderByDescending(c => c.CreatedAt).FirstOrDefault()?.CouponCode;
        DiscountCalculationResultDto discount;
        try
        {
            discount = await CalculateCartDiscountAsync(cart, couponCode);
        }
        catch (BadRequestException ex) when (!string.IsNullOrWhiteSpace(couponCode))
        {
            _logger.LogWarning(ex, "Cart coupon preview became invalid and was removed. UserId={UserId}, CartId={CartId}, CouponCode={CouponCode}", cart.UserId, cart.CartId, couponCode);
            _context.CartCoupons.RemoveRange(cart.CartCoupons);
            cart.CartCoupons.Clear();
            discount = await CalculateCartDiscountAsync(cart, null);
        }

        var activeItems = cart.CartItems.Where(i => i.IsActive).Select((item, index) => new { item, index }).ToList();
        foreach (var entry in activeItems)
        {
            var itemDiscount = discount.Items.FirstOrDefault(i => i.LineNumber == entry.index);
            entry.item.UnitPrice = entry.item.Product.BasePrice + entry.item.Customizations.Sum(c => c.ExtraPrice);
            entry.item.ProductDiscountAmount = itemDiscount?.ProductDiscountAmount ?? 0;
            entry.item.FinalUnitPrice = itemDiscount?.FinalUnitPrice ?? entry.item.UnitPrice;
            entry.item.LineTotal = itemDiscount?.FinalLineTotal ?? entry.item.UnitPrice * entry.item.Quantity;
        }

        var coupon = cart.CartCoupons.OrderByDescending(c => c.CreatedAt).FirstOrDefault();
        if (coupon is not null)
            coupon.CouponDiscountAmount = discount.CouponDiscountAmount;

        await _context.SaveChangesAsync();
    }

    private async Task<DiscountCalculationResultDto> CalculateCartDiscountAsync(Cart cart, string? couponCode)
    {
        var items = cart.CartItems.Where(i => i.IsActive).Select((i, index) => new DiscountCalculationItem
        {
            LineNumber = index,
            ProductId = i.ProductId,
            SKU = i.SKU,
            Quantity = i.Quantity,
            UnitPrice = i.Product.BasePrice + i.Customizations.Sum(c => c.ExtraPrice),
            TotalPrice = (i.Product.BasePrice + i.Customizations.Sum(c => c.ExtraPrice)) * i.Quantity
        }).ToList();

        return await _discountService.ApplyDiscountAsync(new DiscountCalculationContext
        {
            UserId = cart.UserId,
            Subtotal = items.Sum(i => i.TotalPrice),
            CouponCode = couponCode,
            Items = items
        });
    }

    private static void EnsureActive(Cart cart)
    {
        if (cart.CartStatus == CartStatus.CHECKED_OUT)
            throw new ConflictException("Cart is already checked out.");
        if (cart.CartStatus == CartStatus.ABANDONED)
            throw new ConflictException("Cart is abandoned.");
    }

    private static string BuildSignature(CartItem item)
    {
        return BuildSignature(item.ProductId, item.Customizations.ToList(), item.Prescription);
    }

    private static string BuildSignature(int productId, List<CartItemCustomization> customizations, CartItemPrescription? prescription)
    {
        var customPart = string.Join("|", customizations
            .OrderBy(c => c.CustomizationOptionId)
            .ThenBy(c => c.CustomizationValueId)
            .Select(c => $"{c.CustomizationOptionId}:{c.CustomizationValueId}:{c.CustomizationName}:{c.CustomizationValue}"));
        var prescriptionPart = prescription is null
            ? "none"
            : $"{prescription.CustomerPrescriptionsId}:{prescription.RightSphere}:{prescription.RightCylinder}:{prescription.RightAxis}:{prescription.LeftSphere}:{prescription.LeftCylinder}:{prescription.LeftAxis}:{prescription.PD}";
        return $"{productId}::{customPart}::{prescriptionPart}";
    }

    private static string BuildPrescriptionSignature(CartItemPrescription prescription)
    {
        return $"{prescription.CustomerPrescriptionsId}:{prescription.RightSphere}:{prescription.RightCylinder}:{prescription.RightAxis}:{prescription.RightAdd}:{prescription.LeftSphere}:{prescription.LeftCylinder}:{prescription.LeftAxis}:{prescription.LeftAdd}:{prescription.PD}:{prescription.Notes}";
    }

    private CartResponseDto MapCart(Cart cart)
    {
        var coupon = cart.CartCoupons.OrderByDescending(c => c.CreatedAt).FirstOrDefault();
        var items = cart.CartItems.Where(i => i.IsActive).Select(i => new CartItemResponseDto
        {
            CartItemId = i.CartItemId,
            ProductId = i.ProductId,
            ProductName = i.Product.Name,
            ProductImageUrl = i.Product.ProductImages.OrderByDescending(img => img.IsPrimary).ThenBy(img => img.DisplayOrder).FirstOrDefault()?.ImageUrl,
            SKU = i.SKU,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            ProductDiscountAmount = i.ProductDiscountAmount,
            FinalUnitPrice = i.FinalUnitPrice,
            LineTotal = i.LineTotal,
            InStock = i.Product.IsActive && i.Product.AvailableQuantity >= i.Quantity,
            Customizations = i.Customizations.Select(c => new CartItemCustomizationResponseDto
            {
                CartItemCustomizationId = c.CartItemCustomizationId,
                CustomizationOptionId = c.CustomizationOptionId,
                CustomizationValueId = c.CustomizationValueId,
                CustomizationName = c.CustomizationName,
                CustomizationValue = c.CustomizationValue,
                ExtraPrice = c.ExtraPrice
            }).ToList(),
            Prescription = i.Prescription is null ? null : new CartItemPrescriptionResponseDto
            {
                CartItemPrescriptionId = i.Prescription.CartItemPrescriptionId,
                CustomerPrescriptionsId = i.Prescription.CustomerPrescriptionsId,
                RightSphere = i.Prescription.RightSphere,
                RightCylinder = i.Prescription.RightCylinder,
                RightAxis = i.Prescription.RightAxis,
                RightAdd = i.Prescription.RightAdd,
                LeftSphere = i.Prescription.LeftSphere,
                LeftCylinder = i.Prescription.LeftCylinder,
                LeftAxis = i.Prescription.LeftAxis,
                LeftAdd = i.Prescription.LeftAdd,
                PD = i.Prescription.PD,
                Notes = i.Prescription.Notes
            }
        }).ToList();

        return new CartResponseDto
        {
            CartId = cart.CartId,
            CartStatus = cart.CartStatus,
            Items = items,
            CouponCode = coupon?.CouponCode,
            CouponDiscountAmount = coupon?.CouponDiscountAmount ?? 0,
            Subtotal = items.Sum(i => i.UnitPrice * i.Quantity),
            ProductDiscountTotal = items.Sum(i => i.ProductDiscountAmount * i.Quantity),
            FinalAmount = items.Sum(i => i.LineTotal) - (coupon?.CouponDiscountAmount ?? 0)
        };
    }

    private async Task AddOrderLogAsync(string customerOrderId, int userId, string eventType, string message)
    {
        await _orderStatusLogService.CreateAsync(new CreateOrderStatusLogDto
        {
            CustomerOrderId = customerOrderId,
            Status = OrderStatus.CREATED,
            PaymentStatus = PaymentStatus.PENDING,
            EventType = eventType,
            Comment = message,
            LogMessage = message,
            CreatedByUserId = userId
        });
    }
}
