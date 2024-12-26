// ViewModels/Payment/PaymentViewModels.cs
using System.ComponentModel.DataAnnotations;
using MyEBookLibrary.Models;

namespace MyEBookLibrary.ViewModels.Payment
{
    public class CreditCardDetails
    {
        [Required(ErrorMessage = "נדרש מספר כרטיס")]
        [Display(Name = "מספר כרטיס")]
        public string CardNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "נדרש חודש תפוגה")]
        [RegularExpression(@"^(0[1-9]|1[0-2])$", ErrorMessage = "חודש תפוגה לא תקין")]
        [Display(Name = "חודש תפוגה")]
        public string ExpiryMonth { get; set; } = string.Empty;

        [Required(ErrorMessage = "נדרשת שנת תפוגה")]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "שנת תפוגה לא תקינה")]
        [Display(Name = "שנת תפוגה")]
        public string ExpiryYear { get; set; } = string.Empty;

        [Required(ErrorMessage = "נדרש קוד אבטחה")]
        [RegularExpression(@"^\d{3,4}$", ErrorMessage = "קוד אבטחה לא תקין")]
        [Display(Name = "קוד אבטחה")]
        public string CVV { get; set; } = string.Empty;

        [Required(ErrorMessage = "נדרש שם בעל הכרטיס")]
        [Display(Name = "שם בעל הכרטיס")]
        public string CardHolderName { get; set; } = string.Empty;
        public string? StripeToken { get; internal set; }
    }

    public class CheckoutViewModel
    {
        public decimal Total => Items.Sum(item => item.Subtotal);

        public List<CartItem> Items { get; set; } = new();
        public CreditCardDetails CardDetails { get; set; } = new();
        public PaymentMethod PaymentMethod { get; set; }
    }

    public class PaymentConfirmationViewModel
    {
        public decimal Amount { get; set; }
        public List<CartItem> PurchasedItems { get; set; } = new();
        public string TransactionId { get; set; } = string.Empty;
        public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;
    }

    public class PaymentResultViewModel
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string? RedirectUrl { get; set; }
    }
}