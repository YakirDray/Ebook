using MyEBookLibrary.Models;

namespace MyEBookLibrary.ViewModels
{
    public class UserProfileViewModel
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<UserBookViewModel> BorrowedBooks { get; set; } = [];
        public List<UserBookViewModel> PurchasedBooks { get; set; } = [];
        public List<WaitingListItemViewModel> WaitingList { get; set; } = [];
        public List<BookReviewViewModel> Reviews { get; set; } = [];
        public UserStatisticsViewModel Statistics { get; set; } = new();
    }
    public class BookReviewViewModel
    {
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime ReviewDate { get; set; }
        public string UserName { get; set; } = string.Empty;
    }

    public class UserBookViewModel
    {
        public int BookId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string CoverImageUrl { get; set; } = string.Empty;
        public BookFormat Format { get; set; }
        public DateTime? BorrowDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsLate { get; set; }
        public bool IsReturned { get; set; }
        public decimal Price { get; set; }
    }

    public class UserStatisticsViewModel
    {
        public int TotalBorrows { get; set; }
        public int ActiveBorrows { get; set; }
        public int TotalPurchases { get; set; }
        public int ReviewsCount { get; set; }
        public decimal TotalSpent { get; set; }
        public int OverdueBooks { get; set; }
        public int WaitingListCount { get; set; }
    }
}