using MyEBookLibrary.Models;

public class HomeViewModel
{
    public int TotalBooks { get; set; }
    public List<Book> NewBooks { get; set; } = [];
    public List<Book> PopularBooks { get; set; } = [];
    public List<Book> BooksOnSale { get; set; } = [];
    public List<BookReview> UserReviews { get; set; } = [];
}
