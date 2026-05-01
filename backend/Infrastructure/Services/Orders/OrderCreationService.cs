using backend.Application.Abstractions.Orders;
using backend.Application.Exceptions;
using backend.Data;
using backend.DTOs.CustomerPrescriptionDtos;
using backend.DTOs.OrderAddressDtos;
using backend.DTOs.OrderCreationDtos;
using backend.DTOs.OrderDtos;
using backend.DTOs.OrderItemCustomizationDtos;
using backend.DTOs.OrderItemDtos;
using backend.DTOs.OrderStatusLogDtos;
using backend.DTOs.PaymentDtos;
using backend.Models.Orders;
using backend.Models.Products;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Services.Orders;

public class OrderCreationService : IOrderCreationService
{
    private readonly AppDbContext _context;
    private readonly IOrderCreationValidatorService _validator;
    private readonly ICustomerOrderIdGeneratorService _customerOrderIdGenerator;
    private readonly IDiscountService _discountService;
    private readonly IOrderService _orderService;
    private readonly IOrderItemService _orderItemService;
    private readonly IOrderItemCustomizationService _orderItemCustomizationService;
    private readonly IPaymentService _paymentService;
    private readonly IOrderAddressService _orderAddressService;
    private readonly IOrderStatusLogService _orderStatusLogService;
    private readonly ICustomerPrescriptionService _customerPrescriptionService;
    private readonly ILogger<OrderCreationService> _logger;

    public OrderCreationService(
        AppDbContext context,
        IOrderCreationValidatorService validator,
        ICustomerOrderIdGeneratorService customerOrderIdGenerator,
        IDiscountService discountService,
        IOrderService orderService,
        IOrderItemService orderItemService,
        IOrderItemCustomizationService orderItemCustomizationService,
        IPaymentService paymentService,
        IOrderAddressService orderAddressService,
        IOrderStatusLogService orderStatusLogService,
        ICustomerPrescriptionService customerPrescriptionService,
        ILogger<OrderCreationService> logger)
    {
        _context = context;
        _validator = validator;
        _customerOrderIdGenerator = customerOrderIdGenerator;
        _discountService = discountService;
        _orderService = orderService;
        _orderItemService = orderItemService;
        _orderItemCustomizationService = orderItemCustomizationService;
        _paymentService = paymentService;
        _orderAddressService = orderAddressService;
        _orderStatusLogService = orderStatusLogService;
        _customerPrescriptionService = customerPrescriptionService;
        _logger = logger;
    }

