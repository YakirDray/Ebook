// Services/LibraryService.cs
using Microsoft.EntityFrameworkCore;
using MyEBookLibrary.Data;
using MyEBookLibrary.Models;
using MyEBookLibrary.Services.Interfaces;

namespace MyEBookLibrary.Services
{
    public class LibraryService : ILibraryService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailNotificationService _emailService;

        public LibraryService(ApplicationDbContext context, IEmailNotificationService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<bool> BorrowBookAsync(int userId, int bookId, BookFormat format)
        {
            var book = await _context.Books.FindAsync(bookId);
            if (book == null || !book.IsBorrowable || book.AvailableCopies <= 0)
                return false;

            var existingBorrow = await _context.UserBooks
                .FirstOrDefaultAsync(ub => ub.UserId == userId && ub.BookId == bookId && ub.IsBorrowed);

            if (existingBorrow != null)
                return false;

            var borrowedCount = await GetBorrowedBooksCountAsync(userId);
            var maxBorrowLimit = 3;

            if (borrowedCount >= maxBorrowLimit)
                return false;

            var userBook = new UserBook
            {
                UserId = userId,
                BookId = bookId,
                IsBorrowed = true,
                Format = format,
                BorrowDate = DateTime.UtcNow,
                ReturnDate = DateTime.UtcNow.AddDays(14)
            };

            book.AvailableCopies--;
            _context.UserBooks.Add(userBook);
            await _context.SaveChangesAsync();
            await NotifyWaitingUsersAsync(bookId);

            return true;
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

        public async Task<IEnumerable<UserBook>> GetUserBorrowedBooksAsync(int userId)
        {
            return await _context.UserBooks
                .Include(ub => ub.Book)
                .Where(ub => ub.UserId == userId && ub.IsBorrowed)
                .ToListAsync();
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
    }
}