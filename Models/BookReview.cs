using System.ComponentModel.DataAnnotations;
using static MyEBookLibrary.Models.Book;

namespace MyEBookLibrary.Models
{
    public class BookReview
    {
        public int Id { get; set; }

        [Required]
        public int BookId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [Required]
        [StringLength(1000, ErrorMessage = "Comment cannot be longer than 1000 characters")]
        public string Comment { get; set; } = string.Empty;
    public int ReviewerId { get; set; } // שינוי שם למפתח הזר

        public DateTime ReviewDate { get; set; } = DateTime.UtcNow;
    public required User Reviewer { get; set; } // התאמת המפתח הזר

        public virtual Book Book { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;
    }

    
}