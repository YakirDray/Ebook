// ViewModels/BooksIndexViewModel.cs
using MyEBookLibrary.Models;

#pragma warning disable CA1050 // Declare types in namespaces
public class BooksIndexViewModel
#pragma warning restore CA1050 // Declare types in namespaces
{
    public required IEnumerable<Book> Books { get; set; }
    public required string SearchString { get; set; }
    public required string SelectedGenre { get; set; }
    public required IEnumerable<string> Genres { get; set; }
}