    public async Task<OrderCreationResponseDto> CreateAsync(int userId, OrderCreationRequestDto dto)
    {
        var currentStep = "start";
        string? customerOrderId = null;
        if (dto is null)
            throw new BadRequestException("Request body is required.");

        _logger.LogInformation("Order creation started. UserId={UserId}, ItemCount={ItemCount}", userId, dto.Items?.Count ?? 0);

        await using var transaction = await _context.Database.BeginTransactionAsync();
        _logger.LogInformation("Order creation transaction started. UserId={UserId}", userId);

        try
        {
            currentStep = "validation";
            _logger.LogInformation("Product validation started. UserId={UserId}", userId);
            await _validator.ValidateAsync(userId, dto);
            _logger.LogInformation("Product validation completed. UserId={UserId}", userId);

            currentStep = "customer_order_id_generation";
            customerOrderId = await _customerOrderIdGenerator.GenerateAsync();
            _logger.LogInformation("CustomerOrderId generated for order creation. UserId={UserId}, CustomerOrderId={CustomerOrderId}", userId, customerOrderId);

            currentStep = "totals_calculation";
            var preparedItems = await PrepareItemsAsync(dto);
            var subtotal = preparedItems.Sum(i => i.TotalPrice);
            _logger.LogInformation("Order totals calculated. UserId={UserId}, CustomerOrderId={CustomerOrderId}, Subtotal={Subtotal}", userId, customerOrderId, subtotal);

            currentStep = "discount_application";
            var discount = await _discountService.ApplyDiscountAsync(new DiscountCalculationContext
            {
                UserId = userId,
                Subtotal = subtotal,
                CouponCode = dto.CouponCode ?? dto.DiscountCode,
                Items = preparedItems.Select((i, index) => new DiscountCalculationItem
                {
                    LineNumber = index,
                    ProductId = i.Product.ProductId,
                    SKU = i.Product.SKU,
                    Quantity = i.Request.Quantity,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.TotalPrice
                }).ToList()
            });
            ValidateDiscountResult(subtotal, discount);
            _logger.LogInformation(
                "Discount applied. UserId={UserId}, CustomerOrderId={CustomerOrderId}, DiscountAmount={DiscountAmount}, FinalAmount={FinalAmount}",
                userId,
                customerOrderId,
                discount.DiscountAmount,
                discount.FinalAmount);

            currentStep = "order_creation";
            var order = await _orderService.CreateAsync(new CreateOrderDto
            {
                CustomerOrderId = customerOrderId,
                UserId = userId,
                CustomerName = dto.Customer.Name,
                CustomerEmail = dto.Customer.Email,
                CustomerPhone = dto.Customer.Phone,
                TotalAmount = discount.FinalAmount,
                OriginalSubtotal = discount.OriginalSubtotal,
                ProductDiscountTotal = discount.ProductDiscountTotal,
                CouponCode = discount.CouponCode,
                CouponDiscountAmount = discount.CouponDiscountAmount,
                FinalAmount = discount.FinalAmount,
                PaymentStatus = PaymentStatus.PENDING,
                OrderStatus = OrderStatus.CREATED,
                Notes = dto.Notes
            });
            _logger.LogInformation("Order record created. UserId={UserId}, CustomerOrderId={CustomerOrderId}, OrdersId={OrdersId}", userId, customerOrderId, order.OrdersId);
            await AddJourneyLogAsync(customerOrderId, OrderStatus.CREATED, PaymentStatus.PENDING, "ORDER_CREATION_STARTED", "Order creation started.", userId);
            await AddJourneyLogAsync(customerOrderId, OrderStatus.CREATED, PaymentStatus.PENDING, "PRODUCT_VALIDATION_COMPLETED", "Product validation completed.", userId);
            await AddJourneyLogAsync(customerOrderId, OrderStatus.CREATED, PaymentStatus.PENDING, "STOCK_VALIDATION_COMPLETED", "Stock validation completed.", userId);
            if (discount.ProductDiscountTotal > 0)
                await AddJourneyLogAsync(customerOrderId, OrderStatus.CREATED, PaymentStatus.PENDING, "PRODUCT_DISCOUNT_APPLIED", $"Product discount applied: {discount.ProductDiscountTotal:0.00}.", userId);
            if (discount.CouponDiscountAmount > 0)
                await AddJourneyLogAsync(customerOrderId, OrderStatus.CREATED, PaymentStatus.PENDING, "COUPON_APPLIED", $"Coupon applied: {discount.CouponCode}.", userId);
            await AddJourneyLogAsync(customerOrderId, OrderStatus.CREATED, PaymentStatus.PENDING, "ORDER_CREATED", "Order record created.", userId);

            currentStep = "items_creation";
            var createdItems = new List<OrderCreationItemResponseDto>();
            foreach (var preparedItem in preparedItems.Select((value, index) => new { value, index }))
            {
                var itemDiscount = discount.Items.First(i => i.LineNumber == preparedItem.index);
                var item = await _orderItemService.CreateAsync(new CreateOrderItemDto
                {
                    CustomerOrderId = customerOrderId,
                    ProductId = preparedItem.value.Product.ProductId,
                    SKU = preparedItem.value.Product.SKU,
                    ProductName = preparedItem.value.Product.Name,
                    Quantity = preparedItem.value.Request.Quantity,
                    Price = itemDiscount.FinalUnitPrice,
                    OriginalUnitPrice = itemDiscount.OriginalUnitPrice,
                    ProductDiscountAmount = itemDiscount.ProductDiscountAmount,
                    FinalUnitPrice = itemDiscount.FinalUnitPrice,
                    FinalLineTotal = itemDiscount.FinalLineTotal
                });
                _logger.LogInformation("Order item created. CustomerOrderId={CustomerOrderId}, OrderItemId={OrderItemId}, ProductId={ProductId}", customerOrderId, item.OrderItemsId, item.ProductId);

                currentStep = "customizations_creation";
                var customizations = new List<OrderItemCustomizationResponseDto>();
                foreach (var customization in preparedItem.value.Customizations)
                {
                    var createdCustomization = await _orderItemCustomizationService.CreateAsync(new CreateOrderItemCustomizationDto
                    {
                        OrderItemId = item.OrderItemsId,
                        CustomizationOptionId = customization.CustomizationOptionId,
                        CustomizationValueId = customization.CustomizationValueId,
                        Type = customization.Type,
                        Value = customization.Value
                    });
                    customizations.Add(createdCustomization);
                }
                _logger.LogInformation("Order item customizations created. CustomerOrderId={CustomerOrderId}, OrderItemId={OrderItemId}, Count={Count}", customerOrderId, item.OrderItemsId, customizations.Count);

                createdItems.Add(new OrderCreationItemResponseDto
                {
                    Item = item,
                    Customizations = customizations
                });
            }
            await AddJourneyLogAsync(customerOrderId, OrderStatus.CREATED, PaymentStatus.PENDING, "ORDER_ITEMS_CREATED", "Order items created.", userId);

            currentStep = "address_creation";
            var address = await _orderAddressService.CreateAsync(new CreateOrderAddressDto
            {
                CustomerOrderId = customerOrderId,
                Type = dto.Address.Type,
                Line1 = dto.Address.Line1,
                Line2 = dto.Address.Line2,
                City = dto.Address.City,
                State = dto.Address.State,
                Pincode = dto.Address.Pincode,
                Country = dto.Address.Country
            });
            _logger.LogInformation("Order address created. CustomerOrderId={CustomerOrderId}, OrderAddressesId={OrderAddressesId}", customerOrderId, address.OrderAddressesId);

            currentStep = "prescription_creation";
            var prescription = dto.Prescription is null
                ? null
                : await _customerPrescriptionService.CreateAsync(new CreateCustomerPrescriptionDto
                {
                    UserId = userId,
                    CustomerOrderId = customerOrderId,
                    RightSphere = dto.Prescription.RightSphere,
                    RightCylinder = dto.Prescription.RightCylinder,
                    RightAxis = dto.Prescription.RightAxis,
                    RightAdd = dto.Prescription.RightAdd,
                    LeftSphere = dto.Prescription.LeftSphere,
                    LeftCylinder = dto.Prescription.LeftCylinder,
                    LeftAxis = dto.Prescription.LeftAxis,
                    LeftAdd = dto.Prescription.LeftAdd,
                    PD = dto.Prescription.PD,
                    Notes = dto.Prescription.Notes
                });
            _logger.LogInformation("Order prescription step completed. CustomerOrderId={CustomerOrderId}, Created={Created}", customerOrderId, prescription is not null);

            currentStep = "coupon_usage_creation";
            if (!string.IsNullOrWhiteSpace(discount.CouponId) && !string.IsNullOrWhiteSpace(discount.CouponCode) && discount.CouponDiscountAmount > 0)
            {
                await _discountService.RecordCouponUsageAsync(discount.CouponId, userId, customerOrderId, discount.CouponCode, discount.CouponDiscountAmount);
                await AddJourneyLogAsync(customerOrderId, OrderStatus.CREATED, PaymentStatus.PENDING, "COUPON_USAGE_CREATED", "Coupon usage recorded.", userId);
            }

            currentStep = "payment_creation";
            var payment = await _paymentService.CreateAsync(new CreatePaymentDto
            {
                CustomerOrderId = customerOrderId,
                Method = dto.Payment.Method,
                TransactionId = dto.Payment.TransactionId,
                Amount = discount.FinalAmount,
                Status = PaymentTxnStatus.INITIATED
            });
            _logger.LogInformation("Payment record created. CustomerOrderId={CustomerOrderId}, PaymentsId={PaymentsId}, Method={PaymentMethod}, Status={PaymentStatus}", customerOrderId, payment.PaymentsId, payment.Method, payment.Status);
            await AddJourneyLogAsync(customerOrderId, OrderStatus.CREATED, PaymentStatus.PENDING, "PAYMENT_CREATED", "Payment row created.", userId);

            currentStep = "status_log_creation";
            var statusLog = await AddJourneyLogAsync(customerOrderId, OrderStatus.CREATED, PaymentStatus.PENDING, "ORDER_CONFIRMED", "Order confirmed.", userId);
            _logger.LogInformation("Order status log created. CustomerOrderId={CustomerOrderId}, OrderStatusLogsId={OrderStatusLogsId}", customerOrderId, statusLog.OrderStatusLogsId);

            currentStep = "inventory_update";
            await UpdateInventoryAsync(preparedItems);
            _logger.LogInformation("Inventory updated for order. CustomerOrderId={CustomerOrderId}, ItemCount={ItemCount}", customerOrderId, preparedItems.Count);
            await AddJourneyLogAsync(customerOrderId, OrderStatus.CREATED, PaymentStatus.PENDING, "STOCK_UPDATED", "Stock updated.", userId);

            currentStep = "transaction_commit";
            await transaction.CommitAsync();
            _logger.LogInformation("Order creation transaction committed. UserId={UserId}, CustomerOrderId={CustomerOrderId}", userId, customerOrderId);

            var response = new OrderCreationResponseDto
            {
                CustomerOrderId = customerOrderId,
                Subtotal = discount.Subtotal,
                OriginalSubtotal = discount.OriginalSubtotal,
                ProductDiscountTotal = discount.ProductDiscountTotal,
                CouponCode = discount.CouponCode,
                CouponDiscountAmount = discount.CouponDiscountAmount,
                DiscountAmount = discount.DiscountAmount,
                FinalAmount = discount.FinalAmount,
                Order = order,
                Address = address,
                Items = createdItems,
                Prescription = prescription,
                Payment = payment,
                StatusLog = statusLog
            };

            _logger.LogInformation("Order creation succeeded. UserId={UserId}, CustomerOrderId={CustomerOrderId}, FinalAmount={FinalAmount}", userId, customerOrderId, response.FinalAmount);
            return response;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Order creation failed and transaction rolled back. UserId={UserId}, Step={Step}", userId, currentStep);
            throw;
        }
    }

