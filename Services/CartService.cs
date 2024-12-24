using Microsoft.EntityFrameworkCore;
using MyEBookLibrary.Controllers;
using MyEBookLibrary.Data;
using MyEBookLibrary.Models;
using MyEBookLibrary.Services.Interfaces;
using Stripe;

namespace MyEBookLibrary.Services
{
    public class CartService(ApplicationDbContext context) : ICartService
    {
        private readonly ApplicationDbContext _context = context;
        private readonly ILogger<CartService>? _logger;

        public CartService(ApplicationDbContext context, ILogger<CartService> logger) : this(context)
        {
            _logger = logger;
        }
        public async Task<ShoppingCart> GetOrCreateCartAsync(string userId)
        {
            var cart = await _context.ShoppingCarts
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
                _context.ShoppingCarts.Add(cart);
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
        public async Task<bool> ProcessCartAsync(string userId, PaymentInfo paymentInfo)
        {
            if (string.IsNullOrEmpty(userId))
            {
                _logger?.LogWarning("ProcessCartAsync was called with null or empty userId");
                return false;
            }

            if (string.IsNullOrEmpty(paymentInfo?.StripeToken))
            {
                _logger?.LogWarning("Invalid payment info: Missing Stripe token");
                return false;
            }

            var cart = await GetOrCreateCartAsync(userId);

            if (cart == null || !cart.Items.Any())
            {
                _logger?.LogWarning($"Empty or null cart for user {userId}");
                return false;
            }

            try
            {
                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(cart.Total * 100),
                    Currency = "ils",
                    PaymentMethod = paymentInfo.StripeToken,
                    Confirm = true,
                    AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                    {
                        Enabled = true,
                    }
                };

                var service = new PaymentIntentService();
                var intent = await service.CreateAsync(options);

                if (intent.Status == "succeeded")
                {
                    cart.Status = CartStatus.Completed;
                    cart.CheckoutAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    _logger?.LogInformation($"Payment processed successfully for cart {cart.Id}");
                    return true;
                }

                _logger?.LogWarning($"Payment failed for cart {cart.Id}. Status: {intent.Status}");
                return false;
            }
            catch (StripeException stripeEx)
            {
                _logger?.LogError(stripeEx, $"Stripe payment processing failed for cart {cart.Id}. Error: {stripeEx.Message}");
                return false;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Payment processing failed for cart {cart.Id}");
                return false;
            }
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



        public Task<ShoppingCart> GetCartAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CompleteOrderAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsBookAvailableAsync(int bookId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateCartStatusAsync(string userId, CartStatus status)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetCartItemCountAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<CartItem?> GetCartItemAsync(string userId, int bookId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ProcessCartAsync(string? userId, object paymentInfo)
        {
            throw new NotImplementedException();
        }
    }
}
