using System.ComponentModel.DataAnnotations;
using MyEBookLibrary.Models;

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
