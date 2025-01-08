// Services/LibraryService.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyEBookLibrary.Data;
using MyEBookLibrary.Models;
using MyEBookLibrary.Services.Interfaces;
using MyEBookLibrary.ViewModels;

namespace MyEBookLibrary.Services
{
    public class LibraryService(ApplicationDbContext context, IEmailNotificationService emailService, ILogger<LibraryService> logger) : ILibraryService
    {
        private readonly ApplicationDbContext _context = context;
        private readonly IEmailNotificationService _emailService = emailService;
        private readonly ILogger<LibraryService> _logger = logger; // הוספת שדה חדש

        public async Task<bool> BorrowBookAsync(int userId, int bookId, BookFormat format)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var book = await _context.Books
                    .Include(b => b.UserBooks)
                    .FirstOrDefaultAsync(b => b.Id == bookId);

                if (book == null || !book.IsBorrowable)
                    return false;

                // בדיקה שיש עותקים זמינים
                var activeBorrows = book.UserBooks.Count(ub => ub.IsBorrowed && !ub.ReturnDate.HasValue);
                if (activeBorrows >= 3)
                    return false;

                // בדיקת מגבלת השאלות למשתמש
                var userBorrows = await _context.UserBooks
                    .CountAsync(ub => ub.UserId == userId && ub.IsBorrowed && !ub.ReturnDate.HasValue);

                if (userBorrows >= 3)
                    return false;

                var userBook = new UserBook
                {
                    UserId = userId,
                    BookId = bookId,
                    IsBorrowed = true,
                    Format = format,
                    BorrowDate = DateTime.UtcNow,
                    ReturnDate = null,
                    IsReturned = false
                };

