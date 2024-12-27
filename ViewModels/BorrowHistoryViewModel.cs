using System.ComponentModel.DataAnnotations;
using MyEBookLibrary.Models;

namespace MyEBookLibrary.ViewModels
{
    public class BorrowHistoryViewModel
    {
        public int BorrowId { get; set; }

        [Display(Name = "שם משתמש")]
        public string UserName { get; set; } = string.Empty;

        [Display(Name = "שם הספר")]
        public string BookTitle { get; set; } = string.Empty;

        [Display(Name = "מחבר")]
        public string Author { get; set; } = string.Empty;

        [Display(Name = "תאריך השאלה")]
        [DataType(DataType.Date)]
        public DateTime BorrowDate { get; set; }

        [Display(Name = "תאריך החזרה")]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        [Display(Name = "הוחזר בתאריך")]
        [DataType(DataType.Date)]
        public DateTime? ReturnDate { get; set; }

        public bool IsReturned { get; set; }
        public bool IsLate { get; set; }

        [Display(Name = "קנס")]
        [DataType(DataType.Currency)]
        public decimal FineAmount { get; set; }

        [Display(Name = "פורמט")]
        public BookFormat Format { get; set; }

        public string CoverImageUrl { get; set; } = string.Empty;

        // מאפיינים מחושבים
        [Display(Name = "ימים שנותרו")]
        public int DaysRemaining => ReturnDate.HasValue ? 0 : (DueDate - DateTime.Now).Days;

        [Display(Name = "ימי איחור")]
        public int DaysOverdue => IsOverdue ? (DateTime.Now - DueDate).Days : 0;

        public bool IsOverdue => !IsReturned && DateTime.Now > DueDate;

        public bool NeedsReminder => !IsReturned && DaysRemaining <= 5 && DaysRemaining > 0;

        [Display(Name = "סטטוס")]
        public string Status => GetStatus();

        [Display(Name = "צבע סטטוס")]
        public string StatusColor => GetStatusColor();

        private string GetStatus()
        {
            if (IsReturned)
                return "הוחזר";
            if (IsOverdue)
                return $"באיחור של {DaysOverdue} ימים";
            if (DaysRemaining <= 5)
                return $"להחזרה בעוד {DaysRemaining} ימים";
            return "פעיל";
        }

        private string GetStatusColor()
        {
            if (IsReturned)
                return "text-success";
            if (IsOverdue)
                return "text-danger";
            if (DaysRemaining <= 5)
                return "text-warning";
            return "text-primary";
        }

        // פרטים נוספים על הספר
        public int BookId { get; set; }
        public string Genre { get; set; } = string.Empty;
        public string Publisher { get; set; } = string.Empty;
        public int YearOfPublication { get; set; }

        // פרטי המשתמש
        public int UserId { get; set; }
        public string UserEmail { get; set; } = string.Empty;

        // פעולות אפשריות
        public bool CanExtend => !IsReturned && !IsOverdue && DaysRemaining <= 10;
        public bool CanReturn => !IsReturned;
        public bool CanReview => IsReturned && !HasReview;
        public bool HasReview { get; set; }

        // היסטוריית הארכות
        public List<ExtensionHistory> Extensions { get; set; } = [];

        // היסטוריית התראות
        public List<ReminderHistory> Reminders { get; set; } = [];
    }

    public class ExtensionHistory
    {
        public DateTime ExtensionDate { get; set; }
        public DateTime OldDueDate { get; set; }
        public DateTime NewDueDate { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string ApprovedBy { get; set; } = string.Empty;
    }

    public class ReminderHistory
    {
        public DateTime SentDate { get; set; }
        public string Type { get; set; } = string.Empty;
        public bool WasRead { get; set; }
        public DateTime? ReadDate { get; set; }
    }

    public class BorrowHistoryFilterViewModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool? IsReturned { get; set; }
        public bool? IsOverdue { get; set; }
        public string? SearchTerm { get; set; }
        public BookFormat? Format { get; set; }
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class BorrowHistoryPageViewModel
    {
        public List<BorrowHistoryViewModel> Items { get; set; } = [];
        public BorrowHistoryFilterViewModel Filter { get; set; } = new();
        
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalItems / (double)Filter.PageSize);
        
        public BorrowStatistics Statistics { get; set; } = new();
    }

    public class BorrowStatistics
    {
        public int TotalBorrows { get; set; }
        public int ActiveBorrows { get; set; }
        public int OverdueBorrows { get; set; }
        public decimal TotalFines { get; set; }
        public int ReturnedOnTime { get; set; }
        public int ReturnedLate { get; set; }
        public double OnTimeReturnRate => TotalBorrows == 0 ? 0 : (ReturnedOnTime * 100.0 / TotalBorrows);
        public Dictionary<BookFormat, int> BorrowsByFormat { get; set; } = [];
        public Dictionary<string, int> BorrowsByGenre { get; set; } = [];
    }
}