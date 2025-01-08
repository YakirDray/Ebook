using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyEBookLibrary.Models;
using MyEBookLibrary.Services.Interfaces;

namespace MyEBookLibrary.Controllers
{
    [Authorize]
    public class BorrowController(
        ILibraryService libraryService,
        UserManager<User> userManager,
        ILogger<BorrowController> logger) : Controller
    {
        private readonly ILibraryService _libraryService = libraryService;
        private readonly UserManager<User> _userManager = userManager;
        private readonly ILogger<BorrowController> _logger = logger;

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var activeBorrows = await _libraryService.GetAllActiveBorrowsAsync();
                return View(activeBorrows);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active borrows");
                TempData["Error"] = "אירעה שגיאה בטעינת ההשאלות הפעילות";
                return RedirectToAction("Index", "Admin");
            }
        }

        public async Task<IActionResult> MyBorrows()
        {
            try
            {
                var userId = int.Parse(_userManager.GetUserId(User)!);
                var borrowedBooks = await _libraryService.GetUserBorrowedBooksAsync(userId);
                return View(borrowedBooks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving borrowed books for user {UserId}", User.Identity?.Name);
                TempData["Error"] = "אירעה שגיאה בטעינת רשימת ההשאלות";
                return RedirectToAction("Index", "Home");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Return(int id)
        {
            try
            {
                var userId = int.Parse(_userManager.GetUserId(User)!);
                var result = await _libraryService.ReturnBookAsync(userId, id);

                if (result)
                {
                    TempData["Success"] = "הספר הוחזר בהצלחה";
                }
                else
                {
                    TempData["Error"] = "לא ניתן להחזיר את הספר";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error returning book {BookId} by user {UserId}", id, User.Identity?.Name);
                TempData["Error"] = "אירעה שגיאה בהחזרת הספר";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Extend(int id)
        {
            try
            {
                var userId = int.Parse(_userManager.GetUserId(User)!);
                var result = await _libraryService.ExtendBorrowAsync(userId, id);

                if (result)
                {
                    TempData["Success"] = "תקופת ההשאלה הוארכה בהצלחה";
                }
                else
                {
                    TempData["Error"] = "לא ניתן להאריך את תקופת ההשאלה";
                }

                // Redirect to different actions based on user role
                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction(nameof(Index));
                }
                return RedirectToAction(nameof(MyBorrows));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extending borrow {BorrowId} for user {UserId}", id, User.Identity?.Name);
                TempData["Error"] = "אירעה שגיאה בהארכת תקופת ההשאלה";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}