                _context.UserBooks.Add(userBook);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // שליחת התראה אם נדרש
                await _emailService.SendBookAvailabilityNotificationAsync(
                    userId.ToString(),
                    bookId,
                    format);

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error borrowing book {BookId} for user {UserId}", bookId, userId);
                return false;
            }
        }
        public async Task<bool> ReturnBookAsync(int userId, int bookId)
        {
            var userBook = await _context.UserBooks
                .FirstOrDefaultAsync(ub => ub.UserId == userId && ub.BookId == bookId && ub.IsBorrowed);

            if (userBook == null)
                return false;

            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
                return false;

            userBook.IsBorrowed = false;
            userBook.ReturnDate = DateTime.UtcNow;
            book.AvailableCopies++;

            await _context.SaveChangesAsync();
            await NotifyWaitingUsersAsync(bookId);

            return true;
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

            if (borrow.IsReturned)
                return BadRequest("הספר כבר הוחזר");

            borrow.ReturnDate = DateTime.UtcNow;
            borrow.IsReturned = true;

            // Check waiting list
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

        private IActionResult RedirectToAction(string v)
        {
            throw new NotImplementedException();
        }

        private IActionResult BadRequest(string v)
        {
            throw new NotImplementedException();
        }

        private IActionResult NotFound()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> PurchaseBookAsync(int userId, int bookId, BookFormat format)
        {
            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
                return false;

            var userBook = new UserBook
            {
                UserId = userId,
                BookId = bookId,
                IsPurchased = true,
                Format = format,
                BorrowDate = DateTime.UtcNow
            };

            _context.UserBooks.Add(userBook);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> AddToWaitingListAsync(int userId, int bookId, BookFormat format)
        {
            if (await IsUserInWaitingListAsync(userId, bookId))
                return false;

            var waitingItem = new WaitingListItem
            {
                UserId = userId,
                BookId = bookId,
                Format = format,
                JoinDate = DateTime.UtcNow
            };

            _context.WaitingList.Add(waitingItem);
            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<IEnumerable<BorrowHistoryViewModel>> GetUserBorrowedBooksAsync(int userId)
        {
            return await _context.UserBooks
                .Include(ub => ub.Book)
                .Include(ub => ub.User)
                .Where(ub => ub.UserId == userId && ub.IsBorrowed)
                .Select(ub => new BorrowHistoryViewModel
                {
                    BorrowId = ub.Id,
                    BookTitle = ub.Book.Title,
                    Author = ub.Book.Author,
                    UserName = ub.User.UserName ?? "לא ידוע",
                    BorrowDate = ub.BorrowDate ?? DateTime.UtcNow,
                    DueDate = (ub.BorrowDate ?? DateTime.UtcNow).AddDays(30),
                    ReturnDate = ub.ReturnDate,
                    Format = ub.Format,
                    // חישוב IsReturned בהתבסס על ReturnDate
                    IsReturned = ub.ReturnDate != null,
                    IsLate = !ub.ReturnDate.HasValue &&
                            ub.BorrowDate.HasValue &&
                            ub.BorrowDate.Value.AddDays(30) < DateTime.UtcNow,
                    CoverImageUrl = ub.Book.CoverImageUrl
                })
                .ToListAsync();
        }
        public async Task<bool> ExtendBorrowAsync(int userId, int borrowId)
        {
            try
            {
                var borrow = await _context.UserBooks
                    .Include(b => b.Book)
                    .FirstOrDefaultAsync(b => b.Id == borrowId && b.UserId == userId);

                if (borrow == null || !borrow.IsBorrowed || borrow.IsReturned)
                {
                    return false;
                }

                // Check if there's anyone waiting for this book
                var hasWaitingList = await _context.WaitingList
                    .AnyAsync(w => w.BookId == borrow.BookId && !w.IsNotified);

                if (hasWaitingList)
                {
                    return false;
                }

                // Extend the borrow period by 30 days from the original due date
                var currentDueDate = borrow.BorrowDate?.AddDays(30) ?? DateTime.UtcNow.AddDays(30);
                var newDueDate = currentDueDate.AddDays(30);

                // Send notification
                await _emailService.SendBorrowExtensionNotificationAsync(
                    userId.ToString(),
                    borrow.BookId,
                    newDueDate);

                borrow.BorrowDate = currentDueDate; // Reset borrow date to extend period
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extending borrow {BorrowId} for user {UserId}", borrowId, userId);
                return false;
            }
        }
        public async Task<IEnumerable<UserBook>> GetUserPurchasedBooksAsync(int userId)
        {
            return await _context.UserBooks
                .Include(ub => ub.Book)
                .Where(ub => ub.UserId == userId && ub.IsPurchased)
                .ToListAsync();
        }

        public async Task<IEnumerable<WaitingListItem>> GetUserWaitingListAsync(int userId)
        {
            return await _context.WaitingList
                .Include(w => w.Book)
                .Where(w => w.UserId == userId)
                .ToListAsync();
        }

        public async Task<bool> IsBookAvailableAsync(int bookId, BookFormat format)
        {
            var book = await _context.Books.FindAsync(bookId);
            return book != null && book.AvailableCopies > 0 && book.AvailableFormats.Contains(format);
        }

        public async Task<int> GetBorrowedBooksCountAsync(int userId)
        {
            return await _context.UserBooks
                .CountAsync(ub => ub.UserId == userId && ub.IsBorrowed);
        }

        public async Task<bool> IsBookBorrowedByUserAsync(int userId, int bookId)
        {
            return await _context.UserBooks
                .AnyAsync(ub => ub.UserId == userId && ub.BookId == bookId && ub.IsBorrowed);
        }

        public async Task<bool> IsBookPurchasedByUserAsync(int userId, int bookId)
        {
            return await _context.UserBooks
                .AnyAsync(ub => ub.UserId == userId && ub.BookId == bookId && ub.IsPurchased);
        }

        public async Task<bool> IsUserInWaitingListAsync(int userId, int bookId)
        {
            return await _context.WaitingList
                .AnyAsync(w => w.UserId == userId && w.BookId == bookId);
        }
        private async Task NotifyWaitingUsersAsync(int bookId)
        {
            var waitingUsers = await _context.WaitingList
                .Include(w => w.User)
                .Where(w => w.BookId == bookId && !w.IsNotified)
                .ToListAsync();

            var book = await _context.Books.FindAsync(bookId);
            if (book == null) return;

            foreach (var waitingItem in waitingUsers)
            {
                if (await IsBookAvailableAsync(bookId, waitingItem.Format))
                {
                    waitingItem.IsNotified = true;
                    await _emailService.SendBookAvailableNotificationAsync(waitingItem.User, book);
                }
            }

            await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<BorrowHistoryViewModel>> GetAllActiveBorrowsAsync()
        {
            try
            {
                var activeLoans = await _context.UserBooks
                    .Include(b => b.User)
                    .Include(b => b.Book)
                    .Where(b => b.IsBorrowed) // Using only IsBorrowed flag
                    .Select(b => new BorrowHistoryViewModel
                    {
                        BorrowId = b.Id,
                        UserName = b.User.UserName ?? "Unknown",
                        BookTitle = b.Book.Title,
                        Author = b.Book.Author,
                        BorrowDate = b.BorrowDate ?? DateTime.UtcNow,
                        DueDate = (b.BorrowDate ?? DateTime.UtcNow).AddDays(30),
                        ReturnDate = b.ReturnDate,
                        Format = b.Format,
                        CoverImageUrl = b.Book.CoverImageUrl,
                        IsReturned = b.ReturnDate.HasValue,
                        IsLate = !b.ReturnDate.HasValue &&
                            b.BorrowDate.HasValue &&
                            b.BorrowDate.Value.AddDays(30) < DateTime.UtcNow,
                    })
                    .OrderByDescending(b => b.BorrowDate)
                    .ToListAsync();

                return activeLoans;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active borrows");
                throw;
            }
        }
    }
}