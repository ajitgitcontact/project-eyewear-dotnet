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
using Npgsql;

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

    public async Task<OrderCreationResponseDto> CreateAsync(int userId, OrderCreationRequestDto dto, string? idempotencyKey = null, Func<string, Task>? beforeCommitAsync = null)
    {
        var currentStep = "start";
        var currentStepDetail = "Order creation request accepted by service.";
        string? customerOrderId = null;
        if (dto is null)
            throw new BadRequestException("Request body is required.");

        idempotencyKey = NormalizeIdempotencyKey(idempotencyKey);
        if (idempotencyKey is not null)
        {
            var existing = await GetByIdempotencyKeyAsync(userId, idempotencyKey);
            if (existing is not null)
            {
                _logger.LogInformation("Order creation idempotency replay. UserId={UserId}, CustomerOrderId={CustomerOrderId}", userId, existing.CustomerOrderId);
                return existing;
            }
        }

        _logger.LogInformation("Order creation started. UserId={UserId}, ItemCount={ItemCount}, IdempotencyKeyProvided={IdempotencyKeyProvided}", userId, dto.Items?.Count ?? 0, idempotencyKey is not null);

        await using var transaction = await _context.Database.BeginTransactionAsync();
        _logger.LogInformation("Order creation transaction started. UserId={UserId}", userId);

        try
        {
            currentStep = "validation";
            currentStepDetail = $"Validating payload. ItemCount={dto.Items?.Count ?? 0}, CouponProvided={!string.IsNullOrWhiteSpace(dto.CouponCode ?? dto.DiscountCode)}";
            _logger.LogInformation("Product validation started. UserId={UserId}", userId);
            await _validator.ValidateAsync(userId, dto);
            _logger.LogInformation("Product validation completed. UserId={UserId}", userId);

            currentStep = "customer_order_id_generation";
            currentStepDetail = "Generating backend CustomerOrderId.";
            customerOrderId = await _customerOrderIdGenerator.GenerateAsync();
            _logger.LogInformation("CustomerOrderId generated for order creation. UserId={UserId}, CustomerOrderId={CustomerOrderId}", userId, customerOrderId);

            currentStep = "totals_calculation";
            currentStepDetail = "Preparing product, customization, and line-total snapshots.";
            var preparedItems = await PrepareItemsAsync(dto);
            var subtotal = preparedItems.Sum(i => i.TotalPrice);
            _logger.LogInformation("Order totals calculated. UserId={UserId}, CustomerOrderId={CustomerOrderId}, Subtotal={Subtotal}", userId, customerOrderId, subtotal);

            currentStep = "discount_application";
            currentStepDetail = $"Applying admin discounts and coupon. CouponProvided={!string.IsNullOrWhiteSpace(dto.CouponCode ?? dto.DiscountCode)}";
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
            currentStepDetail = "Creating Orders row with backend-calculated totals and statuses.";
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
                IdempotencyKey = idempotencyKey,
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
            currentStepDetail = $"Creating order items. ItemCount={preparedItems.Count}";
            var createdItems = new List<OrderCreationItemResponseDto>();
            foreach (var preparedItem in preparedItems.Select((value, index) => new { value, index }))
            {
                currentStep = "items_creation";
                currentStepDetail = $"Creating order item. LineNumber={preparedItem.index}, ProductId={preparedItem.value.Product.ProductId}, Quantity={preparedItem.value.Request.Quantity}";
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
                currentStepDetail = $"Creating item customizations. LineNumber={preparedItem.index}, ProductId={preparedItem.value.Product.ProductId}, OrderItemId={item.OrderItemsId}, CustomizationCount={preparedItem.value.Customizations.Count}";
                var customizations = new List<OrderItemCustomizationResponseDto>();
                foreach (var customization in preparedItem.value.Customizations)
                {
                    currentStepDetail = $"Creating item customization. LineNumber={preparedItem.index}, ProductId={preparedItem.value.Product.ProductId}, OrderItemId={item.OrderItemsId}, CustomizationOptionId={customization.CustomizationOptionId}, CustomizationValueId={customization.CustomizationValueId}";
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
            currentStepDetail = "Creating order address row.";
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
            currentStepDetail = dto.Prescription is null
                ? "Skipping prescription creation because no prescription was provided."
                : "Creating optional prescription snapshot.";
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
            currentStepDetail = string.IsNullOrWhiteSpace(discount.CouponId)
                ? "Skipping coupon usage because no coupon discount was applied."
                : $"Recording coupon usage. CouponId={discount.CouponId}, CouponCode={discount.CouponCode}";
            if (!string.IsNullOrWhiteSpace(discount.CouponId) && !string.IsNullOrWhiteSpace(discount.CouponCode) && discount.CouponDiscountAmount > 0)
            {
                await _discountService.RecordCouponUsageAsync(discount.CouponId, userId, customerOrderId, discount.CouponCode, discount.CouponDiscountAmount);
                await AddJourneyLogAsync(customerOrderId, OrderStatus.CREATED, PaymentStatus.PENDING, "COUPON_USAGE_CREATED", "Coupon usage recorded.", userId);
            }

            currentStep = "payment_creation";
            currentStepDetail = $"Creating payment row. Method={dto.Payment.Method}, Amount={discount.FinalAmount}";
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
            currentStepDetail = "Creating final order confirmed journey log.";
            var statusLog = await AddJourneyLogAsync(customerOrderId, OrderStatus.CREATED, PaymentStatus.PENDING, "ORDER_CONFIRMED", "Order confirmed.", userId);
            _logger.LogInformation("Order status log created. CustomerOrderId={CustomerOrderId}, OrderStatusLogsId={OrderStatusLogsId}", customerOrderId, statusLog.OrderStatusLogsId);

            currentStep = "inventory_update";
            currentStepDetail = $"Updating inventory atomically. DistinctProductCount={preparedItems.Select(i => i.Product.ProductId).Distinct().Count()}";
            await UpdateInventoryAsync(preparedItems);
            _logger.LogInformation("Inventory updated for order. CustomerOrderId={CustomerOrderId}, ItemCount={ItemCount}", customerOrderId, preparedItems.Count);
            await AddJourneyLogAsync(customerOrderId, OrderStatus.CREATED, PaymentStatus.PENDING, "STOCK_UPDATED", "Stock updated.", userId);

            if (beforeCommitAsync is not null)
            {
                currentStep = "before_commit_callback";
                currentStepDetail = "Running checkout caller callback before transaction commit.";
                await beforeCommitAsync(customerOrderId);
            }

            currentStep = "transaction_commit";
            currentStepDetail = "Committing order creation transaction.";
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
            var failure = GetFailureDetails(ex);
            try
            {
                await transaction.RollbackAsync();
                LogOrderCreationFailure(ex, failure, userId, customerOrderId, currentStep, currentStepDetail, dto, idempotencyKey, rollbackSucceeded: true);
            }
            catch (Exception rollbackException)
            {
                _logger.LogError(
                    rollbackException,
                    "Order creation rollback failed. UserId={UserId}, CustomerOrderId={CustomerOrderId}, FailedStep={FailedStep}, OriginalFailureReason={FailureReason}",
                    userId,
                    customerOrderId,
                    currentStep,
                    failure.Reason);

                LogOrderCreationFailure(ex, failure, userId, customerOrderId, currentStep, currentStepDetail, dto, idempotencyKey, rollbackSucceeded: false);
            }

            if (idempotencyKey is not null && IsUniqueViolation(ex))
            {
                var existing = await GetByIdempotencyKeyAsync(userId, idempotencyKey);
                if (existing is not null)
                {
                    _logger.LogInformation("Order creation unique idempotency conflict returned existing order. UserId={UserId}, CustomerOrderId={CustomerOrderId}", userId, existing.CustomerOrderId);
                    return existing;
                }
            }

            throw;
        }
    }

    private void LogOrderCreationFailure(
        Exception exception,
        OrderCreationFailureDetails failure,
        int userId,
        string? customerOrderId,
        string step,
        string stepDetail,
        OrderCreationRequestDto dto,
        string? idempotencyKey,
        bool rollbackSucceeded)
    {
        var logLevel = failure.IsExpectedBusinessFailure ? LogLevel.Warning : LogLevel.Error;
        _logger.Log(
            logLevel,
            exception,
            "Order creation failed. UserId={UserId}, CustomerOrderId={CustomerOrderId}, FailedStep={FailedStep}, StepDetail={StepDetail}, FailureCategory={FailureCategory}, FailureReason={FailureReason}, ExceptionType={ExceptionType}, HttpStatus={HttpStatus}, DbSqlState={DbSqlState}, DbConstraint={DbConstraint}, RollbackSucceeded={RollbackSucceeded}, IdempotencyKeyProvided={IdempotencyKeyProvided}, ItemCount={ItemCount}, CouponProvided={CouponProvided}",
            userId,
            customerOrderId,
            step,
            stepDetail,
            failure.Category,
            failure.Reason,
            failure.ExceptionType,
            failure.HttpStatus,
            failure.DbSqlState,
            failure.DbConstraint,
            rollbackSucceeded,
            idempotencyKey is not null,
            dto.Items?.Count ?? 0,
            !string.IsNullOrWhiteSpace(dto.CouponCode ?? dto.DiscountCode));
    }

    public async Task<OrderCreationResponseDto?> GetByIdempotencyKeyAsync(int userId, string idempotencyKey)
    {
        idempotencyKey = NormalizeIdempotencyKey(idempotencyKey) ?? string.Empty;
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            return null;

        var order = await _context.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.UserId == userId && o.IdempotencyKey == idempotencyKey);

        if (order is null)
            return null;

        return await BuildExistingResponseAsync(order.CustomerOrderId);
    }

    private async Task<OrderCreationResponseDto> BuildExistingResponseAsync(string existingCustomerOrderId)
    {
        var order = await _orderService.GetByCustomerOrderIdAsync(existingCustomerOrderId);
        var addresses = (await _orderAddressService.GetByCustomerOrderIdAsync(existingCustomerOrderId)).ToList();
        var items = (await _orderItemService.GetByCustomerOrderIdAsync(existingCustomerOrderId)).ToList();
        var prescriptions = (await _customerPrescriptionService.GetByCustomerOrderIdAsync(existingCustomerOrderId)).ToList();
        var payments = (await _paymentService.GetByCustomerOrderIdAsync(existingCustomerOrderId)).ToList();
        var statusLogs = (await _orderStatusLogService.GetByCustomerOrderIdAsync(existingCustomerOrderId)).ToList();

        var itemResponses = new List<OrderCreationItemResponseDto>();
        foreach (var item in items)
        {
            itemResponses.Add(new OrderCreationItemResponseDto
            {
                Item = item,
                Customizations = (await _orderItemCustomizationService.GetByOrderItemIdAsync(item.OrderItemsId)).ToList()
            });
        }

        return new OrderCreationResponseDto
        {
            CustomerOrderId = order.CustomerOrderId,
            Subtotal = order.OriginalSubtotal,
            OriginalSubtotal = order.OriginalSubtotal,
            ProductDiscountTotal = order.ProductDiscountTotal,
            CouponCode = order.CouponCode,
            CouponDiscountAmount = order.CouponDiscountAmount,
            DiscountAmount = order.ProductDiscountTotal + order.CouponDiscountAmount,
            FinalAmount = order.FinalAmount,
            Order = order,
            Address = addresses.FirstOrDefault() ?? new OrderAddressResponseDto(),
            Items = itemResponses,
            Prescription = prescriptions.FirstOrDefault(),
            Payment = payments.FirstOrDefault() ?? new PaymentResponseDto(),
            StatusLog = statusLogs.OrderBy(l => l.CreatedAt).LastOrDefault() ?? new OrderStatusLogResponseDto()
        };
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

    private static string? NormalizeIdempotencyKey(string? idempotencyKey)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            return null;

        idempotencyKey = idempotencyKey.Trim();
        if (idempotencyKey.Length > 100)
            throw new BadRequestException("Idempotency-Key must be 100 characters or fewer.");

        return idempotencyKey;
    }

    private static bool IsUniqueViolation(Exception ex)
    {
        if (ex is PostgresException postgresException)
            return postgresException.SqlState == PostgresErrorCodes.UniqueViolation;

        return ex.InnerException is not null && IsUniqueViolation(ex.InnerException);
    }

    private static OrderCreationFailureDetails GetFailureDetails(Exception ex)
    {
        var postgresException = FindPostgresException(ex);

        return ex switch
        {
            BadRequestException badRequest => new OrderCreationFailureDetails(
                Category: "BadRequest",
                Reason: badRequest.Message,
                ExceptionType: ex.GetType().Name,
                HttpStatus: StatusCodes.Status400BadRequest,
                DbSqlState: postgresException?.SqlState,
                DbConstraint: postgresException?.ConstraintName,
                IsExpectedBusinessFailure: true),

            NotFoundException notFound => new OrderCreationFailureDetails(
                Category: "NotFound",
                Reason: notFound.Message,
                ExceptionType: ex.GetType().Name,
                HttpStatus: StatusCodes.Status404NotFound,
                DbSqlState: postgresException?.SqlState,
                DbConstraint: postgresException?.ConstraintName,
                IsExpectedBusinessFailure: true),

            ConflictException conflict => new OrderCreationFailureDetails(
                Category: "Conflict",
                Reason: conflict.Message,
                ExceptionType: ex.GetType().Name,
                HttpStatus: StatusCodes.Status409Conflict,
                DbSqlState: postgresException?.SqlState,
                DbConstraint: postgresException?.ConstraintName,
                IsExpectedBusinessFailure: true),

            DbUpdateException => new OrderCreationFailureDetails(
                Category: postgresException is null ? "DatabaseUpdate" : "DatabaseUpdatePostgres",
                Reason: GetDatabaseFailureReason(postgresException),
                ExceptionType: ex.GetType().Name,
                HttpStatus: StatusCodes.Status500InternalServerError,
                DbSqlState: postgresException?.SqlState,
                DbConstraint: postgresException?.ConstraintName,
                IsExpectedBusinessFailure: false),

            InvalidOperationException invalidOperation => new OrderCreationFailureDetails(
                Category: "InvalidOperation",
                Reason: invalidOperation.Message,
                ExceptionType: ex.GetType().Name,
                HttpStatus: StatusCodes.Status409Conflict,
                DbSqlState: postgresException?.SqlState,
                DbConstraint: postgresException?.ConstraintName,
                IsExpectedBusinessFailure: false),

            _ => new OrderCreationFailureDetails(
                Category: "Unexpected",
                Reason: ex.Message,
                ExceptionType: ex.GetType().Name,
                HttpStatus: StatusCodes.Status500InternalServerError,
                DbSqlState: postgresException?.SqlState,
                DbConstraint: postgresException?.ConstraintName,
                IsExpectedBusinessFailure: false)
        };
    }

    private static string GetDatabaseFailureReason(PostgresException? postgresException)
    {
        if (postgresException is null)
            return "Database update failed while saving order data.";

        return postgresException.SqlState switch
        {
            PostgresErrorCodes.UniqueViolation => $"Duplicate value violated unique constraint {postgresException.ConstraintName}.",
            PostgresErrorCodes.ForeignKeyViolation => $"Related database row was missing for foreign key constraint {postgresException.ConstraintName}.",
            PostgresErrorCodes.NotNullViolation => $"Required database column was null for constraint {postgresException.ConstraintName}.",
            PostgresErrorCodes.CheckViolation => $"Database check constraint {postgresException.ConstraintName} failed.",
            _ => $"PostgreSQL error {postgresException.SqlState} while saving order data."
        };
    }

    private static PostgresException? FindPostgresException(Exception ex)
    {
        if (ex is PostgresException postgresException)
            return postgresException;

        return ex.InnerException is null ? null : FindPostgresException(ex.InnerException);
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

    private sealed record OrderCreationFailureDetails(
        string Category,
        string Reason,
        string ExceptionType,
        int HttpStatus,
        string? DbSqlState,
        string? DbConstraint,
        bool IsExpectedBusinessFailure);
}
