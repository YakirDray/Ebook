using MyEBookLibrary.Models;

namespace MyEBookLibrary.Services.Interfaces
{
    public interface ICartService
    {
        Task<ShoppingCart> GetOrCreateCartAsync(int userId);
        Task<bool> AddToCartAsync(int userId, int bookId, bool isBorrow, BookFormat format);
        Task<bool> RemoveFromCartAsync(int userId, int bookId);
        Task<bool> ProcessCartAsync(int userId, PaymentInfo paymentInfo);
        Task ClearCartAsync(int userId);
        Task<IEnumerable<CartItem>> GetCartItemsAsync(int userId);
        Task<decimal> GetCartTotalAsync(int userId);
        Task<bool> UpdateQuantityAsync(int userId, int bookId, int quantity);
        Task<ShoppingCart> GetCartAsync(int userId);
        Task<bool> CompleteOrderAsync(int userId);
        Task<bool> UpdateCartStatusAsync(int userId, CartStatus status);
        Task<int> GetCartItemCountAsync(int userId);
        Task<CartItem?> GetCartItemAsync(int userId, int bookId);
    }
}