// ViewModels/CartViewModel.cs
using MyEBookLibrary.Models;

namespace MyEBookLibrary.ViewModels
{
    public class CartViewModel
    {
        public List<CartItem> Items { get; set; } = new();
    public decimal Total => Items.Sum(item => item.Subtotal);
        public int ItemCount => Items.Count;
        public bool IsEmpty => !Items.Any();
    }
}