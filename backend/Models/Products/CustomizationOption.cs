using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models.Products;

[Table("CustomizationOptions")]
public class CustomizationOption
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int CustomizationOptionId { get; set; }

    [Required]
    public int ProductId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public bool IsRequired { get; set; }

    public int DisplayOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(ProductId))]
    public Product Product { get; set; } = null!;

    public ICollection<CustomizationValue> CustomizationValues { get; set; } = new List<CustomizationValue>();
}
