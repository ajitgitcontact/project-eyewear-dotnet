using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models.Orders;

[Table("Payments")]
public class Payment
{
    [Key]
    [Column(TypeName = "varchar")]
    public string PaymentsId { get; set; } = PrefixedId.Create("payments");

    [Required]
    [MaxLength(50)]
    [Column(TypeName = "varchar(50)")]
    public string CustomerOrderId { get; set; } = string.Empty;

    [Column(TypeName = "payment_method")]
    public PaymentMethod Method { get; set; }

    [MaxLength(150)]
    public string? TransactionId { get; set; }

    [Column(TypeName = "numeric(10,2)")]
    public decimal Amount { get; set; }

    [Column(TypeName = "payment_txn_status")]
    public PaymentTxnStatus Status { get; set; } = PaymentTxnStatus.INITIATED;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Order Order { get; set; } = null!;
}
