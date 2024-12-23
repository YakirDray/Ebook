using Microsoft.EntityFrameworkCore;
using MyEBookLibrary.Data;
using MyEBookLibrary.Models;
using MyEBookLibrary.Services.Interfaces;

namespace MyEBookLibrary.Services
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILibraryService _libraryService;

        public CartService(ApplicationDbContext context, ILibraryService libraryService)
        {
            _context = context;
            _libraryService = libraryService;
        }

        public async Task<ShoppingCart> GetOrCreateCartAsync(string userId)
        {
            var cart = await _context.Set<ShoppingCart>()
                .Include(c => c.Items)
                .ThenInclude(i => i.Book)
                .FirstOrDefaultAsync(c => c.UserId == userId && c.Status == CartStatus.Active);

            if (cart == null)
            {
                cart = new ShoppingCart
                {
                    UserId = userId,
                    Status = CartStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    Items = new List<CartItem>()
                };
                _context.Set<ShoppingCart>().Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        public async Task<bool> AddToCartAsync(string userId, int bookId, bool isBorrow, BookFormat format)
        {
            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
                return false;

            var cart = await GetOrCreateCartAsync(userId);
            var existingItem = cart.Items.FirstOrDefault(i => i.BookId == bookId);
            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                cart.Items.Add(new CartItem
                {
                    BookId = bookId,
                    Title = book.Title,
                    Price = isBorrow ? book.BorrowPrice : book.BuyPrice,
                    Format = format,
                    IsBorrow = isBorrow,
                    Book = book,
                    Quantity = 1
                });
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveFromCartAsync(string userId, int bookId)
        {
            var cart = await GetOrCreateCartAsync(userId);
            var item = cart.Items.FirstOrDefault(i => i.BookId == bookId);
            if (item != null)
            {
                cart.Items.Remove(item);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task ClearCartAsync(string userId)
        {
            var cart = await GetOrCreateCartAsync(userId);
            cart.Items.Clear();
            await _context.SaveChangesAsync();
        }

        public async Task<(bool Success, string Message)> ProcessCartAsync(string userId, PaymentInfo paymentInfo)
        {
            var cart = await GetOrCreateCartAsync(userId);

            // Check if the cart has items
            if (cart == null || !cart.Items.Any())
            {
                return (false, "The cart is empty.");
            }

            // Simulate some processing logic, for example, payment processing
            try
            {
                // Assume ProcessPaymentAsync is a method that processes the payment
                var paymentResult = ProcessPayment(paymentInfo); // This method should be implemented

                if (paymentResult.Success)
                {
                    cart.Status = CartStatus.Completed;
                    cart.CheckoutAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return (true, "Payment processed successfully.");
                }
                else
                {
                    return (false, "Payment failed.");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error processing payment: {ex.Message}");
            }
        }

        private (bool Success, string Message) ProcessPayment(PaymentInfo paymentInfo)
        {
            // Implement your payment processing logic here
            // For now, just a placeholder return statement
            return (true, "Payment successful"); // Simulate success
        }

        public async Task<IEnumerable<CartItem>> GetCartItemsAsync(string userId)
        {
            var cart = await GetOrCreateCartAsync(userId);
            return cart.Items;
        }

        public async Task<decimal> GetCartTotalAsync(string userId)
        {
            var cart = await GetOrCreateCartAsync(userId);
            return cart.Total;
        }

        public Task<bool> UpdateQuantityAsync(string userId, int bookId, int quantity)
        {
            throw new NotImplementedException();
        }

        Task<bool> ICartService.ProcessCartAsync(string userId, PaymentInfo paymentInfo)
        {
            throw new NotImplementedException();
        }

        public Task<ShoppingCart> GetCartAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CompleteOrderAsync(string userId)
        {
            throw new NotImplementedException();
        }
    }
}
