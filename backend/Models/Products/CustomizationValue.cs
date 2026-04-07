using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models.Products;

[Table("CustomizationValues")]
public class CustomizationValue
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int CustomizationValueId { get; set; }

    [Required]
    public int CustomizationOptionId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Value { get; set; } = string.Empty;

    [Column(TypeName = "decimal(10,2)")]
    public decimal AdditionalPrice { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(CustomizationOptionId))]
    public CustomizationOption CustomizationOption { get; set; } = null!;
}
