using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend.Models.Orders;
using backend.Models.Products;

namespace backend.Models.Wishlists;

[Table("WishlistItems")]
public class WishlistItem
{
    [Key]
    [Column(TypeName = "varchar")]
    public string WishlistItemId { get; set; } = PrefixedId.Create("wishlist_items");

    [Required]
    [Column(TypeName = "varchar")]
    public string WishlistId { get; set; } = string.Empty;

    public int ProductId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Wishlist Wishlist { get; set; } = null!;

    [ForeignKey(nameof(ProductId))]
    public Product Product { get; set; } = null!;
}
