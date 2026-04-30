using System.Text.RegularExpressions;
using backend.Application.Abstractions.Orders;
using backend.Application.Exceptions;
using backend.Constants;
using backend.DTOs.CustomerPrescriptionDtos;
using backend.DTOs.OrderAddressDtos;
using backend.DTOs.OrderFetchDtos;
using backend.DTOs.OrderItemCustomizationDtos;
using backend.DTOs.OrderItemDtos;
using backend.DTOs.OrderStatusLogDtos;
using backend.DTOs.PaymentDtos;

namespace backend.Infrastructure.Services.Orders;

public class FetchCompleteOrderService : IFetchCompleteOrderService
{
    private static readonly Regex CustomerOrderIdRegex = new(@"^\d{11}$", RegexOptions.Compiled);

    private readonly IOrderService _orderService;
    private readonly IOrderItemService _orderItemService;
    private readonly IOrderItemCustomizationService _customizationService;
    private readonly IOrderAddressService _addressService;
    private readonly ICustomerPrescriptionService _prescriptionService;
    private readonly IPaymentService _paymentService;
    private readonly IOrderStatusLogService _statusLogService;
    private readonly ILogger<FetchCompleteOrderService> _logger;

    public FetchCompleteOrderService(
        IOrderService orderService,
        IOrderItemService orderItemService,
        IOrderItemCustomizationService customizationService,
        IOrderAddressService addressService,
        ICustomerPrescriptionService prescriptionService,
        IPaymentService paymentService,
        IOrderStatusLogService statusLogService,
        ILogger<FetchCompleteOrderService> logger)
    {
        _orderService = orderService;
        _orderItemService = orderItemService;
        _customizationService = customizationService;
        _addressService = addressService;
        _prescriptionService = prescriptionService;
        _paymentService = paymentService;
        _statusLogService = statusLogService;
        _logger = logger;
    }

    public async Task<CompleteOrderResponseDto> GetByCustomerOrderIdAsync(string customerOrderId, int userId, string role)
    {
        var normalizedCustomerOrderId = NormalizeCustomerOrderId(customerOrderId);
        _logger.LogInformation(
            "Complete order lookup started. CustomerOrderId={CustomerOrderId}, UserId={UserId}, Role={Role}",
            normalizedCustomerOrderId,
            userId,
            role);

        var order = await _orderService.GetByCustomerOrderIdAsync(normalizedCustomerOrderId);

        var isAdmin = role == Roles.Admin || role == Roles.SuperAdmin;
        if (!isAdmin && role != Roles.Customer)
        {
            _logger.LogWarning("Complete order authorization failed. Unsupported role. CustomerOrderId={CustomerOrderId}, UserId={UserId}, Role={Role}", normalizedCustomerOrderId, userId, role);
            throw new ForbiddenException("You do not have permission to access this order.");
        }

        if (role == Roles.Customer && order.UserId != userId)
        {
            _logger.LogWarning("Complete order authorization failed. CustomerOrderId={CustomerOrderId}, UserId={UserId}, OrderUserId={OrderUserId}", normalizedCustomerOrderId, userId, order.UserId);
            throw new ForbiddenException("You do not have permission to access this order.");
        }

        _logger.LogInformation("Complete order authorization succeeded. CustomerOrderId={CustomerOrderId}, UserId={UserId}, Role={Role}", normalizedCustomerOrderId, userId, role);

        var itemDtos = (await _orderItemService.GetByCustomerOrderIdAsync(normalizedCustomerOrderId)).ToList();
        var items = new List<CompleteOrderItemDto>();
        foreach (var item in itemDtos)
        {
            var customizations = (await _customizationService.GetByOrderItemIdAsync(item.OrderItemsId)).ToList();
            items.Add(MapItem(item, customizations));
        }

        var addresses = (await _addressService.GetByCustomerOrderIdAsync(normalizedCustomerOrderId)).Select(MapAddress).ToList();
        var prescriptions = (await _prescriptionService.GetByCustomerOrderIdAsync(normalizedCustomerOrderId)).Select(MapPrescription).ToList();
        var payments = (await _paymentService.GetByCustomerOrderIdAsync(normalizedCustomerOrderId)).ToList();
        var statusLogs = (await _statusLogService.GetByCustomerOrderIdAsync(normalizedCustomerOrderId)).Select(MapStatusLog).ToList();

        _logger.LogInformation(
            "Complete order data fetched. CustomerOrderId={CustomerOrderId}, ItemCount={ItemCount}, AddressCount={AddressCount}, PaymentCount={PaymentCount}, PrescriptionCount={PrescriptionCount}, StatusLogCount={StatusLogCount}",
            normalizedCustomerOrderId,
            items.Count,
            addresses.Count,
            payments.Count,
            prescriptions.Count,
            statusLogs.Count);

        var response = new CompleteOrderResponseDto
        {
            Order = new CompleteOrderSummaryDto
            {
                OrdersId = order.OrdersId,
                CustomerOrderId = order.CustomerOrderId,
                UserId = order.UserId,
                CustomerName = order.CustomerName,
                CustomerEmail = order.CustomerEmail,
                CustomerPhone = order.CustomerPhone,
                TotalAmount = order.TotalAmount,
                PaymentStatus = order.PaymentStatus,
                OrderStatus = order.OrderStatus,
                Notes = order.Notes,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt
            },
            Items = items,
            Addresses = addresses,
            Prescriptions = prescriptions,
            StatusLogs = statusLogs
        };

        if (isAdmin)
            response.AdminPayments = payments.Select(MapAdminPayment).ToList();
        else
            response.CustomerPayments = payments.Select(MapCustomerPayment).ToList();

        _logger.LogInformation("Complete order lookup succeeded. CustomerOrderId={CustomerOrderId}, UserId={UserId}, Role={Role}", normalizedCustomerOrderId, userId, role);
        return response;
    }

