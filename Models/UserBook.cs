// Models/UserBook.cs

using System.ComponentModel.DataAnnotations.Schema;

namespace MyEBookLibrary.Models
{
    public class UserBook
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int BookId { get; set; }
        public bool IsBorrowed { get; set; }
        public bool IsPurchased { get; set; }
        public DateTime? BorrowDate { get; set; }
        public DateTime? ReturnDate { get; set; }

        public BookFormat Format { get; set; }
        public bool IsReturned { get; set; }

        [NotMapped]
        public DateTime DueDate => (BorrowDate ?? DateTime.UtcNow).AddDays(30);

        [NotMapped]
        public bool IsLate => !IsReturned && DateTime.UtcNow > DueDate;
        public string? UserName => User?.UserName;
        public virtual User User { get; set; } = null!;
        public virtual Book Book { get; set; } = null!;

        public decimal BuyPrice { get; set; }
    }
    public class Transaction
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; } // Add the Amount property
        // Add other properties here
    }
}