    private async Task<List<PreparedOrderItem>> PrepareItemsAsync(OrderCreationRequestDto dto)
    {
        var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _context.Products
            .Include(p => p.CustomizationOptions)
                .ThenInclude(o => o.CustomizationValues)
            .Where(p => productIds.Contains(p.ProductId))
            .ToDictionaryAsync(p => p.ProductId);

        var preparedItems = new List<PreparedOrderItem>();
        foreach (var item in dto.Items)
        {
            var product = products[item.ProductId];
            var customizations = PrepareCustomizations(item, product);
            var customizationTotal = customizations.Sum(c => c.AdditionalPrice);
            var unitPrice = product.BasePrice + customizationTotal;

            preparedItems.Add(new PreparedOrderItem
            {
                Request = item,
                Product = product,
                UnitPrice = unitPrice,
                TotalPrice = unitPrice * item.Quantity,
                Customizations = customizations
            });
        }

        return preparedItems;
    }

    private static List<PreparedOrderItemCustomization> PrepareCustomizations(OrderCreationItemDto item, Product product)
    {
        var prepared = new List<PreparedOrderItemCustomization>();

        foreach (var customization in item.Customizations)
        {
            var option = customization.CustomizationOptionId.HasValue
                ? product.CustomizationOptions.FirstOrDefault(o => o.CustomizationOptionId == customization.CustomizationOptionId.Value)
                : null;

            var value = customization.CustomizationValueId.HasValue
                ? product.CustomizationOptions
                    .SelectMany(o => o.CustomizationValues.Select(v => new { Option = o, Value = v }))
                    .FirstOrDefault(x => x.Value.CustomizationValueId == customization.CustomizationValueId.Value)
                : null;

            prepared.Add(new PreparedOrderItemCustomization
            {
                CustomizationOptionId = option?.CustomizationOptionId ?? value?.Option.CustomizationOptionId,
                CustomizationValueId = value?.Value.CustomizationValueId,
                Type = option?.Name ?? value?.Option.Name ?? customization.Type ?? "Customization",
                Value = value?.Value.Value ?? customization.Value ?? "Selected",
                AdditionalPrice = value?.Value.AdditionalPrice ?? 0
            });
        }

        return prepared;
    }

