using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyEBookLibrary.Data;
using MyEBookLibrary.Models;
using MyEBookLibrary.ViewModels;
using MyEBookLibrary.Services.Interfaces;

namespace MyEBookLibrary.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController(

        ApplicationDbContext context,
        UserManager<User> userManager,
        IEmailNotificationService emailService,
        ILibraryService libraryService) : Controller
    {
        private readonly ApplicationDbContext _context = context;
        private readonly UserManager<User> _userManager = userManager;
        private readonly IEmailNotificationService _emailService = emailService;
        private readonly ILibraryService _libraryService = libraryService;

        public async Task<IActionResult> Index()
        {
            var statistics = new AdminDashboardViewModel
            {
                TotalBooks = await _context.Books.CountAsync(),
                TotalUsers = await _context.Users.CountAsync(),
                ActiveBorrows = await _context.UserBooks.CountAsync(ub => ub.IsBorrowed),
                TotalSales = await _context.UserBooks.CountAsync(ub => ub.IsPurchased),
                WaitingListCount = await _context.WaitingList.CountAsync()
            };

            return View("Index", statistics);  // 
        }



        public async Task<IActionResult> UserIndex()
        {
            var users = await _userManager.Users
                .Include(u => u.Books)
                .Select(user => new
                UserManagementViewModel
                {
                    Id = user.Id.ToString(),
                    UserName = user.UserName!,
                    Email = user.Email!,
                    IsLocked = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow,
                    BorrowedBooksCount = user.Books.Count(b => b.IsBorrowed),
                    PurchasedBooksCount = user.Books.Count(b => b.IsPurchased),
                    TotalSpent = user.Transactions.Sum(t => t.Amount)
                }).ToListAsync();

            return View("UserList", users);  // טוען תצוגה בשם UserList.cshtml
        }


        [HttpPost]
        public async Task<IActionResult> ToggleUserStatus(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            user.LockoutEnd = user.LockoutEnd == null ? DateTimeOffset.MaxValue : null;
            await _userManager.UpdateAsync(user);

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> ManageBooks()
        {
            var books = await _context.Books
                .Include(b => b.UserBooks)
                .Include(b => b.Reviews)
                .OrderByDescending(b => b.Title)
                .ToListAsync();
            return View(books);
        }

        [HttpGet]
        public IActionResult CreateBook()
        {
            return View(new BookCreateViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> CreateBook(BookCreateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var book = new Book
            {
                Title = model.Title,
                Author = model.Author,
                Publisher = model.Publisher,
                YearOfPublication = model.YearOfPublication,
                Genre = model.Genre,
                Description = model.Description,
                BuyPrice = model.BuyPrice,
                BorrowPrice = model.BorrowPrice,
                IsBorrowable = model.IsBorrowable,
                AgeRestriction = model.AgeRestriction,
                AvailableCopies = model.AvailableCopies,
                AvailableFormats = model.AvailableFormats
            };

            if (model.CoverImage != null)
            {
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(model.CoverImage.FileName)}";
                var filePath = Path.Combine("wwwroot/images/books", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.CoverImage.CopyToAsync(stream);
                }

                book.CoverImageUrl = $"/images/books/{fileName}";
            }

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            TempData["Success"] = "הספר נוסף בהצלחה";
            return RedirectToAction(nameof(ManageBooks));
        }

        [HttpGet]
        public async Task<IActionResult> EditBook(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
                return NotFound();

            var viewModel = new BookEditViewModel
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                Publisher = book.Publisher,
                YearOfPublication = book.YearOfPublication,
                Genre = book.Genre,
                Description = book.Description,
                BuyPrice = book.BuyPrice,
                BorrowPrice = book.BorrowPrice,
                IsBorrowable = book.IsBorrowable,
                AgeRestriction = book.AgeRestriction,
                AvailableCopies = book.AvailableCopies,
                AvailableFormats = book.AvailableFormats,
                CurrentCoverImageUrl = book.CoverImageUrl
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditBook(BookEditViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var book = await _context.Books.FindAsync(model.Id);
            if (book == null)
                return NotFound();

            book.Title = model.Title;
            book.Author = model.Author;
            book.Publisher = model.Publisher;
            book.YearOfPublication = model.YearOfPublication;
            book.Genre = model.Genre;
            book.Description = model.Description;
            book.BuyPrice = model.BuyPrice;
            book.BorrowPrice = model.BorrowPrice;
            book.IsBorrowable = model.IsBorrowable;
            book.AgeRestriction = model.AgeRestriction;
            book.AvailableCopies = model.AvailableCopies;
            book.AvailableFormats = model.AvailableFormats;

            if (model.CoverImage != null)
            {
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(model.CoverImage.FileName)}";
                var filePath = Path.Combine("wwwroot/images/books", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.CoverImage.CopyToAsync(stream);
                }

                if (!string.IsNullOrEmpty(book.CoverImageUrl))
                {
                    var oldFilePath = Path.Combine("wwwroot", book.CoverImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                book.CoverImageUrl = $"/images/books/{fileName}";
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "הספר עודכן בהצלחה";
            return RedirectToAction(nameof(ManageBooks));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
                return NotFound();

            if (!string.IsNullOrEmpty(book.CoverImageUrl))
            {
                var filePath = Path.Combine("wwwroot", book.CoverImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            TempData["Success"] = "הספר נמחק בהצלחה";
            return RedirectToAction(nameof(ManageBooks));
        }

        public async Task<IActionResult> ManageUsers()
        {
            var users = await _userManager.Users
                .Select(u => new UserManagementViewModel
                {
                    Id = u.Id.ToString(),
                    UserName = u.UserName!,
                    Email = u.Email!,
                    BorrowedBooksCount = u.Books.Count(b => b.IsBorrowed),
                    IsLocked = u.LockoutEnd != null && u.LockoutEnd > DateTimeOffset.UtcNow
                })
                .ToListAsync();

            return View(users);  // נוודא שה-View תואם לרשימת המשתמשים
        }


        [HttpGet]
        public async Task<IActionResult> WaitingList()
        {
            var waitingList = await _context.WaitingList
                .Include(w => w.User)
                .Include(w => w.Book)
                .Select(w => new WaitingListManagementViewModel
                {
                    BookId = w.BookId,
                    BookTitle = w.Book.Title,
                    WaitingUsers = new List<WaitingListItemViewModel>
                    {
                new WaitingListItemViewModel
                {
                    UserId = w.UserId.ToString(),
                    UserName = w.User.UserName ?? "לא ידוע",
                    JoinDate = w.JoinDate,
                    IsNotified = w.IsNotified,
                    Format = w.Format
                }
                    }
                })
                .ToListAsync();

            // מיזוג רשומות לפי ספר
            var groupedList = waitingList
                .GroupBy(w => w.BookId)
                .Select(g => new WaitingListManagementViewModel
                {
                    BookId = g.Key,
                    BookTitle = g.First().BookTitle,
                    WaitingUsers = g.SelectMany(w => w.WaitingUsers)
                        .OrderBy(u => u.JoinDate)
                        .ToList()
                })
                .ToList();

            return View(groupedList);
        }
        [HttpPost]


        public async Task<IActionResult> RemoveFromWaitingList(int bookId, string userId)
        {
            var waitingUser = await _context.WaitingList
                .FirstOrDefaultAsync(w => w.BookId == bookId && w.UserId == int.Parse(userId));

            if (waitingUser == null)
                return NotFound();

            _context.WaitingList.Remove(waitingUser);
            await _context.SaveChangesAsync();

            return Ok();
        }

        public async Task<IActionResult> ManageDiscounts()
        {
            var books = await _context.Books.OrderBy(b => b.Title).ToListAsync();
            return View(books);
        }

        [HttpPost]
        public async Task<IActionResult> ApplyDiscount(int bookId, decimal discountedPrice, DateTime endDate)
        {
            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
                return NotFound();

            book.OriginalPrice = book.BuyPrice;
            book.BuyPrice = discountedPrice;
            book.DiscountEndDate = endDate;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ManageDiscounts));
        }

        [HttpPost]
        public async Task<IActionResult> RemoveDiscount(int bookId)
        {
            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
                return NotFound();

            if (book.OriginalPrice.HasValue)
            {
                book.BuyPrice = book.OriginalPrice.Value;
                book.OriginalPrice = null;
                book.DiscountEndDate = null;

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(ManageDiscounts));
        }
        [HttpPost]
        public async Task<IActionResult> NotifyWaitingUser(int bookId, string userId)
        {
            var waitingUser = await _context.WaitingList
                .FirstOrDefaultAsync(w => w.BookId == bookId && w.UserId == int.Parse(userId));

            if (waitingUser == null)
                return NotFound();

            waitingUser.IsNotified = true;
            _context.Update(waitingUser);
            await _context.SaveChangesAsync();

            return Ok(new { message = "התראה נשלחה בהצלחה!" });
        }


    }
}

