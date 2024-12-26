using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyEBookLibrary.Models
{
    public class CartItem
    {
        public int Id { get; set; }

        public int BookId { get; set; }

        [Required]
        [StringLength(200)]
        public required string Title { get; set; }

        public decimal Price { get; set; }
        public BookFormat Format { get; set; }
        public bool IsBorrow { get; set; }
        public int Quantity { get; set; }

        [ForeignKey("BookId")]
        public virtual required Book Book { get; set; }

        [NotMapped]
        public decimal Subtotal => Price * Quantity;
    }

    public class ShoppingCart
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        public List<CartItem> Items { get; set; } = new();

        [NotMapped]
        public decimal Total => Items.Sum(item => item.Subtotal);

        public CartStatus Status { get; set; } = CartStatus.Active;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CheckoutAt { get; set; }
    }
}