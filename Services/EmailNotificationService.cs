// Services/EmailNotificationService.cs
using MyEBookLibrary.Models;
using MyEBookLibrary.Services.Interfaces;

namespace MyEBookLibrary.Services
{
    public class EmailNotificationService : IEmailNotificationService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailNotificationService> _logger;

        public EmailNotificationService(
            IConfiguration configuration,
            ILogger<EmailNotificationService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                // בשלב הפיתוח, נרשום רק ללוג
                _logger.LogInformation($"Sending email to {to}");
                _logger.LogInformation($"Subject: {subject}");
                _logger.LogInformation($"Body: {body}");

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email");
                throw;
            }
        }

        public async Task SendBookAvailableNotificationAsync(User user, Book book)
        {
            var subject = $"הספר {book.Title} זמין עכשיו!";
            var body = $@"
                <h2>שלום {user.UserName},</h2>
                <p>הספר {book.Title} שביקשת זמין כעת להשאלה.</p>
                <p>אנא היכנס למערכת כדי לבצע את ההשאלה.</p>";

            await SendEmailAsync(user.Email!, subject, body);
        }

        public async Task SendReturnReminderAsync(User user, Book book, DateTime returnDate)
        {
            var subject = $"תזכורת: החזרת הספר {book.Title}";
            var daysLeft = (returnDate - DateTime.Now).Days;
            var body = $@"
                <h2>שלום {user.UserName},</h2>
                <p>תזכורת ידידותית שעליך להחזיר את הספר {book.Title} בעוד {daysLeft} ימים.</p>
                <p>תאריך ההחזרה: {returnDate:dd/MM/yyyy}</p>";

            await SendEmailAsync(user.Email!, subject, body);
        }

        public Task SendBookAvailabilityNotificationAsync(string userId, int bookId, BookFormat format)
        {
            throw new NotImplementedException();
        }

        public Task SendBorrowExtensionNotificationAsync(string userId, int bookId, DateTime newDueDate)
        {
            throw new NotImplementedException();
        }
    }
}