using System.ComponentModel.DataAnnotations;
using backend.Models.Orders;

namespace backend.DTOs.PaymentDtos;

public class UpdatePaymentDto
{
    public PaymentMethod Method { get; set; }

    [MaxLength(150)]
    public string? TransactionId { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Amount { get; set; }

    public PaymentTxnStatus Status { get; set; }
}
