using MyEBookLibrary.Models;
using System.ComponentModel.DataAnnotations;

namespace MyEBookLibrary.ViewModels
{
    public class WaitingListManagementViewModel
    {
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public List<WaitingListItemViewModel> WaitingUsers { get; set; } = new();
        public int AvailableCopies { get; set; }
        public DateTime? NextAvailableDate { get; set; }
    }

public class WaitingListItemViewModel
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "שם המשתמש נדרש.")]
    [StringLength(100)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    public DateTime JoinDate { get; set; }

    public bool IsNotified { get; set; }

    [EnumDataType(typeof(BookFormat))]
    public BookFormat Format { get; set; }

    public DateTime? EstimatedAvailabilityDate { get; set; }

    [EmailAddress(ErrorMessage = "כתובת אימייל לא חוקית")]
    public string? Email { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "מיקום ברשימת המתנה חייב להיות חיובי.")]
    public int WaitingPosition { get; set; }
    public object? Book { get; internal set; }
}

}