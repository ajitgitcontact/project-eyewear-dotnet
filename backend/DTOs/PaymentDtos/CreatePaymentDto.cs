using System.ComponentModel.DataAnnotations;
using backend.Models.Orders;

namespace backend.DTOs.PaymentDtos;

public class CreatePaymentDto
{
    [Required]
    [MaxLength(50)]
    public string CustomerOrderId { get; set; } = string.Empty;

    public PaymentMethod Method { get; set; }

    [MaxLength(150)]
    public string? TransactionId { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Amount { get; set; }

    public PaymentTxnStatus Status { get; set; } = PaymentTxnStatus.INITIATED;
}
