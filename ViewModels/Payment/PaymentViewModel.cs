using System.ComponentModel.DataAnnotations;
using MyEBookLibrary.Models;

namespace MyEBookLibrary.ViewModels.Payment
{
    public abstract class PaymentBaseViewModel
    {
        [Display(Name = "סכום לתשלום")]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        public List<CartItem> Items { get; set; } = [];

        public bool IsSuccess { get; set; }

        public string? ErrorMessage { get; set; }

        [Required(ErrorMessage = "יש לבחור אמצעי תשלום")]
        [Display(Name = "אמצעי תשלום")]
        public PaymentMethod PaymentMethod { get; set; }

        // פרטי הזמנה
        [Display(Name = "מספר הזמנה")]
        public string OrderId { get; set; } = Guid.NewGuid().ToString();

        [Display(Name = "תאריך הזמנה")]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        // מאפיינים מחושבים
        private decimal _subtotal;
        public decimal Subtotal
        {
            get => _subtotal;
            set => _subtotal = value;
        }

        private decimal _total;
        public decimal Total
        {
            get => _total;
            set => _total = value;
        }

        // עדכון הסכומים
        public void UpdateTotals()
        {
            Subtotal = Items.Sum(item => item.Price * item.Quantity);
            Total = Subtotal; // אפשר להוסיף כאן גם חישובי מע"מ והנחות
        }
        [Display(Name = "מע\"מ")]
        public decimal VAT => Subtotal * 0.17m;

        [Display(Name = "הנחה")]
        public decimal Discount { get; set; }

        [Display(Name = "סה\"כ לתשלום")]

        public int ItemCount => Items.Count;

        public int TotalQuantity => Items.Sum(item => item.Quantity);

        public bool HasErrors => !string.IsNullOrEmpty(ErrorMessage);

        // פרטי פריטים לפי סוג
        public IEnumerable<CartItem> BorrowedItems =>
            Items.Where(item => item.IsBorrow);

        public IEnumerable<CartItem> PurchasedItems =>
            Items.Where(item => !item.IsBorrow);

        public int BorrowedItemsCount => BorrowedItems.Count();

        public int PurchasedItemsCount => PurchasedItems.Count();

        [Display(Name = "סכום השאלות")]
        public decimal BorrowTotal =>
            BorrowedItems.Sum(item => item.Price * item.Quantity);

        [Display(Name = "סכום רכישות")]
        public decimal PurchaseTotal =>
            PurchasedItems.Sum(item => item.Price * item.Quantity);

        // וולידציה
        public virtual bool Validate()
        {
            if (!Items.Any())
            {
                ErrorMessage = "העגלה ריקה";
                return false;
            }

            // בדיקת הגבלת השאלות
            if (BorrowedItems.Any())
            {
                var borrowGroups = BorrowedItems.GroupBy(item => item.BookId);
                foreach (var group in borrowGroups)
                {
                    if (group.Sum(item => item.Quantity) > 3)
                    {
                        ErrorMessage = $"לא ניתן לשאול יותר מ-3 עותקים של אותו ספר";
                        return false;
                    }
                }
            }

            return true;
        }

        // סטטוסים שונים להצגה
        public bool ShowVAT => Total > 0;
        public bool HasDiscount => Discount > 0;
        public bool IsMixedOrder => BorrowedItemsCount > 0 && PurchasedItemsCount > 0;
        public bool RequiresShipping => false; // ספרים דיגיטליים בלבד
    }

    public class PaymentConfirmationViewModel : PaymentBaseViewModel
    {
        public string? TransactionId { get; set; }
        public DateTime ProcessedAt { get; set; }
        public string? ReceiptUrl { get; set; }
        public PaymentStatus Status { get; set; }
        public DateTime PurchaseDate { get; set; }

        public List<string> DownloadLinks { get; set; } = [];
        public Dictionary<int, DateTime> BorrowDueDates { get; set; } = [];

        public bool IsFullyProcessed => Status == PaymentStatus.Captured;
        public bool NeedsRetry => Status == PaymentStatus.Failed;
        public bool CanDownload => IsFullyProcessed && DownloadLinks.Any();
    }

    public class CheckoutViewModel : PaymentBaseViewModel
    {
        [Required(ErrorMessage = "יש להזין פרטי כרטיס אשראי")]
        public required CreditCardDetails CardDetails { get; set; }

        [Display(Name = "שמור כרטיס לרכישות הבאות")]
        public bool SaveCard { get; set; }

        public string? PromoCode { get; set; }

        [Display(Name = "אני מאשר/ת את תנאי השימוש")]
        [Range(typeof(bool), "true", "true", ErrorMessage = "יש לאשר את תנאי השימוש")]
        public bool AcceptTerms { get; set; }

        public override bool Validate()
        {
            if (!base.Validate()) return false;

            if (!AcceptTerms)
            {
                ErrorMessage = "יש לאשר את תנאי השימוש";
                return false;
            }

            return true;
        }
    }

    public class PaymentFailureViewModel : PaymentBaseViewModel
    {
        public string? FailureReason { get; set; }
        public bool CanRetry { get; set; }
        public string? RetryUrl { get; set; }
        public List<string> SuggestedActions { get; set; } = [];
    }

    public class CreditCardDetails
    {
        [Required(ErrorMessage = "יש להזין מספר כרטיס")]
        [CreditCard(ErrorMessage = "מספר כרטיס לא תקין")]
        [Display(Name = "מספר כרטיס")]
        public string CardNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "יש להזין חודש תפוגה")]
        [RegularExpression(@"^(0[1-9]|1[0-2])$", ErrorMessage = "חודש תפוגה לא תקין")]
        [Display(Name = "חודש תפוגה")]
        public string ExpiryMonth { get; set; } = string.Empty;

        [Required(ErrorMessage = "יש להזין שנת תפוגה")]
        [RegularExpression(@"^[0-9]{2}$", ErrorMessage = "שנת תפוגה לא תקינה")]
        [Display(Name = "שנת תפוגה")]
        public string ExpiryYear { get; set; } = string.Empty;

        [Required(ErrorMessage = "יש להזין קוד אבטחה")]
        [RegularExpression(@"^[0-9]{3,4}$", ErrorMessage = "קוד אבטחה לא תקין")]
        [Display(Name = "קוד אבטחה (CVV)")]
        public string CVV { get; set; } = string.Empty;

        [Required(ErrorMessage = "יש להזין שם בעל הכרטיס")]
        [Display(Name = "שם בעל הכרטיס")]
        public string CardHolderName { get; set; } = string.Empty;

        public bool IsExpired()
        {
            if (int.TryParse(ExpiryYear, out int year) &&
                int.TryParse(ExpiryMonth, out int month))
            {
                var expiryDate = new DateTime(2000 + year, month, 1).AddMonths(1).AddDays(-1);
                return expiryDate < DateTime.UtcNow;
            }
            return true;
        }
    }
}