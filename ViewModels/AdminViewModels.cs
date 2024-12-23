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
        public List<WaitingListItem> RecentWaitingList { get; set; } = new();
    }

    public class BookCreateViewModel
    {
        [Required(ErrorMessage = "נדרשת כותרת")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "נדרש שם מחבר")]
        [StringLength(100)]
        public string Author { get; set; } = string.Empty;

        [Required(ErrorMessage = "נדרשת הוצאה לאור")]
        public string Publisher { get; set; } = string.Empty;

        [Required(ErrorMessage = "נדרשת שנת הוצאה")]
        [Range(1800, 2100)]
        public int YearOfPublication { get; set; }

        [Required(ErrorMessage = "נדרש ז'אנר")]
        public string Genre { get; set; } = string.Empty;

        [Required(ErrorMessage = "נדרש תיאור")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "נדרש מחיר רכישה")]
        [Range(0.01, 1000)]
        public decimal BuyPrice { get; set; }

        [Required(ErrorMessage = "נדרש מחיר השאלה")]
        [Range(0.01, 1000)]
        public decimal BorrowPrice { get; set; }

        public bool IsBorrowable { get; set; }

        [Required(ErrorMessage = "נדרשת הגבלת גיל")]
        public string AgeRestriction { get; set; } = string.Empty;

        [Required(ErrorMessage = "נדרש מספר עותקים")]
        [Range(0, 100)]
        public int AvailableCopies { get; set; }

        [Required(ErrorMessage = "נדרש לפחות פורמט אחד")]
        public List<BookFormat> AvailableFormats { get; set; } = new();

        public IFormFile? CoverImage { get; set; }
    }

    public class BookEditViewModel : BookCreateViewModel
    {
        public int Id { get; set; }
        public string? CurrentCoverImageUrl { get; set; }
        public decimal? OriginalPrice { get; set; }
        public DateTime? DiscountEndDate { get; set; }
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