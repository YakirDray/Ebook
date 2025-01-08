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

        [Display(Name = "תאריך השאלה"), DataType(DataType.Date)]
        public DateTime BorrowDate { get; set; }

        [Display(Name = "תאריך החזרה"), DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        [Display(Name = "הוחזר בתאריך"), DataType(DataType.Date)]
        public DateTime? ReturnDate { get; set; }

        public bool IsReturned { get; set; }
        public bool IsLate { get; set; }

        [Display(Name = "קנס"), DataType(DataType.Currency)]
        public decimal FineAmount { get; set; }

        [Display(Name = "פורמט")]
        public BookFormat Format { get; set; }

        public string CoverImageUrl { get; set; } = string.Empty;

        // חישוב ימים שנותרים עד החזרת הספר
        [Display(Name = "ימים שנותרו")]
        public int DaysRemaining => ReturnDate.HasValue ? 0 : (DueDate - DateTime.Now).Days;

        // חישוב ימי איחור
        [Display(Name = "ימי איחור")]
        public int DaysOverdue => IsOverdue ? (DateTime.Now - DueDate).Days : 0;

        // בדיקה האם הספר באיחור
        public bool IsOverdue => !IsReturned && DateTime.Now > DueDate;

        // הצגת סטטוס ההשאלה
        [Display(Name = "סטטוס")]
        public string Status => GetStatus();

        // הצגת צבע עבור סטטוס
        [Display(Name = "צבע סטטוס")]
        public string StatusColor => GetStatusColor();

        private string GetStatus()
        {
            if (IsReturned) return "הוחזר";
            if (IsOverdue) return $"באיחור של {DaysOverdue} ימים";
            if (DaysRemaining <= 5) return $"להחזרה בעוד {DaysRemaining} ימים";
            return "פעיל";
        }

        private string GetStatusColor()
        {
            if (IsReturned) return "text-success";
            if (IsOverdue) return "text-danger";
            if (DaysRemaining <= 5) return "text-warning";
            return "text-primary";
        }
    }
}
