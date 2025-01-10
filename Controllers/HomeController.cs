using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyEBookLibrary.Data;
using MyEBookLibrary.Models;
using MyEBookLibrary.ViewModels;
using System.Diagnostics;

namespace MyEBookLibrary.Controllers
{
   public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(
        ILogger<HomeController> logger,
        ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var newBooks = await _context.Books
            .OrderByDescending(b => b.Id)
            .Take(6)
            .AsNoTracking()
            .ToListAsync();

        var popularBooks = await _context.Books
            .Include(b => b.Reviews)
            .Select(b => new
            {
                Book = b,
                AverageRating = b.Reviews.Any() ? b.Reviews.Average(r => r.Rating) : 0
            })
            .OrderByDescending(b => b.AverageRating)
            .Take(6)
            .AsNoTracking()
            .Select(b => b.Book)
            .ToListAsync();

        var viewModel = new HomeViewModel
        {
            TotalBooks = await _context.Books.CountAsync(),
            NewBooks = newBooks,
            PopularBooks = popularBooks,
            BooksOnSale = await _context.Books
                .Where(b => b.DiscountEndDate != null && 
                           b.DiscountEndDate > DateTime.Now)
                .Take(6)
                .AsNoTracking()
                .ToListAsync(),
            UserReviews = await _context.BookReviews
                .Include(r => r.Reviewer)
                .Include(r => r.Book)
                .OrderByDescending(r => r.ReviewDate)
                .Take(5)
                .AsNoTracking()
                .ToListAsync()
        };

        return View(viewModel);
    }

    public async Task<IActionResult> Genres()
    {
        var genres = await _context.Books
            .Select(b => b.Genre)
            .Distinct()
            .AsNoTracking()
            .ToListAsync();

        var booksByGenre = new Dictionary<string, List<Book>>();
        foreach (var genre in genres)
        {
            var books = await _context.Books
                .Where(b => b.Genre == genre)
                .Take(6)
                .AsNoTracking()
                .ToListAsync();

            booksByGenre.Add(genre, books);
        }

        return View(booksByGenre);
    }

    public async Task<IActionResult> Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return RedirectToAction(nameof(Index));

        var searchResults = await _context.Books
            .Where(b => b.Title.Contains(query) ||
                       b.Author.Contains(query) ||
                       b.Description.Contains(query))
            .AsNoTracking()
            .ToListAsync();

        return View(searchResults);
    }

    public async Task<IActionResult> OnSale()
    {
        var booksOnSale = await _context.Books
            .Where(b => b.DiscountEndDate != null && 
                       b.DiscountEndDate > DateTime.Now)
            .AsNoTracking()
            .ToListAsync();

        return View(booksOnSale);
    }

    public async Task<IActionResult> Age(string restriction)
    {
        var books = await _context.Books
            .Where(b => b.AgeRestriction == restriction)
            .AsNoTracking()
            .ToListAsync();

        return View(books);
    }

    public IActionResult ServiceReviews()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
}