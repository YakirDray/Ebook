using System.ComponentModel.DataAnnotations;
using MyEBookLibrary.Models;

namespace MyEBookLibrary.ViewModels
{
    public class WaitingListManagementViewModel
    {
        public int BookId { get; set; }

        [Required(ErrorMessage = "נדרש שם ספר")]
        public string BookTitle { get; set; } = string.Empty;

        public List<WaitingListItemViewModel> WaitingUsers { get; set; } = [];

        [Range(0, 3, ErrorMessage = "מספר העותקים הזמינים חייב להיות בין 0 ל-3")]
        public int AvailableCopies { get; set; }

        public DateTime? NextAvailableDate { get; set; }

        // מאפיינים מחושבים
        public int TotalWaitingUsers => WaitingUsers.Count;
        public int NotifiedUsersCount => WaitingUsers.Count(u => u.IsNotified);
        public bool HasAvailableCopies => AvailableCopies > 0;
        public bool HasWaitingList => WaitingUsers.Any();
        public TimeSpan? EstimatedWaitTime => CalculateEstimatedWaitTime();

        private TimeSpan? CalculateEstimatedWaitTime()
        {
            if (!NextAvailableDate.HasValue || !WaitingUsers.Any())
                return null;

            return NextAvailableDate.Value - DateTime.UtcNow;
        }
    }

    public class WaitingListItemViewModel
    {
        [Required(ErrorMessage = "נדרש מזהה משתמש")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "שם המשתמש נדרש")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "שם המשתמש חייב להכיל בין 2 ל-100 תווים")]
        [Display(Name = "שם משתמש")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "תאריך הצטרפות נדרש")]
        [Display(Name = "תאריך הצטרפות")]
        [DataType(DataType.DateTime)]
        public DateTime JoinDate { get; set; }

        [Display(Name = "האם קיבל התראה")]
        public bool IsNotified { get; set; }

        [Required(ErrorMessage = "נדרש פורמט ספר")]
        [Display(Name = "פורמט")]
        [EnumDataType(typeof(BookFormat), ErrorMessage = "פורמט לא חוקי")]
        public BookFormat Format { get; set; }

        [Display(Name = "תאריך זמינות משוער")]
        [DataType(DataType.DateTime)]
        public DateTime? EstimatedAvailabilityDate { get; set; }

        [Display(Name = "דואר אלקטרוני")]
        [EmailAddress(ErrorMessage = "כתובת דואר אלקטרוני לא חוקית")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "נדרש מיקום ברשימת המתנה")]
        [Range(1, int.MaxValue, ErrorMessage = "מיקום ברשימת המתנה חייב להיות חיובי")]
        [Display(Name = "מיקום בתור")]
        public int WaitingPosition { get; set; }

        [Display(Name = "פרטי ספר")]
        public Book? Book { get; set; }

        [Required(ErrorMessage = "נדרש שם ספר")]
        [Display(Name = "שם הספר")]
        public string BookTitle { get; set; } = string.Empty;

        // מאפיינים מחושבים
        [Display(Name = "זמן המתנה")]
        public TimeSpan WaitingTime => DateTime.UtcNow - JoinDate;

        [Display(Name = "מצב המתנה")]
        public string Status => GetStatus();

        [Display(Name = "צבע סטטוס")]
        public string StatusColor => GetStatusColor();

        // מאפיינים לוגיים
        public bool IsNextInLine => WaitingPosition == 1;
        public bool IsWaitingTooLong => WaitingTime.TotalDays > 30;
        public bool CanBeNotified => !IsNotified && IsNextInLine;
        public bool HasValidEmail => !string.IsNullOrEmpty(Email);

        private string GetStatus()
        {
            if (IsNotified)
                return "ממתין לאישור";
            if (IsNextInLine)
                return "הבא בתור";
            if (IsWaitingTooLong)
                return "ממתין זמן רב";
            return $"ממתין (מקום {WaitingPosition})";
        }

        private string GetStatusColor()
        {
            if (IsNotified)
                return "text-success";
            if (IsNextInLine)
                return "text-primary";
            if (IsWaitingTooLong)
                return "text-warning";
            return "text-secondary";
        }
    }

    public class WaitingListFilterViewModel
    {
        public int? BookId { get; set; }
        public string? SearchTerm { get; set; }
        public BookFormat? Format { get; set; }
        public bool? IsNotified { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class WaitingListPageViewModel
    {
        public List<WaitingListManagementViewModel> Items { get; set; } = [];
        public WaitingListFilterViewModel Filter { get; set; } = new();
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalItems / (double)Filter.PageSize);

        // סטטיסטיקות
        public int TotalWaitingUsers { get; set; }
        public int TotalNotifiedUsers { get; set; }
        public Dictionary<BookFormat, int> WaitingByFormat { get; set; } = [];
        public double AverageWaitTime { get; set; }
    }
}