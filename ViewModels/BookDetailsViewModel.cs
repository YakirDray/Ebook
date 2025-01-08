// ViewModels/BookDetailsViewModel.cs
using MyEBookLibrary.Models;

namespace MyEBookLibrary.ViewModels
{
    public class BookDetailsViewModel
    {
        public Book Book { get; set; } = null!;
        public BookReview? UserReview { get; set; }
        public bool IsBorrowed { get; set; }
        public bool IsPurchased { get; set; }
        public bool IsInWaitingList { get; set; }
        public List<BookFormat> AvailableFormats => Book?.AvailableFormats ?? [];
    }
}