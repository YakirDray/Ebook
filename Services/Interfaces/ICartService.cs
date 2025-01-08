using MyEBookLibrary.Models;

namespace MyEBookLibrary.Services.Interfaces
{
    public interface ICartService
    {
        // פעולות בסיסיות של העגלה
        Task<ShoppingCart> GetOrCreateCartAsync(int userId);
        Task<ShoppingCart> GetCartAsync(int userId);
        Task ClearCartAsync(int userId);
        Task<bool> UpdateCartStatusAsync(int userId, CartStatus status);

        // ניהול פריטים בעגלה
        Task<bool> AddToCartAsync(int userId, int bookId, bool isBorrow, BookFormat format);
        Task<bool> RemoveFromCartAsync(int userId, int bookId);
        Task<bool> UpdateQuantityAsync(int userId, int bookId, int quantity);
        Task<CartItem?> GetCartItemAsync(int userId, int bookId);
        Task<IEnumerable<CartItem>> GetCartItemsAsync(int userId);
        Task<int> GetCartItemCountAsync(int userId);

        // חישובי סכומים
        Task<decimal> GetCartTotalAsync(int userId);
        Task<decimal> GetDiscountedTotalAsync(int userId, string? promoCode = null);
        Task<decimal> CalculateVATAsync(int userId);

        // תהליך רכישה והזמנה
        Task<PaymentResult> ProcessCartAsync(int userId, PaymentInfo paymentInfo);
        Task<bool> ValidateCartItemsAsync(int userId);
        Task<bool> CompleteOrderAsync(int userId);
        Task<bool> CancelOrderAsync(int userId, string reason);

        // בדיקות זמינות והגבלות
        Task<bool> ValidateBorrowLimitAsync(int userId);
        Task<bool> CheckBookAvailabilityAsync(int bookId, bool isBorrow);
        Task<bool> ValidateFormatAvailabilityAsync(int bookId, BookFormat format);

        // ניהול שמירת עגלה ושחזור
        Task<bool> SaveCartForLaterAsync(int userId);
        Task<bool> RestoreSavedCartAsync(int userId);
        Task<bool> MergeCartsAsync(int sourceUserId, int targetUserId);

        // דוחות ומידע
        Task<CartSummary> GetCartSummaryAsync(int userId);
        Task<IEnumerable<CartAuditLog>> GetCartHistoryAsync(int userId);
        Task<OrderEstimate> GetOrderEstimateAsync(int userId);
        Task<int> GetActiveUserBorrowsCountAsync(int userId);
        Task<int> GetAvailableCopiesCountAsync(int bookId);

        Task<bool> ValidateCartAsync(int userId);
    }

    public class CartSummary
    {
        public int TotalItems { get; set; }
        public int BorrowedItems { get; set; }
        public int PurchasedItems { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Discount { get; set; }
        public decimal VAT { get; set; }
        public decimal Total { get; set; }
        public bool HasBorrowLimitReached { get; set; }
        public List<string> Warnings { get; set; } = [];
        public Dictionary<string, decimal> Breakdowns { get; set; } = [];
    }

    public class OrderEstimate
    {
        public decimal SubTotal { get; set; }
        public decimal Discount { get; set; }
        public decimal VAT { get; set; }
        public decimal Total { get; set; }
        public string Currency { get; set; } = "ILS";
        public DateTime EstimatedDeliveryDate { get; set; }
        public List<OrderEstimateItem> Items { get; set; } = [];
        public List<string> AppliedDiscounts { get; set; } = [];
    }

    public class OrderEstimateItem
    {
        public int BookId { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal OriginalPrice { get; set; }
        public decimal FinalPrice { get; set; }
        public decimal Discount { get; set; }
        public bool IsBorrow { get; set; }
        public BookFormat Format { get; set; }
        public bool IsAvailable { get; set; }
        public string? UnavailabilityReason { get; set; }
    }

    public class CartAuditLog
    {
        public DateTime Timestamp { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public CartStatus OldStatus { get; set; }
        public CartStatus NewStatus { get; set; }
        public decimal? AmountChanged { get; set; }

    }
}