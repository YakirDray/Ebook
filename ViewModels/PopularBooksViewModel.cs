using MyEBookLibrary.Models;

public class PopularBooksViewModel
{
    public List<Book> Books { get; set; } = [];
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }
}