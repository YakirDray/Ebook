
using System.ComponentModel.DataAnnotations;
namespace MyEBookLibrary.Models;
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
    public required string StripeToken { get; set; }

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
