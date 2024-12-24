// Controllers/BorrowController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyEBookLibrary.Data;
using MyEBookLibrary.ViewModels;

namespace MyEBookLibrary.Controllers
{
    public class BorrowController(ApplicationDbContext context) : Controller
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<IActionResult> ActiveBorrows()
        {
            var activeBorrows = await _context.Borrows
                .Include(b => b.Book)
                .Include(b => b.User)
                .Where(b => b.ReturnDate == null || b.ReturnDate > DateTime.Now)
                .Select(static b => new BorrowHistoryViewModel
                {
                    BorrowId = b.Id,
                    UserName = b.UserName!,
                    BookTitle = b.Book.Title,
                    BorrowDate = b.BorrowDate,
                    ReturnDate = b.ReturnDate,
                    IsReturned = b.IsReturned,
                    IsLate = b.ReturnDate < DateTime.Now && !b.IsReturned
                }).ToListAsync();

            return View("ActiveBorrows", activeBorrows);
        }

    }
}
