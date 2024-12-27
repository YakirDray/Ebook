// Services/EmailNotificationService.cs
using System.Net;
using System.Net.Mail;
using MyEBookLibrary.Data;
using MyEBookLibrary.Models;
using MyEBookLibrary.Services.Interfaces;

namespace MyEBookLibrary.Services
{
    public class EmailNotificationService : IEmailNotificationService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailNotificationService> _logger;
        private readonly ApplicationDbContext _context;

        public EmailNotificationService(
            IConfiguration configuration,
            ILogger<EmailNotificationService> logger,
            ApplicationDbContext context)

        {
            _configuration = configuration;
            _logger = logger;
            _context = context;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                var fromEmail = _configuration["EmailSettings:FromEmail"] ??
                    throw new InvalidOperationException("FromEmail is not configured in EmailSettings");
                var smtpHost = _configuration["EmailSettings:SmtpHost"];
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "25");

                var smtpUser = _configuration["EmailSettings:SmtpUser"] ?? throw new InvalidOperationException("SmtpUser is not configured in EmailSettings");
                var smtpPass = _configuration["EmailSettings:SmtpPass"] ?? throw new InvalidOperationException("SmtpPass is not configured in EmailSettings");

                var message = new MailMessage
                {
                    From = new MailAddress(fromEmail),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                message.To.Add(to);

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(smtpUser, smtpPass),
                    EnableSsl = true
                };

                await client.SendMailAsync(message);
                _logger.LogInformation($"Email sent successfully to {to}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email");
                throw;
            }
        }
        public async Task SendBookAvailabilityNotificationAsync(string userId, int bookId, BookFormat format)
        {
            var user = await _context.Users.FindAsync(userId);
            var book = await _context.Books.FindAsync(bookId);

            if (user == null || book == null) return;

            var subject = $"הספר {book.Title} זמין כעת בפורמט {format}";
            var body = $@"
        <h2>שלום {user.UserName},</h2>
        <p>הספר שביקשת {book.Title} בפורמט {format} זמין כעת.</p>
        <p>אנא היכנס למערכת והשלם את תהליך ההשאלה או הרכישה.</p>";

            await SendEmailAsync(user.Email!, subject, body);
        }

        public async Task SendBorrowExtensionNotificationAsync(string userId, int bookId, DateTime newDueDate)
        {
            var user = await _context.Users.FindAsync(userId);
            var book = await _context.Books.FindAsync(bookId);

            if (user == null || book == null) return;

            var subject = $"תקופת השאלה עבור {book.Title} הוארכה";
            var body = $@"
        <h2>שלום {user.UserName},</h2>
        <p>תקופת ההשאלה של הספר {book.Title} הוארכה עד ל-{newDueDate:dd/MM/yyyy}.</p>
        <p>אנא ודא כי תאריך ההחזרה החדש מעודכן בפרופיל שלך.</p>";

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

        public Task SendBookAvailableNotificationAsync(User user, Book book)
        {
            throw new NotImplementedException();
        }

        public Task SendOrderConfirmationAsync(string email, List<CartItem> cartItems, decimal total)
        {
            throw new NotImplementedException();
        }
    }
}