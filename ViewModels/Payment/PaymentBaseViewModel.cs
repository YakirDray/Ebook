// ViewModels/Payment/PaymentBaseViewModel.cs
using MyEBookLibrary.Models;

namespace MyEBookLibrary.ViewModels.Payment
{
    public abstract class PaymentBaseViewModel
    {
        public decimal Amount { get; set; }
        public List<CartItem> Items { get; set; } = new();
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public PaymentMethod PaymentMethod { get; set; }

        // מאפיינים מחושבים משותפים
        public decimal Total => Items.Sum(item => item.Price);
        public int ItemCount => Items.Count;
        public bool HasErrors => !string.IsNullOrEmpty(ErrorMessage);
    }
}