    private static void ValidateDiscountResult(decimal subtotal, DiscountCalculationResultDto discount)
    {
        if (discount.Subtotal != subtotal)
            throw new BadRequestException("Discount calculation subtotal mismatch.");

        if (discount.DiscountAmount < 0 || discount.DiscountAmount > subtotal)
            throw new BadRequestException("Invalid discount amount.");

        if (discount.FinalAmount != subtotal - discount.DiscountAmount)
            throw new BadRequestException("Invalid final order amount.");
    }

    private async Task<OrderStatusLogResponseDto> AddJourneyLogAsync(
        string customerOrderId,
        OrderStatus orderStatus,
        PaymentStatus paymentStatus,
        string eventType,
        string message,
        int userId)
    {
        return await _orderStatusLogService.CreateAsync(new CreateOrderStatusLogDto
        {
            CustomerOrderId = customerOrderId,
            Status = orderStatus,
            PaymentStatus = paymentStatus,
            EventType = eventType,
            Comment = message,
            LogMessage = message,
            CreatedByUserId = userId
        });
    }

    private async Task UpdateInventoryAsync(IEnumerable<PreparedOrderItem> items)
    {
        var productQuantities = items
            .GroupBy(i => i.Product.ProductId)
            .Select(g => new { ProductId = g.Key, Quantity = g.Sum(i => i.Request.Quantity) });

        foreach (var productQuantity in productQuantities)
        {
            var affectedRows = await _context.Database.ExecuteSqlInterpolatedAsync($"""
                UPDATE "Products"
                SET "AvailableQuantity" = "AvailableQuantity" - {productQuantity.Quantity},
                    "SoldQuantity" = "SoldQuantity" + {productQuantity.Quantity},
                    "UpdatedAt" = CURRENT_TIMESTAMP
                WHERE "ProductId" = {productQuantity.ProductId}
                  AND "AvailableQuantity" >= {productQuantity.Quantity}
                """);

            if (affectedRows != 1)
                throw new ConflictException("Insufficient product stock.");
        }
    }

    private sealed class PreparedOrderItem
    {
        public OrderCreationItemDto Request { get; set; } = new();
        public Product Product { get; set; } = new();
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public List<PreparedOrderItemCustomization> Customizations { get; set; } = new();
    }

    private sealed class PreparedOrderItemCustomization
    {
        public int? CustomizationOptionId { get; set; }
        public int? CustomizationValueId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public decimal AdditionalPrice { get; set; }
    }
}
