using MyEBookLibrary.Models;

namespace MyEBookLibrary.Services.Interfaces
{
    public interface ILibraryService
    {
        Task<bool> BorrowBookAsync(int userId, int bookId, BookFormat format);
        Task<bool> ReturnBookAsync(int userId, int bookId);
        Task<bool> PurchaseBookAsync(int userId, int bookId, BookFormat format);
        Task<bool> AddToWaitingListAsync(int userId, int bookId, BookFormat format);
        Task<IEnumerable<UserBook>> GetUserBorrowedBooksAsync(int userId);
        Task<IEnumerable<UserBook>> GetUserPurchasedBooksAsync(int userId);
        Task<IEnumerable<WaitingListItem>> GetUserWaitingListAsync(int userId);
        Task<bool> IsBookAvailableAsync(int bookId, BookFormat format);
        Task<int> GetBorrowedBooksCountAsync(int userId);
        Task<bool> IsBookBorrowedByUserAsync(int userId, int bookId);
        Task<bool> IsBookPurchasedByUserAsync(int userId, int bookId);
        Task<bool> IsUserInWaitingListAsync(int userId, int bookId);
    }
}