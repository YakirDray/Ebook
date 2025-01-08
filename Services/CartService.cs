using Microsoft.EntityFrameworkCore;
using MyEBookLibrary.Data;
using MyEBookLibrary.Models;
using MyEBookLibrary.Services.Interfaces;
using Stripe;

namespace MyEBookLibrary.Services
{
    public class CartService(ApplicationDbContext context, ILogger<CartService> logger) : ICartService
    {
        private readonly ApplicationDbContext _context = context;
        private readonly ILogger<CartService>? _logger = logger;

        public async Task<ShoppingCart> GetOrCreateCartAsync(int userId)
        {
            var userIdString = userId.ToString();
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
                    Items = []
                };
                _context.ShoppingCarts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        public async Task<bool> AddToCartAsync(int userId, int bookId, bool isBorrow, BookFormat format)
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

        public async Task<bool> RemoveFromCartAsync(int userId, int bookId)
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

        public async Task ClearCartAsync(int userId)
        {
            var cart = await GetOrCreateCartAsync(userId);
            cart.Items.Clear();
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetActiveUserBorrowsCountAsync(int userId)
        {
            return await _context.UserBooks
                .CountAsync(ub =>
                    ub.UserId == userId &&
                    ub.IsBorrowed &&
                    !ub.ReturnDate.HasValue);
        }

        public async Task<int> GetAvailableCopiesCountAsync(int bookId)
        {
            var totalBorrowed = await _context.UserBooks
                .CountAsync(ub =>
                    ub.BookId == bookId &&
                    ub.IsBorrowed &&
                    !ub.ReturnDate.HasValue);

            // מקסימום 3 עותקים להשאלה
            return Math.Max(0, 3 - totalBorrowed);
        }

        public async Task<bool> ValidateCartAsync(int userId)
        {
            var cart = await GetOrCreateCartAsync(userId);
            var borrowItems = cart.Items.Where(i => i.IsBorrow).ToList();

            // בדיקת מגבלת השאלות למשתמש
            if (borrowItems.Any())
            {
                var currentBorrows = await GetActiveUserBorrowsCountAsync(userId);
                if (currentBorrows + borrowItems.Count > 3)
                {
                    return false;
                }
            }

            // בדיקת זמינות ספרים
            foreach (var item in borrowItems)
            {
                var availableCopies = await GetAvailableCopiesCountAsync(item.BookId);
                if (availableCopies <= 0)
                {
                    return false;
                }
            }

            return true;
        }
        public async Task<bool> ProcessCartAsync(int userId, PaymentInfo paymentInfo)
        {
            if (paymentInfo?.StripeToken == null)
            {
                _logger?.LogWarning("Invalid payment info: Missing Stripe token");
                return false;
            }

            var cart = await GetOrCreateCartAsync(userId);

            if (!cart.Items.Any())
            {
                _logger?.LogWarning($"Empty cart for user {userId}");
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
                _logger?.LogError(stripeEx, $"Stripe payment processing failed for cart {cart.Id}");
                return false;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Payment processing failed for cart {cart.Id}");
                return false;
            }
        }

        public async Task<IEnumerable<CartItem>> GetCartItemsAsync(int userId)
        {
            var cart = await GetOrCreateCartAsync(userId);
            return cart.Items;
        }

        public async Task<decimal> GetCartTotalAsync(int userId)
        {
            var cart = await GetOrCreateCartAsync(userId);
            return cart.Total;
        }

        public async Task<bool> UpdateQuantityAsync(int userId, int bookId, int quantity)
        {
            if (quantity < 0) return false;

            var cart = await GetOrCreateCartAsync(userId);
            var item = cart.Items.FirstOrDefault(i => i.BookId == bookId);

            if (item == null) return false;

            if (quantity == 0)
            {
                return await RemoveFromCartAsync(userId, bookId);
            }

            item.Quantity = quantity;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ShoppingCart> GetCartAsync(int userId)
        {
            return await GetOrCreateCartAsync(userId);
        }

        public async Task<bool> CompleteOrderAsync(int userId)
        {
            var cart = await GetOrCreateCartAsync(userId);
            if (!cart.Items.Any()) return false;

            cart.Status = CartStatus.Completed;
            cart.CheckoutAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateCartStatusAsync(int userId, CartStatus status)
        {
            var cart = await GetOrCreateCartAsync(userId);
            cart.Status = status;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetCartItemCountAsync(int userId)
        {
            var cart = await GetOrCreateCartAsync(userId);
            return cart.Items.Count;
        }

        public async Task<CartItem?> GetCartItemAsync(int userId, int bookId)
        {
            var cart = await GetOrCreateCartAsync(userId);
            return cart.Items.FirstOrDefault(i => i.BookId == bookId);
        }

        public Task<decimal> GetDiscountedTotalAsync(int userId, string? promoCode = null)
        {
            throw new NotImplementedException();
        }

        public Task<decimal> CalculateVATAsync(int userId)
        {
            throw new NotImplementedException();
        }

        Task<PaymentResult> ICartService.ProcessCartAsync(int userId, PaymentInfo paymentInfo)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ValidateCartItemsAsync(int userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CancelOrderAsync(int userId, string reason)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ValidateBorrowLimitAsync(int userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CheckBookAvailabilityAsync(int bookId, bool isBorrow)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ValidateFormatAvailabilityAsync(int bookId, BookFormat format)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SaveCartForLaterAsync(int userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RestoreSavedCartAsync(int userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> MergeCartsAsync(int sourceUserId, int targetUserId)
        {
            throw new NotImplementedException();
        }

        public Task<CartSummary> GetCartSummaryAsync(int userId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<CartAuditLog>> GetCartHistoryAsync(int userId)
        {
            throw new NotImplementedException();
        }

        public Task<OrderEstimate> GetOrderEstimateAsync(int userId)
        {
            throw new NotImplementedException();
        }


    }
}