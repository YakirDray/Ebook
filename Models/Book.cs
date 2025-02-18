// Models/Book.cs

namespace MyEBookLibrary.Models
{
    public enum BookFormat
    {
        PDF,
        EPUB,
        MOBI,
        F2B,
        Unknown

    }
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Publisher { get; set; } = string.Empty;
        public int YearOfPublication { get; set; }
        public string CoverImageUrl { get; set; } = string.Empty;
        public decimal BuyPrice { get; set; }
        public decimal BorrowPrice { get; set; }
        public bool IsBorrowable { get; set; }
        public int AvailableCopies { get; set; }
        public List<BookFormat> AvailableFormats { get; set; } = [];
        public string AgeRestriction { get; set; } = string.Empty;
        public decimal? OriginalPrice { get; set; }
        public DateTime? DiscountEndDate { get; set; }
        public List<BookReview> Reviews { get; set; } = [];
        public List<UserBook> UserBooks { get; set; } = [];
        public List<WaitingListItem> WaitingList { get; set; } = [];
        public decimal DiscountedPrice { get; private set; }
        public bool IsAvailable { get; internal set; }

        public decimal GetCurrentPrice()
        {
            return (DiscountEndDate.HasValue && DiscountEndDate.Value > DateTime.Now) ? DiscountedPrice : BuyPrice;
        }

    }
}