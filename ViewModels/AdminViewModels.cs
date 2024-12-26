// ViewModels/AdminViewModels.cs
using System.ComponentModel.DataAnnotations;
using MyEBookLibrary.Models;

namespace MyEBookLibrary.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalBooks { get; set; }
        public int TotalUsers { get; set; }
        public int ActiveBorrows { get; set; }
        public int TotalSales { get; set; }
        public int WaitingListCount { get; set; }
        public int TotalBorrows { get; set; }

        public decimal TotalRevenue { get; set; }
        public int TotalReviews { get; set; }
        public List<Book> PopularBooks { get; set; } = new();

        // ספרים שהוחזרו
        public List<Book> ReturnedBooks { get; set; } = new();

        // תאריכי החזרה צפויים
        public List<DateTime> ReturnDueDates { get; set; } = new();

        // ספרים עם איחור
        public List<Book> LateReturns { get; set; } = new();

        // קנסות על החזרות מאוחרות
        public decimal TotalFines { get; set; }

        // סטטוס השאלה
        public bool BorrowLimitExceeded { get; set; }
        public List<WaitingListItem> RecentWaitingList { get; set; } = new();
        // תוספת: ספרים שההחזרה שלהם צפויה ב-7 הימים הקרובים
        public List<BorrowHistoryViewModel> UpcomingReturns { get; set; } = new();

        // תוספת: משתמשים שהגיעו לגבול השאלות שלהם
        public List<UserManagementViewModel> UsersNearBorrowLimit { get; set; } = new();
    }

  
    public class DiscountViewModel
    {
        public int BookId { get; set; }

        [Required(ErrorMessage = "נדרש מחיר מוזל")]
        [Range(0.01, 1000)]
        public decimal DiscountedPrice { get; set; }

        [Required(ErrorMessage = "נדרש תאריך סיום")]
        public DateTime EndDate { get; set; }
    }


    public class UserManagementViewModel
    {
        public string Id { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsLocked { get; set; }
        public int BorrowedBooksCount { get; set; }
        public int PurchasedBooksCount { get; set; }
        public decimal TotalSpent { get; set; }
        public List<UserBook> RecentActivity { get; set; } = new();
    }

    public class BookStatsViewModel
    {
        public int Id { get; set; }
        public required string UserId { get; set; }

        public string Title { get; set; } = string.Empty;
        public int TotalBorrows { get; set; }
        public int TotalPurchases { get; set; }
        public decimal Revenue { get; set; }
        public double AverageRating { get; set; }
        public int ReviewsCount { get; set; }
        public int WaitingListCount { get; set; }
    }
}