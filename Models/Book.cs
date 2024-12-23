// Models/Book.cs
using System.ComponentModel.DataAnnotations;

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
        public List<BookFormat> AvailableFormats { get; set; } = new();
        public string AgeRestriction { get; set; } = string.Empty;
        public decimal? OriginalPrice { get; set; }
        public DateTime? DiscountEndDate { get; set; }
        public List<BookReview> Reviews { get; set; } = new();
        public List<UserBook> UserBooks { get; set; } = new List<UserBook>();
        public List<WaitingListItem> WaitingList { get; set; } = new List<WaitingListItem>();
        public decimal DiscountedPrice { get; private set; }

        public decimal GetCurrentPrice()
        {
            return (DiscountEndDate.HasValue && DiscountEndDate.Value > DateTime.Now) ? DiscountedPrice : BuyPrice;
        }
        public class ApplicationUser
        {
            [Key] // Data Annotation for specifying 'Id' as the primary key
            public int Id { get; set; }
            public virtual List<BookReview> Reviews { get; set; } = new List<BookReview>();
            public required string UserName { get; set; }

        }
    }
}