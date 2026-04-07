using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models.Products;

[Table("CustomizationImages")]
public class CustomizationImage
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int CustomizationImageId { get; set; }

    [Required]
    public int ProductId { get; set; }

    [Required]
    public int CustomizationOptionId { get; set; }

    [Required]
    public int CustomizationValueId { get; set; }

    [Required]
    [MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(ProductId))]
    public Product Product { get; set; } = null!;

    [ForeignKey(nameof(CustomizationOptionId))]
    public CustomizationOption CustomizationOption { get; set; } = null!;

    [ForeignKey(nameof(CustomizationValueId))]
    public CustomizationValue CustomizationValue { get; set; } = null!;
}
