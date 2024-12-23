using System.ComponentModel.DataAnnotations;

namespace MyEBookLibrary.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "נדרש שם משתמש")]
        [Display(Name = "שם משתמש")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "נדרשת כתובת אימייל")]
        [EmailAddress(ErrorMessage = "כתובת אימייל לא תקינה")]
        [Display(Name = "אימייל")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "נדרשת סיסמה")]
        [StringLength(100, ErrorMessage = "הסיסמה חייבת להכיל לפחות {2} תווים", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "סיסמה")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "אימות סיסמה")]
        [Compare("Password", ErrorMessage = "הסיסמאות אינן תואמות")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}