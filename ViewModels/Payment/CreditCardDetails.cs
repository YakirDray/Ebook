using System.ComponentModel.DataAnnotations;

namespace MyEBookLibrary.ViewModels.Payment
{
    public class CreditCardDetails
    {
        private string _cardNumber = string.Empty;
        private string _expiryMonth = string.Empty;
        private string _expiryYear = string.Empty;
        private string _cvv = string.Empty;
        private string _cardHolderName = string.Empty;

        [Required(ErrorMessage = "יש להזין מספר כרטיס")]
        [CreditCard(ErrorMessage = "מספר כרטיס לא תקין")]
        [Display(Name = "מספר כרטיס")]
        public string CardNumber
        {
            get => _cardNumber;
            init => _cardNumber = value;
        }

        [Required(ErrorMessage = "יש להזין חודש תפוגה")]
        [RegularExpression(@"^(0[1-9]|1[0-2])$", ErrorMessage = "חודש תפוגה לא תקין")]
        [Display(Name = "חודש תפוגה")]
        public string ExpiryMonth
        {
            get => _expiryMonth;
            init => _expiryMonth = value;
        }

        [Required(ErrorMessage = "יש להזין שנת תפוגה")]
        [RegularExpression(@"^[0-9]{2}$", ErrorMessage = "שנת תפוגה לא תקינה")]
        [Display(Name = "שנת תפוגה")]
        public string ExpiryYear
        {
            get => _expiryYear;
            init => _expiryYear = value;
        }

        [Required(ErrorMessage = "יש להזין קוד אבטחה")]
        [RegularExpression(@"^[0-9]{3,4}$", ErrorMessage = "קוד אבטחה לא תקין")]
        [Display(Name = "קוד אבטחה (CVV)")]
        public string CVV
        {
            get => _cvv;
            init => _cvv = value;
        }

        [Required(ErrorMessage = "יש להזין שם בעל הכרטיס")]
        [Display(Name = "שם בעל הכרטיס")]
        public string CardHolderName
        {
            get => _cardHolderName;
            init => _cardHolderName = value;
        }

        // קונסטרקטור ריק לאפשר יצירת אובייקט ריק
        public CreditCardDetails()
        {
        }

        // קונסטרקטור מלא לאתחול כל השדות
        public CreditCardDetails(string cardNumber, string expiryMonth, string expiryYear,
                               string cvv, string cardHolderName)
        {
            _cardNumber = cardNumber;
            _expiryMonth = expiryMonth;
            _expiryYear = expiryYear;
            _cvv = cvv;
            _cardHolderName = cardHolderName;
        }

        public bool IsExpired()
        {
            if (int.TryParse(ExpiryYear, out int year) &&
                int.TryParse(ExpiryMonth, out int month))
            {
                var expiryDate = new DateTime(2000 + year, month, 1).AddMonths(1).AddDays(-1);
                return expiryDate < DateTime.UtcNow;
            }
            return true;
        }
    }
}