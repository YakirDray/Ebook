

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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
        private readonly IEmailNotificationService _emailService;
        private readonly ILogger<CartController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public CartController(
            ICartService cartService,
            UserManager<User> userManager,
            IConfiguration configuration,
            IEmailNotificationService emailService,
            ILogger<CartController> logger,
            IHttpClientFactory httpClientFactory)
        {
            _cartService = cartService;
            _userManager = userManager;
            _configuration = configuration;
            _emailService = emailService;
            _logger = logger;
            _httpClientFactory = httpClientFactory;

            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int bookId, BookFormat format, bool isBorrow)
        {
            try
            {
                var userId = int.Parse(_userManager.GetUserId(User)!);

                // בדיקת מגבלות השאלה
                if (isBorrow)
                {
                    var currentBorrows = await _cartService.GetActiveUserBorrowsCountAsync(userId);
                    if (currentBorrows >= 3)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "הגעת למגבלת ההשאלות המקסימלית (3 ספרים)"
                        });
                    }

                    var availableCopies = await _cartService.GetAvailableCopiesCountAsync(bookId);
                    if (availableCopies <= 0)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "הספר אינו זמין כרגע להשאלה"
                        });
                    }
                }

                var result = await _cartService.AddToCartAsync(userId, bookId, isBorrow, format);
                if (!result)
                {
                    return Json(new
                    {
                        success = false,
                        message = "לא ניתן להוסיף את הספר לעגלה"
                    });
                }

                var cart = await _cartService.GetOrCreateCartAsync(userId);
                return Json(new
                {
                    success = true,
                    message = "הספר נוסף בהצלחה לעגלה",
                    cartItemsCount = cart.Items.Count,
                    cartTotal = cart.Total
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding book {bookId} to cart");
                return Json(new
                {
                    success = false,
                    message = "אירעה שגיאה בהוספת הספר לעגלה"
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(int bookId)
        {
            try
            {
                var userId = int.Parse(_userManager.GetUserId(User)!);
                var result = await _cartService.RemoveFromCartAsync(userId, bookId);

                if (!result)
                {
                    TempData["Error"] = "לא ניתן להסיר את הפריט מהעגלה";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing book {bookId} from cart");
                TempData["Error"] = "אירעה שגיאה בהסרת הפריט מהעגלה";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = int.Parse(_userManager.GetUserId(User)!);
                var cart = await _cartService.GetOrCreateCartAsync(userId);
                return View(cart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accessing cart");
                TempData["Error"] = "אירעה שגיאה בגישה לעגלה";
                return RedirectToAction("Index", "Home");
            }
        }


        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            try
            {
                var userId = int.Parse(_userManager.GetUserId(User)!);
                var cart = await _cartService.GetOrCreateCartAsync(userId);

                if (!cart.Items.Any())
                {
                    TempData["Error"] = "העגלה ריקה";
                    return RedirectToAction(nameof(Index));
                }

                // וולידציה של פריטים בעגלה
                if (await ValidateCartAsync(cart) is IActionResult validationResult)
                {
                    return validationResult;
                }

                var viewModel = new CheckoutViewModel
                {
                    Items = cart.Items,
                    Total = cart.Total,
                    CardDetails = new CreditCardDetails(
                        cardNumber: string.Empty,
                        expiryMonth: string.Empty,
                        expiryYear: string.Empty,
                        cvv: string.Empty,
                        cardHolderName: string.Empty
                    ),
                    PaymentMethod = Models.PaymentMethod.CreditCard,
                    AcceptTerms = false,
                    SaveCard = false
                };

                ViewBag.StripePublicKey = _configuration["Stripe:PublishableKey"];
                ViewBag.PayPalClientId = _configuration["PayPal:ClientId"];
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accessing checkout");
                TempData["Error"] = "אירעה שגיאה בגישה לדף התשלום";
                return RedirectToAction(nameof(Index));
            }
        }
        private async Task<IActionResult?> ValidateCartAsync(ShoppingCart cart)
        {
            try
            {
                var userId = int.Parse(_userManager.GetUserId(User)!);
                var borrowItems = cart.Items.Where(i => i.IsBorrow).ToList();

                // בדיקת מגבלת השאלות
                if (borrowItems.Any())
                {
                    var currentBorrows = await _cartService.GetActiveUserBorrowsCountAsync(userId);
                    if (currentBorrows + borrowItems.Count > 3)
                    {
                        TempData["Error"] = "לא ניתן לשאול יותר מ-3 ספרים";
                        return RedirectToAction(nameof(Index));
                    }
                }

                // בדיקת זמינות ספרים להשאלה
                foreach (var item in borrowItems)
                {
                    var availableCopies = await _cartService.GetAvailableCopiesCountAsync(item.BookId);
                    if (availableCopies <= 0)
                    {
                        TempData["Error"] = $"הספר {item.Title} אינו זמין כרגע להשאלה";
                        return RedirectToAction(nameof(Index));
                    }
                }

                // בדיקת מחירים
                foreach (var item in cart.Items)
                {
                    if (item.Price <= 0)
                    {
                        TempData["Error"] = $"מחיר לא תקין עבור הספר {item.Title}";
                        return RedirectToAction(nameof(Index));
                    }
                }

                // תקין - אין צורך בהחזרת תוצאה
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating cart");
                TempData["Error"] = "אירעה שגיאה בבדיקת העגלה";
                return RedirectToAction(nameof(Index));
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePayPalOrder()
        {
            try
            {
                var userId = int.Parse(_userManager.GetUserId(User)!);
                var cart = await _cartService.GetOrCreateCartAsync(userId);

                if (!cart.Items.Any())
                {
                    return BadRequest(new { error = "העגלה ריקה" });
                }

                decimal itemTotal = cart.Items.Sum(item => item.Price * item.Quantity);

                var orderRequest = new PayPalOrderRequest
                {
                    Intent = "CAPTURE",
                    PurchaseUnits =
                    [
                        new() {
                    ReferenceId = Guid.NewGuid().ToString(),
                    Amount = new PayPalAmount
                    {
                        CurrencyCode = "ILS",
                        Value = itemTotal.ToString("0.00"),
                        Breakdown = new PayPalAmountBreakdown
                        {
                            ItemTotal = new PayPalAmount
                            {
                                CurrencyCode = "ILS",
                                Value = itemTotal.ToString("0.00")
                            }
                        }
                    },
                    Items = cart.Items.Select(item => new PayPalItem
                    {
                        Name = item.Title.Length > 127 ? item.Title.Substring(0, 127) : item.Title,
                        UnitAmount = new PayPalAmount
                        {
                            CurrencyCode = "ILS",
                            Value = item.Price.ToString("0.00")
                        },
                        Quantity = item.Quantity.ToString(),
                        Category = "DIGITAL_GOODS",
                        Description = "ספר דיגיטלי"
                    }).ToList(),
                    Description = $"רכישה מספרייה דיגיטלית - {cart.Items.Count} פריטים"
                }
                    ],
                    ApplicationContext = new ApplicationContext
                    {
                        ReturnUrl = new Uri($"{Request.Scheme}://{Request.Host}/Cart/PayPalSuccess").ToString(),
                        CancelUrl = new Uri($"{Request.Scheme}://{Request.Host}/Cart/PayPalCancel").ToString(),
                        BrandName = "eBook Library",
                        Locale = "he-IL",
                        UserAction = "PAY_NOW",
                        ShippingPreference = "NO_SHIPPING",
                        LandingPage = "BILLING",  // שינוי מ-LOGIN ל-BILLING
                        PaymentMethod = new PayPalPaymentMethod  // הוספת הגדרות תשלום
                        {
                            PayeePreferred = "IMMEDIATE_PAYMENT_REQUIRED",
                            StandardEntryClass = "WEB",
                        },
                        ExperienceContext = new PayPalExperienceContext  // הוספת הקשר חווית משתמש
                        {
                            PaymentMethodPreference = "PAYPAL",
                            BrandName = "eBook Library",
                            Locale = "he-IL",
                            LandingPage = "BILLING",
                            UserAction = "PAY_NOW",
                            ReturnUrl = new Uri($"{Request.Scheme}://{Request.Host}/Cart/PayPalSuccess").ToString(),
                            CancelUrl = new Uri($"{Request.Scheme}://{Request.Host}/Cart/PayPalCancel").ToString()
                        }
                    }
                };

                // בדיקת התאמת סכומים
                decimal totalAmount = decimal.Parse(orderRequest.PurchaseUnits[0].Amount.Value);
                decimal itemsTotal = orderRequest.PurchaseUnits[0].Items?.Sum(
                    item => decimal.Parse(item.UnitAmount.Value) * decimal.Parse(item.Quantity)) ?? 0m;

                if (Math.Abs(totalAmount - itemsTotal) > 0.01m)
                {
                    _logger.LogError($"Amount mismatch: Total={totalAmount}, Items={itemsTotal}");
                    return BadRequest(new { error = "סכום הפריטים אינו תואם לסכום הכולל" });
                }

                using var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri("https://api-m.sandbox.paypal.com/");

                var accessToken = await GetPayPalAccessTokenAsync();
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await client.PostAsJsonAsync("/v2/checkout/orders", orderRequest);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"PayPal API error: Status={response.StatusCode}, Response={responseContent}");
                    var errorDetails = JsonSerializer.Deserialize<PayPalErrorResponse>(responseContent);
                    return BadRequest(new
                    {
                        error = "אירעה שגיאה ביצירת ההזמנה",
                        details = errorDetails?.Details?.FirstOrDefault()?.Description
                    });
                }

                var orderResponse = await response.Content.ReadFromJsonAsync<PayPalOrderResponse>();
                if (orderResponse?.Id == null)
                {
                    _logger.LogError("PayPal returned null order ID");
                    return BadRequest(new { error = "לא התקבל מזהה הזמנה" });
                }

                _logger.LogInformation($"PayPal order created successfully: {orderResponse.Id}");
                return Ok(new { id = orderResponse.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating PayPal order");
                return BadRequest(new { error = "אירעה שגיאה בלתי צפויה" });
            }
        }


        // מחלקות עזר חדשות
        public class PayPalPaymentMethod
        {
            [JsonPropertyName("payee_preferred")]
            public string PayeePreferred { get; set; } = string.Empty;

            [JsonPropertyName("standard_entry_class")]
            public string StandardEntryClass { get; set; } = string.Empty;
        }

        public class PayPalExperienceContext
        {
            [JsonPropertyName("payment_method_preference")]
            public string PaymentMethodPreference { get; set; } = string.Empty;

            [JsonPropertyName("brand_name")]
            public string BrandName { get; set; } = string.Empty;

            [JsonPropertyName("locale")]
            public string Locale { get; set; } = string.Empty;

            [JsonPropertyName("landing_page")]
            public string LandingPage { get; set; } = string.Empty;

            [JsonPropertyName("user_action")]
            public string UserAction { get; set; } = string.Empty;

            [JsonPropertyName("return_url")]
            public string ReturnUrl { get; set; } = string.Empty;

            [JsonPropertyName("cancel_url")]
            public string CancelUrl { get; set; } = string.Empty;
        }
        // מחלקת עזר לטיפול בשגיאות PayPal
        public class PayPalErrorResponse
        {
            public List<PayPalErrorDetail>? Details { get; set; }
            public string? Message { get; set; }
        }

        public class PayPalErrorDetail
        {
            public string? Issue { get; set; }
            public string? Description { get; set; }
        }
        public async Task<IActionResult> CapturePayPalOrder([FromBody] PayPalCaptureRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request?.OrderId))
                {
                    return BadRequest(new { error = "מזהה הזמנה לא תקין" });
                }

                var accessToken = await GetPayPalAccessTokenAsync();
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri("https://api-m.sandbox.paypal.com/");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await client.PostAsync($"/v2/checkout/orders/{request.OrderId}/capture", new StringContent(""));

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("PayPal capture failed with status: {Status}", response.StatusCode);
                    return BadRequest(new { error = "אירעה שגיאה בעיבוד התשלום" });
                }

                var captureResponse = await response.Content.ReadFromJsonAsync<PayPalVerificationResponse>();

                if (captureResponse?.IsCompleted == true)
                {
                    var userId = int.Parse(_userManager.GetUserId(User)!);
                    var paymentInfo = new PaymentInfo
                    {
                        UserId = userId.ToString(),
                        PayPalOrderId = request.OrderId,
                        Amount = await _cartService.GetCartTotalAsync(userId),
                        Method = Models.PaymentMethod.PayPal,
                        Currency = "ILS",
                        StripeToken = null
                    };

                    var result = await _cartService.ProcessCartAsync(userId, paymentInfo);
                    if (result.Success)
                    {
                        var user = await _userManager.GetUserAsync(User);
                        if (user?.Email != null)
                        {
                            var cart = await _cartService.GetOrCreateCartAsync(userId);
                            await _emailService.SendOrderConfirmationAsync(
                                user.Email,
                                cart.Items.ToList(),
                                cart.Total);
                        }

                        return Ok(new
                        {
                            success = true,
                            redirectUrl = Url.Action("Success", "Payment")
                        });
                    }
                }

                return BadRequest(new { error = "אירעה שגיאה בעיבוד התשלום" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error capturing PayPal payment");
                return BadRequest(new { error = "אירעה שגיאה בעיבוד התשלום" });
            }
        }

        private async Task<string> GetPayPalAccessTokenAsync()
        {
            var client = _httpClientFactory.CreateClient();
            var clientId = _configuration["PayPal:ClientId"];
            var clientSecret = _configuration["PayPal:ClientSecret"];
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            var response = await client.PostAsync(
                "https://api-m.sandbox.paypal.com/v1/oauth2/token",
                new FormUrlEncodedContent(new[]
                {
                new KeyValuePair<string, string>("grant_type", "client_credentials")
                })
            );

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Failed to get PayPal access token");
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<JsonElement>();
            return tokenResponse.GetProperty("access_token").GetString()!;
        }
    }

    public class PayPalCaptureRequest
    {
        public string? OrderId { get; internal set; }
    }
}
