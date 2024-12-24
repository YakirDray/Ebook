
namespace MyEBookLibrary.Models;

public class Borrow
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public User User { get; set; } = new();  // קשר למשתמש

    public int BookId { get; set; }
    public Book Book { get; set; } = new();
    public DateTime BorrowDate { get; set; }
    public DateTime? ReturnDate { get; set; }

    public bool IsReturned => ReturnDate.HasValue && ReturnDate <= DateTime.Now;
}

