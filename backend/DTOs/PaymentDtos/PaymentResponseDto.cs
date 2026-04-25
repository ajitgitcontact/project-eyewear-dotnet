using backend.Models.Orders;

namespace backend.DTOs.PaymentDtos;

public class PaymentResponseDto
{
    public string PaymentsId { get; set; } = string.Empty;
    public string CustomerOrderId { get; set; } = string.Empty;
    public PaymentMethod Method { get; set; }
    public string? TransactionId { get; set; }
    public decimal Amount { get; set; }
    public PaymentTxnStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
