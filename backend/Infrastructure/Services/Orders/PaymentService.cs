using backend.Application.Abstractions.Orders;
using backend.Application.Exceptions;
using backend.Data;
using backend.DTOs.PaymentDtos;
using backend.Models.Orders;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Services.Orders;

public class PaymentService : IPaymentService
{
    private readonly AppDbContext _context;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(AppDbContext context, ILogger<PaymentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<PaymentResponseDto>> GetByCustomerOrderIdAsync(string customerOrderId)
    {
        _logger.LogInformation("Fetching payments. Input: CustomerOrderId={CustomerOrderId}", customerOrderId);
        var payments = await _context.Payments
            .Where(p => p.CustomerOrderId == customerOrderId)
            .Select(p => MapToDto(p))
            .ToListAsync();
        _logger.LogInformation("Fetched payments. Input: CustomerOrderId={CustomerOrderId} => Output: Count={Count}", customerOrderId, payments.Count);
        return payments;
    }

    public async Task<PaymentResponseDto> GetByIdAsync(string id)
    {
        _logger.LogInformation("Fetching payment by id. Input: PaymentsId={PaymentsId}", id);
        var payment = await _context.Payments.FindAsync(id);
        if (payment is null)
            throw new NotFoundException("Payment not found.");

        _logger.LogInformation("Get payment by id result. Input: PaymentsId={PaymentsId} => Output: Found=true", id);
        return MapToDto(payment);
    }

    public async Task<PaymentResponseDto> CreateAsync(CreatePaymentDto dto)
    {
        _logger.LogInformation("Creating payment. Input: CustomerOrderId={CustomerOrderId}, Method={Method}, Amount={Amount}, Status={Status}", dto.CustomerOrderId, dto.Method, dto.Amount, dto.Status);
        await ValidateOrderExistsAsync(dto.CustomerOrderId);

        var payment = new Payment
        {
            CustomerOrderId = dto.CustomerOrderId,
            Method = dto.Method,
            TransactionId = dto.TransactionId,
            Amount = dto.Amount,
            Status = dto.Status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Payment created. Output: PaymentsId={PaymentsId}", payment.PaymentsId);
        return MapToDto(payment);
    }

    public async Task<PaymentResponseDto> UpdateAsync(string id, UpdatePaymentDto dto)
    {
        _logger.LogInformation("Updating payment. Input: PaymentsId={PaymentsId}, Method={Method}, Amount={Amount}, Status={Status}", id, dto.Method, dto.Amount, dto.Status);
        var payment = await _context.Payments.FindAsync(id);
        if (payment is null)
            throw new NotFoundException("Payment not found.");

        payment.Method = dto.Method;
        payment.TransactionId = dto.TransactionId;
        payment.Amount = dto.Amount;
        payment.Status = dto.Status;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Payment updated. Input: PaymentsId={PaymentsId}", id);
        return MapToDto(payment);
    }

    public async Task DeleteAsync(string id)
    {
        _logger.LogInformation("Deleting payment. Input: PaymentsId={PaymentsId}", id);
        var payment = await _context.Payments.FindAsync(id);
        if (payment is null)
            throw new NotFoundException("Payment not found.");

        _context.Payments.Remove(payment);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Delete payment result. Input: PaymentsId={PaymentsId} => Output: Deleted=true", id);
    }

    private async Task ValidateOrderExistsAsync(string customerOrderId)
    {
        var exists = await _context.Orders.AnyAsync(o => o.CustomerOrderId == customerOrderId);
        if (!exists)
            throw new NotFoundException("Order not found.");
    }

    private static PaymentResponseDto MapToDto(Payment payment)
    {
        return new PaymentResponseDto
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
}
