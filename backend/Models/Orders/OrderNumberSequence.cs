using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models.Orders;

[Table("OrderNumberSequences")]
public class OrderNumberSequence
{
    [Key]
    [Column(TypeName = "varchar")]
    public string OrderNumberSequencesId { get; set; } = PrefixedId.Create("order_number_sequences");

    [Column(TypeName = "date")]
    public DateOnly SequenceDate { get; set; }

    public int LastSequenceNumber { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
