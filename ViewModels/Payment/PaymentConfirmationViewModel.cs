using MyEBookLibrary.Models;
using MyEBookLibrary.ViewModels.Payment;

public class PaymentConfirmationViewModel : PaymentBaseViewModel
{
    public string? TransactionId { get; set; }
    public PaymentStatus Status { get; set; }
    public DateTime PurchaseDate { get; set; }
    public string? ReceiptUrl { get; set; }
    public List<string> DownloadLinks { get; set; } = [];  // קישורים להורדת ספרים
    public Dictionary<int, DateTime> BorrowDueDates { get; set; } = [];  // תאריכי החזרה
}