    private static string NormalizeCustomerOrderId(string customerOrderId)
    {
        var normalized = customerOrderId?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalized))
            throw new BadRequestException("CustomerOrderId is required.");

        if (!CustomerOrderIdRegex.IsMatch(normalized))
            throw new BadRequestException("CustomerOrderId must match YYMMDDXXXXX format.");

        return normalized;
    }

    private static CompleteOrderItemDto MapItem(OrderItemResponseDto item, List<OrderItemCustomizationResponseDto> customizations)
    {
        return new CompleteOrderItemDto
        {
            OrderItemsId = item.OrderItemsId,
            CustomerOrderId = item.CustomerOrderId,
            ProductId = item.ProductId,
            SKU = item.SKU,
            ProductName = item.ProductName,
            Quantity = item.Quantity,
            Price = item.Price,
            TotalPrice = item.TotalPrice,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt,
            Customizations = customizations.Select(c => new CompleteOrderCustomizationDto
            {
                OrderItemCustomizationsId = c.OrderItemCustomizationsId,
                OrderItemId = c.OrderItemId,
                CustomizationOptionId = c.CustomizationOptionId,
                CustomizationValueId = c.CustomizationValueId,
                Type = c.Type,
                Value = c.Value,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            }).ToList()
        };
    }

    private static CompleteOrderAddressDto MapAddress(OrderAddressResponseDto address)
    {
        return new CompleteOrderAddressDto
        {
            OrderAddressesId = address.OrderAddressesId,
            CustomerOrderId = address.CustomerOrderId,
            Type = address.Type,
            Line1 = address.Line1,
            Line2 = address.Line2,
            City = address.City,
            State = address.State,
            Pincode = address.Pincode,
            Country = address.Country,
            CreatedAt = address.CreatedAt,
            UpdatedAt = address.UpdatedAt
        };
    }

    private static CompleteOrderPrescriptionDto MapPrescription(CustomerPrescriptionResponseDto prescription)
    {
        return new CompleteOrderPrescriptionDto
        {
            CustomerPrescriptionsId = prescription.CustomerPrescriptionsId,
            UserId = prescription.UserId,
            CustomerOrderId = prescription.CustomerOrderId,
            RightSphere = prescription.RightSphere,
            RightCylinder = prescription.RightCylinder,
            RightAxis = prescription.RightAxis,
            RightAdd = prescription.RightAdd,
            LeftSphere = prescription.LeftSphere,
            LeftCylinder = prescription.LeftCylinder,
            LeftAxis = prescription.LeftAxis,
            LeftAdd = prescription.LeftAdd,
            PD = prescription.PD,
            Notes = prescription.Notes,
            CreatedAt = prescription.CreatedAt,
            UpdatedAt = prescription.UpdatedAt
        };
    }

    private static CustomerSafePaymentDto MapCustomerPayment(PaymentResponseDto payment)
    {
        return new CustomerSafePaymentDto
        {
            Method = payment.Method,
            Status = payment.Status,
            Amount = payment.Amount,
            CreatedAt = payment.CreatedAt
        };
    }

    private static AdminPaymentDto MapAdminPayment(PaymentResponseDto payment)
    {
        return new AdminPaymentDto
        {
            PaymentsId = payment.PaymentsId,
            CustomerOrderId = payment.CustomerOrderId,
            Method = payment.Method,
            TransactionId = payment.TransactionId,
            Amount = payment.Amount,
            Status = payment.Status,
            CreatedAt = payment.CreatedAt,
            UpdatedAt = payment.UpdatedAt
        };
    }

    private static CompleteOrderStatusLogDto MapStatusLog(OrderStatusLogResponseDto log)
    {
        return new CompleteOrderStatusLogDto
        {
            OrderStatusLogsId = log.OrderStatusLogsId,
            CustomerOrderId = log.CustomerOrderId,
            Status = log.Status,
            Comment = log.Comment,
            CreatedAt = log.CreatedAt,
            UpdatedAt = log.UpdatedAt
        };
    }
}
