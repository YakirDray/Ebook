using MyEBookLibrary.Models;

namespace MyEBookLibrary.ViewModels.Payment
{
    public abstract class PaymentBaseViewModel
    {
        // מידע בסיסי
        public decimal Amount { get; set; }  // סכום לתשלום
        public List<CartItem> Items { get; set; } = [];  // פריטים בעגלה
        public PaymentMethod PaymentMethod { get; set; }  // שיטת תשלום
        public string OrderId { get; set; } = Guid.NewGuid().ToString();  // מספר הזמנה ייחודי

        // חישובי סכומים
        public decimal Total { get; set; }
        public decimal VAT => Subtotal * 0.17m;  // מע"מ 17%

        // חישובים לפי סוג פריט
        public IEnumerable<CartItem> BorrowedItems => Items.Where(item => item.IsBorrow);
        public IEnumerable<CartItem> PurchasedItems => Items.Where(item => !item.IsBorrow);

        public decimal Subtotal { get; private set; }

    }
}