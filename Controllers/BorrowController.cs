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
    [Authorize]
    public class BorrowController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IEmailNotificationService _emailService;
        private readonly ILogger<BorrowController> _logger;

        public BorrowController(
            ApplicationDbContext context,
            UserManager<User> userManager,
            IEmailNotificationService emailService,
            ILogger<BorrowController> logger)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(BorrowHistoryFilterViewModel filter)
        {
            var query = _context.UserBooks
               .Include(b => b.User)
               .Include(b => b.Book)
               .Where(b => b.IsBorrowed)
               .AsQueryable();

            if (filter.FromDate.HasValue)
            {
                var fromDate = filter.FromDate.Value;
                query = query.Where(b => b.BorrowDate >= fromDate);
            }

            if (filter.ToDate.HasValue)
            {
                var toDate = filter.ToDate.Value;
                query = query.Where(b => b.BorrowDate <= toDate);
            }

            // חישוב סטטיסטיקות
            var statistics = await CalculateStatisticsAsync(query);

            // סידור התוצאות
            if (!string.IsNullOrEmpty(filter.SortBy))
            {
                query = filter.SortBy.ToLower() switch
                {
                    "date" => filter.SortDescending ?
                        query.OrderByDescending(b => b.BorrowDate) :
                        query.OrderBy(b => b.BorrowDate),
                    "user" => filter.SortDescending ?
                        query.OrderByDescending(b => b.User.UserName) :
                        query.OrderBy(b => b.User.UserName),
                    "book" => filter.SortDescending ?
                        query.OrderByDescending(b => b.Book.Title) :
                        query.OrderBy(b => b.Book.Title),
                    _ => query.OrderByDescending(b => b.BorrowDate)
                };
            }
            else
            {
                query = query.OrderByDescending(b => b.BorrowDate);
            }

            var totalItems = await query.CountAsync();
            var items = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(b => new BorrowHistoryViewModel
                {
                    BorrowId = b.Id,
                    UserId = b.UserId,
                    BookId = b.BookId,
                    UserName = b.User.UserName ?? "Unknown",
                    UserEmail = b.User.Email ?? string.Empty,
                    BookTitle = b.Book.Title,
                    Author = b.Book.Author,
                    Genre = b.Book.Genre,
                    Publisher = b.Book.Publisher,
                    YearOfPublication = b.Book.YearOfPublication,
                    BorrowDate = b.BorrowDate ?? DateTime.UtcNow,
                    DueDate = (b.BorrowDate ?? DateTime.UtcNow).AddDays(30),
                    ReturnDate = b.ReturnDate,
                    Format = b.Format,
                    CoverImageUrl = b.Book.CoverImageUrl,
                    IsReturned = b.ReturnDate.HasValue,
                    IsLate = !b.ReturnDate.HasValue &&
                        b.BorrowDate.HasValue &&
                        b.BorrowDate.Value.AddDays(30) < DateTime.UtcNow
                })
                .ToListAsync();

            var viewModel = new BorrowHistoryPageViewModel
            {
                Items = items,
                Filter = filter,
                TotalItems = totalItems,
                Statistics = statistics
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Return(int id)
        {
            var borrow = await _context.UserBooks
                .Include(b => b.Book)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (borrow == null)
                return NotFound();

            if (borrow.ReturnDate.HasValue)
                return BadRequest("הספר כבר הוחזר");

            borrow.ReturnDate = DateTime.UtcNow;
            borrow.IsReturned = true;

            // בדיקת רשימת המתנה
            var nextInLine = await _context.WaitingList
                .Include(w => w.User)
                .Where(w => w.BookId == borrow.BookId && !w.IsNotified)
                .OrderBy(w => w.JoinDate)
                .FirstOrDefaultAsync();

            if (nextInLine != null)
            {
                nextInLine.IsNotified = true;
                await _emailService.SendBookAvailabilityNotificationAsync(
                    nextInLine.UserId.ToString(),
                    borrow.BookId,
                    nextInLine.Format);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Extend(int id)
        {
            var borrow = await _context.UserBooks
                .FindAsync(id);

            if (borrow == null)
                return NotFound();

            if (borrow.ReturnDate.HasValue)
                return BadRequest("לא ניתן להאריך השאלה של ספר שהוחזר");

            var hasWaitingList = await _context.WaitingList
                .AnyAsync(w => w.BookId == borrow.BookId);

            if (hasWaitingList)
                return BadRequest("לא ניתן להאריך השאלה כאשר יש אנשים ברשימת המתנה");

            var newDueDate = (borrow.BorrowDate ?? DateTime.UtcNow).AddDays(60);

            await _emailService.SendBorrowExtensionNotificationAsync(
                borrow.UserId.ToString(),
                borrow.BookId,
                newDueDate);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

      [HttpGet]
public async Task<IActionResult> MyWaitingList()
{
    try
    {
        var userId = int.Parse(_userManager.GetUserId(User) ?? 
            throw new InvalidOperationException("User ID is required"));

        // קודם נביא את הנתונים הבסיסיים
        var baseWaitingList = await _context.WaitingList
            .Include(w => w.Book)
            .Where(w => w.UserId == userId && !w.IsNotified)
            .Select(w => new
            {
                w.BookId,
                w.Book,
                w.Format,
                w.JoinDate,
                WaitingPosition = _context.WaitingList
                    .Count(wl => wl.BookId == w.BookId && 
                               wl.JoinDate < w.JoinDate) + 1
            })
            .ToListAsync();

        // עכשיו נוכל לחשב את התאריכים המשוערים בנפרד
        var waitingList = new List<WaitingListItemViewModel>();
        
        foreach (var item in baseWaitingList)
        {
            var estimatedDate = await CalculateEstimatedAvailability(item.BookId);
            
            waitingList.Add(new WaitingListItemViewModel
            {
                UserId = userId.ToString(),
                BookTitle = item.Book.Title,
                Format = item.Format,
                JoinDate = item.JoinDate,
                WaitingPosition = item.WaitingPosition,
                Book = item.Book,
                EstimatedAvailabilityDate = estimatedDate
            });
        }

        return View(waitingList);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving waiting list for user {UserId}", User);
        TempData["Error"] = "אירעה שגיאה בטעינת רשימת ההמתנה";
        return RedirectToAction("Index", "Home");
    }
}
        private async Task<DateTime?> CalculateEstimatedAvailability(int bookId)
        {
            var activeLoans = await _context.UserBooks
                .Where(b => b.BookId == bookId && !b.ReturnDate.HasValue)
                .OrderBy(b => b.BorrowDate)
                .FirstOrDefaultAsync();

            return activeLoans?.BorrowDate?.AddDays(30);
        }

        private async Task<BorrowStatistics> CalculateStatisticsAsync(IQueryable<UserBook> query)
        {
            var now = DateTime.UtcNow;
            return new BorrowStatistics
            {
                TotalBorrows = await query.CountAsync(),
                ActiveBorrows = await query.CountAsync(b => !b.ReturnDate.HasValue),
                OverdueBorrows = await query.CountAsync(b => 
                    !b.ReturnDate.HasValue && 
                    b.BorrowDate.HasValue && 
                    b.BorrowDate.Value.AddDays(30) < now),
                ReturnedOnTime = await query.CountAsync(b => 
                    b.ReturnDate.HasValue && 
                    b.BorrowDate.HasValue && 
                    b.ReturnDate <= b.BorrowDate.Value.AddDays(30))
            };
        }
    }
}