using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models.Products;

[Table("Products")]
public class Product
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ProductId { get; set; }

    [Required]
    [MaxLength(50)]
    public string SKU { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [MaxLength(100)]
    public string? Brand { get; set; }

    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal BasePrice { get; set; }

    public bool HasPrescription { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public ICollection<CustomizationOption> CustomizationOptions { get; set; } = new List<CustomizationOption>();
    public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
    public ICollection<CustomizationImage> CustomizationImages { get; set; } = new List<CustomizationImage>();
}
