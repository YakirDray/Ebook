using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyEBookLibrary.Data;
using MyEBookLibrary.Models;
using MyEBookLibrary.Services.Interfaces;
using MyEBookLibrary.ViewModels;

namespace MyEBookLibrary.Controllers
{
    public class BooksController(
        ApplicationDbContext context,
        ILibraryService libraryService,
        ICartService cartService,

        UserManager<User> userManager) : Controller
    {
        private readonly ApplicationDbContext _context = context;
        private readonly ILibraryService _libraryService = libraryService;
        private readonly UserManager<User> _userManager = userManager;
        private readonly ICartService _cartService = cartService;

        public async Task<IActionResult> Index(string searchString, string genre)
        {
            var booksQuery = _context.Books
                .Include(b => b.Reviews)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                booksQuery = booksQuery.Where(b =>
                    b.Title.Contains(searchString) ||
                    b.Author.Contains(searchString) ||
                    b.Description.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(genre))
            {
                booksQuery = booksQuery.Where(b => b.Genre == genre);
            }

            var books = await booksQuery.ToListAsync();
            var genres = await _context.Books.Select(b => b.Genre).Distinct().ToListAsync();

            var viewModel = new BooksIndexViewModel
            {
                Books = books,
                Genres = genres,
                SearchString = searchString,
                SelectedGenre = genre
            };

            return View(viewModel);
        }
        public async Task<IActionResult> Popular(int page = 1)
        {
            const int PageSize = 12;

            // First get all books with their related data
            var query = _context.Books
                .Include(b => b.Reviews)
                .Include(b => b.UserBooks)
                .AsQueryable();

            // Get total count for pagination
            var totalBooks = await query.CountAsync();

            // Then apply ordering and pagination
            var popularBooks = await query
                .OrderByDescending(b => b.Reviews.Any() ? b.Reviews.Average(r => r.Rating) : 0)
                .ThenByDescending(b => b.UserBooks.Count(ub => ub.IsBorrowed || ub.IsPurchased))
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            var totalPages = (int)Math.Ceiling(totalBooks / (double)PageSize);

            var viewModel = new PopularBooksViewModel
            {
                Books = popularBooks,
                CurrentPage = page,
                TotalPages = totalPages,
                HasPreviousPage = page > 1,
                HasNextPage = page < totalPages
            };

            return View(viewModel);
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var book = await _context.Books
                .Include(b => b.Reviews)
                    .ThenInclude(r => r.Reviewer)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (book == null)
                return NotFound();

            var userId = User.Identity?.IsAuthenticated == true
                ? int.Parse(_userManager.GetUserId(User) ?? "0")
                : (int?)null;

            var viewModel = new BookDetailsViewModel
            {
                Book = book,
                IsBorrowed = userId.HasValue && await _libraryService.IsBookBorrowedByUserAsync(userId.Value, book.Id),
                IsPurchased = userId.HasValue && await _libraryService.IsBookPurchasedByUserAsync(userId.Value, book.Id),
                IsInWaitingList = userId.HasValue && await _libraryService.IsUserInWaitingListAsync(userId.Value, book.Id),
                UserReview = userId.HasValue ? book.Reviews.FirstOrDefault(r => r.UserId == userId) : null
            };

            return View(viewModel);
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Borrow(int id, BookFormat format)
        {
            if (!User.Identity!.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            var userId = int.Parse(_userManager.GetUserId(User)!);
            var result = await _cartService.AddToCartAsync(userId, id, true, format);

            if (result)
                return RedirectToAction("Index", "Cart");

            TempData["Error"] = "לא ניתן להוסיף את הספר לעגלה";
            return RedirectToAction("Details", new { id });
        }

        [HttpPost]
        [Authorize]

        public async Task<IActionResult> Purchase(int id, BookFormat format)
        {
            if (!User.Identity!.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            var userId = int.Parse(_userManager.GetUserId(User)!);
            var result = await _cartService.AddToCartAsync(userId!, id, false, format);

            if (result)
                return RedirectToAction("Index", "Cart");

            TempData["Error"] = "לא ניתן להוסיף את הספר לעגלה";
            return RedirectToAction("Details", new { id });
        }
    }

}
