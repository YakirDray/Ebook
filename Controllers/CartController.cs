using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyEBookLibrary.Models;
using MyEBookLibrary.Services.Interfaces;
using MyEBookLibrary.ViewModels.Payment;
using Stripe;

namespace MyEBookLibrary.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;

        public CartController(ICartService cartService, UserManager<User> userManager, IConfiguration configuration)
        {
            _cartService = cartService;
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(_userManager.GetUserId(User)!);
            var cart = await _cartService.GetOrCreateCartAsync(userId);
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int bookId, BookFormat format, bool isBorrow)
        {
            var userId = int.Parse(_userManager.GetUserId(User)!);
            var result = await _cartService.AddToCartAsync(userId, bookId, isBorrow, format);

            if (!result)
            {
                return Json(new { success = false, message = "Failed to add the book to the cart" });
            }

            var cart = await _cartService.GetOrCreateCartAsync(userId);
            return Json(new
            {
                success = true,
                message = "Book added to cart successfully",
                cartItemsCount = cart.Items.Count
            });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int bookId)
        {
            var userId = int.Parse(_userManager.GetUserId(User)!);
            await _cartService.RemoveFromCartAsync(userId, bookId);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var userId = int.Parse(_userManager.GetUserId(User)!);
            var cart = await _cartService.GetOrCreateCartAsync(userId);

            var checkoutViewModel = new CheckoutViewModel
            {
                Items = cart.Items,
                CardDetails = new CreditCardDetails(),
            };

            ViewBag.StripePublicKey = _configuration["Stripe:PublishableKey"];
            return View(checkoutViewModel);
        }

        [HttpPost("checkout/test-payment")]
        public async Task<IActionResult> TestPayment(string stripeToken, decimal amount)
        {
            try
            {
                var options = new ChargeCreateOptions
                {
                    Amount = (long)(amount * 100),
                    Currency = "ils",
                    Source = stripeToken,
                    Description = "Test Payment"
                };

                var service = new ChargeService();
                var charge = await service.CreateAsync(options);

                if (charge.Status == "succeeded")
                {
                    return Json(new { success = true, message = "Payment Successful!" });
                }
                return Json(new { success = false, message = "Payment Failed!" });
            }
            catch (StripeException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Borrow(int id, BookFormat format)
        {
            if (!User.Identity!.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            var userIdString = _userManager.GetUserId(User);
            if (userIdString == null)
                return RedirectToAction("Login", "Account");

            var userId = int.Parse(userIdString);
            var result = await _cartService.AddToCartAsync(userId, id, true, format);

            if (result)
                return RedirectToAction("Checkout", "Cart");

            TempData["Error"] = "לא ניתן להוסיף את הספר לעגלה";
            return RedirectToAction("Details", new { id });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Purchase(int id, BookFormat format)
        {
            if (!User.Identity!.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            var userIdString = _userManager.GetUserId(User);
            if (userIdString == null)
                return RedirectToAction("Login", "Account");

            var userId = int.Parse(userIdString);
            var result = await _cartService.AddToCartAsync(userId, id, false, format);

            if (result)
                return RedirectToAction("Checkout", "Cart");

            TempData["Error"] = "לא ניתן להוסיף את הספר לעגלה";
            return RedirectToAction("Details", new { id });
        }
    }
}

