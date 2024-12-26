using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyEBookLibrary.Data;
using MyEBookLibrary.Models;

namespace MyEBookLibrary.Controllers
{
    public class BorrowController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BorrowController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var borrows = await _context.UserBooks
                .Include(b => b.User)
                .Include(b => b.Book)
                .Where(b => b.IsBorrowed)
                .Select(b => new
                {
                    UserName = b.User.UserName,
                    BookTitle = b.Book.Title,
                    BorrowDate = b.BorrowDate ?? DateTime.MinValue,
                    ReturnDate = b.ReturnDate,
                    IsReturned = b.IsReturned
                })
                .ToListAsync();

            return View(borrows);
        }
    }
}