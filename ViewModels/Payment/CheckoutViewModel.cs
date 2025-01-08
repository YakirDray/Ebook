namespace MyEBookLibrary.ViewModels.Payment
{
    public class CheckoutViewModel : PaymentBaseViewModel
    {
        public required CreditCardDetails CardDetails { get; set; }
        public bool AcceptTerms { get; set; }  // אישור תנאי שימוש
        public string? PromoCode { get; set; }  // קוד קופון
        public decimal Discount { get; set; }
        public bool ShowVAT { get; set; }
        public bool SaveCard { get; internal set; }
    }
}