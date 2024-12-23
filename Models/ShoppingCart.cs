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
        public decimal Subtotal { get; internal set; }
    }

    public class ShoppingCart
    {
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        public List<CartItem> Items { get; set; } = new();
        public decimal Total => Items.Sum(item => item.Subtotal);
        public CartStatus Status { get; set; } = CartStatus.Active;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CheckoutAt { get; set; }
    }

    public class PaymentInfo
    {
        public required string UserId { get; set; }

        [Required]
        [CreditCard]
        public string CardNumber { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^(0[1-9]|1[0-2])$")]
        public string ExpiryMonth { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^[0-9]{2}$")]
        public string ExpiryYear { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^[0-9]{3,4}$")]
        public string CVV { get; set; } = string.Empty;

        [Required]
        public string CardHolderName { get; set; } = string.Empty;

        [Range(0, 100000)]
        public decimal Amount { get; set; }

        [Required]
        public string Currency { get; set; } = "ILS";

        public PaymentMethod Method { get; set; }
        public PaymentStatus Status { get; set; }

    }

    public enum PaymentMethod
    {
        CreditCard,
        PayPal,
        BitCoin
    }

    public enum CartStatus
    {
        Active,
        CheckedOut,
        Abandoned,
        Completed
    }

    public enum PaymentStatus
    {
        Pending,
        Authorized,
        Captured,
        Failed,
        Refunded
    }

    public class PaymentResult
    {
        public bool Success { get; set; }
        public string? TransactionId { get; set; }
        public string? ErrorMessage { get; set; }
        public PaymentStatus Status { get; set; }
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    }
}