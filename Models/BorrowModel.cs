using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyEBookLibrary.Models
{
    public class Borrow
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int BookId { get; set; }

        [Required]
        public DateTime BorrowDate { get; set; } = DateTime.UtcNow;

        public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(30); // 30 ימי השאלה

        public DateTime? ReturnDate { get; set; }

        public BookFormat Format { get; set; }

        public bool ReminderSent { get; set; }

        public bool IsOverdue => !IsReturned && DateTime.UtcNow > DueDate;

        public bool IsReturned => ReturnDate.HasValue;

        public bool IsActive => !IsReturned && !IsOverdue;

        // מספר ימים שנותרו להחזרה
        public int DaysRemaining => IsReturned ?
            0 :
            (int)(DueDate - DateTime.UtcNow).TotalDays;

        // האם צריך לשלוח תזכורת (5 ימים לפני מועד ההחזרה)
        public bool NeedsReminder => IsActive &&
            !ReminderSent &&
            DaysRemaining <= 5 &&
            DaysRemaining > 0;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("BookId")]
        public virtual Book Book { get; set; } = null!;

        // שיטה לחישוב תאריך החזרה
        public static DateTime CalculateDueDate(DateTime borrowDate)
        {
            return borrowDate.AddDays(30);
        }

        // שיטה לבדיקה האם המשתמש יכול לשאול ספר נוסף
        public static bool CanUserBorrow(IEnumerable<Borrow> userBorrows)
        {
            var activeBorrows = userBorrows.Count(b => b.IsActive);
            return activeBorrows < 3; // מקסימום 3 ספרים להשאלה
        }

        // שיטה לבדיקה האם יש עותקים זמינים של הספר
        public static bool IsBookAvailable(IEnumerable<Borrow> bookBorrows)
        {
            var activeBorrows = bookBorrows.Count(b => b.IsActive);
            return activeBorrows < 3; // מקסימום 3 עותקים להשאלה
        }
    }

    public class BorrowConfiguration
    {
        public const int MaxBooksPerUser = 3;
        public const int MaxCopiesPerBook = 3;
        public const int BorrowPeriodDays = 30;
        public const int ReminderDaysBeforeReturn = 5;

        public static readonly TimeSpan ReservationExpiryPeriod = TimeSpan.FromHours(48);
    }

    public enum BorrowStatus
    {
        Active,
        Returned,
        Overdue,
        Reserved,
        Cancelled
    }
}