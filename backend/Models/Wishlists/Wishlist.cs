using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend.Models;
using backend.Models.Orders;

namespace backend.Models.Wishlists;

[Table("Wishlists")]
public class Wishlist
{
    [Key]
    [Column(TypeName = "varchar")]
    public string WishlistId { get; set; } = PrefixedId.Create("wishlists");

    public int UserId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    public ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
}
