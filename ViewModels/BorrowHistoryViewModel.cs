
namespace MyEBookLibrary.ViewModels
{
    public class BorrowHistoryViewModel
    {
        public int BorrowId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string BookTitle { get; set; } = string.Empty;
        public DateTime BorrowDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public bool IsReturned { get; set; }
        public bool IsLate { get; set; }
        public decimal FineAmount { get; set; }
        
        // Number of days until return
        public int DaysUntilReturn => (ReturnDate.HasValue ? (ReturnDate.Value - DateTime.Now).Days : 0);
        
        // Additional helper properties
        public bool IsOverdue => !IsReturned && BorrowDate.AddDays(14) < DateTime.Now;
        public string Status => IsReturned ? "הוחזר" : (IsLate ? "באיחור" : "פעיל");
    }
}