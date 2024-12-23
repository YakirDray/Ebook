using MyEBookLibrary.Models;

namespace MyEBookLibrary.Services.Interfaces
{
    public interface ICartService
    {
        Task<ShoppingCart> GetOrCreateCartAsync(string userId);
        Task<bool> AddToCartAsync(string userId, int bookId, bool isBorrow, BookFormat format);
        Task<bool> UpdateQuantityAsync(string userId, int bookId, int quantity);
        Task<bool> RemoveFromCartAsync(string userId, int bookId);
        Task ClearCartAsync(string userId);
        Task<bool> ProcessCartAsync(string userId, PaymentInfo paymentInfo);
        Task<IEnumerable<CartItem>> GetCartItemsAsync(string userId);
        Task<decimal> GetCartTotalAsync(string userId);
            Task<ShoppingCart> GetCartAsync(string userId);  // Add this if it's used in your controller
                Task<bool> CompleteOrderAsync(string userId);  // Example signature


    }
}