using MyEBookLibrary.Models;

namespace MyEBookLibrary.Services.Interfaces
{
    public interface IEmailNotificationService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendBookAvailableNotificationAsync(User user, Book book);
        Task SendReturnReminderAsync(User user, Book book, DateTime returnDate);
        Task SendBookAvailabilityNotificationAsync(string userId, int bookId, BookFormat format);
        Task SendBorrowExtensionNotificationAsync(string userId, int bookId, DateTime newDueDate);
        Task SendOrderConfirmationAsync(string email, List<CartItem> cartItems, decimal total);
    }
}