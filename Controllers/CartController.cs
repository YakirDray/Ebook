using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyEBookLibrary.Models;
using MyEBookLibrary.Services.Interfaces;
using MyEBookLibrary.ViewModels.Payment;

namespace MyEBookLibrary.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly UserManager<User> _userManager;

        public CartController(ICartService cartService, UserManager<User> userManager)
        {
            _cartService = cartService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }
            var cart = await _cartService.GetOrCreateCartAsync(userId);
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int bookId, BookFormat format, bool isBorrow)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "User not authenticated" });
            }
            var result = await _cartService.AddToCartAsync(userId, bookId, isBorrow, format);
            if (!result)
            {
                return Json(new { success = false, message = "Failed to add the book to the cart" });
            }

            var cart = await _cartService.GetOrCreateCartAsync(userId);
            return Json(new { success = true, message = "Book added to cart successfully", cartItemsCount = cart.Items.Count });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int bookId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }
            await _cartService.RemoveFromCartAsync(userId, bookId);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
public async Task<IActionResult> Checkout()
{
    var userId = _userManager.GetUserId(User);
    if (string.IsNullOrEmpty(userId))
        return RedirectToAction("Login", "Account");

    var cart = await _cartService.GetOrCreateCartAsync(userId);
    if (!cart.Items.Any())
        return RedirectToAction(nameof(Index));

    var viewModel = new CheckoutViewModel
    {
        Items = cart.Items,
        Total = cart.Total,
        PaymentMethod = PaymentMethod.CreditCard,
        CardDetails = new CreditCardDetails()
    };

    return View(viewModel);
}

[HttpPost]
public async Task<IActionResult> ProcessPayment(CheckoutViewModel model)
{
    var userId = _userManager.GetUserId(User);
    if (userId == null)
        return RedirectToAction("Login", "Account");

    if (!ModelState.IsValid)
        return View("Checkout", model);

    var cart = await _cartService.GetOrCreateCartAsync(userId);
    if (!cart.Items.Any())
        return RedirectToAction(nameof(Index));

    var paymentInfo = new PaymentInfo
    {
        UserId = userId,
        CardNumber = model.CardDetails.CardNumber,
        ExpiryMonth = model.CardDetails.ExpiryMonth,
        ExpiryYear = model.CardDetails.ExpiryYear,
        CVV = model.CardDetails.CVV,
        CardHolderName = model.CardDetails.CardHolderName,
        Amount = cart.Total,
        Method = model.PaymentMethod,
        Currency = "ILS",
        Status = PaymentStatus.Pending
    };

    var success = await _cartService.ProcessCartAsync(userId, paymentInfo);
    if (!success)
    {
        ModelState.AddModelError("", "אירעה שגיאה בעיבוד התשלום");
        return View("Checkout", model);
    }

    // יצירת אישור תשלום
    var confirmation = new PaymentConfirmationViewModel
    {
        Amount = cart.Total,
        PurchasedItems = cart.Items.ToList(),
        TransactionId = DateTime.Now.Ticks.ToString(),
        PurchaseDate = DateTime.UtcNow
    };

    await _cartService.ClearCartAsync(userId);
    
    return View("PaymentConfirmation", confirmation);
}
       
    }

}
