using System.ComponentModel.DataAnnotations;

namespace MyEBookLibrary.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "נדרש שם משתמש")]
        [Display(Name = "שם משתמש")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "נדרשת סיסמה")]
        [DataType(DataType.Password)]
        [Display(Name = "סיסמה")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "זכור אותי")]
        public bool RememberMe { get; set; }
    }
}