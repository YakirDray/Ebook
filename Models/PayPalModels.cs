// Models/PayPalModels.cs
using System.Text.Json.Serialization;
using MyEBookLibrary.Controllers;

namespace MyEBookLibrary.Models
{
    public class PayPalOrderRequest
    {
        [JsonPropertyName("intent")]
        public string Intent { get; set; } = "CAPTURE";

        [JsonPropertyName("purchase_units")]
        public List<PurchaseUnit> PurchaseUnits { get; set; } = [];

        [JsonPropertyName("application_context")]
        public ApplicationContext ApplicationContext { get; set; } = new();
    }

    public class PurchaseUnit
    {
        [JsonPropertyName("reference_id")]
        public string ReferenceId { get; set; } = Guid.NewGuid().ToString();

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("amount")]
        public required PayPalAmount Amount { get; set; }

        [JsonPropertyName("items")]
        public List<PayPalItem>? Items { get; set; }
    }

    public class PayPalAmount
    {
        [JsonPropertyName("currency_code")]
        public string CurrencyCode { get; set; } = "ILS";

        [JsonPropertyName("value")]
        public string Value { get; set; } = "0.00";

        [JsonPropertyName("breakdown")]
        public PayPalAmountBreakdown? Breakdown { get; set; }
    }

    public class PayPalAmountBreakdown
    {
        [JsonPropertyName("item_total")]
        public required PayPalAmount ItemTotal { get; set; }

        [JsonPropertyName("tax_total")]
        public PayPalAmount? TaxTotal { get; set; }
    }

    public class PayPalItem
    {
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("quantity")]
        public string Quantity { get; set; } = "1";

        [JsonPropertyName("unit_amount")]
        public required PayPalAmount UnitAmount { get; set; }

        [JsonPropertyName("category")]
        public string Category { get; set; } = "DIGITAL_GOODS";
    }

    public class ApplicationContext
    {
        [JsonPropertyName("return_url")]
        public string ReturnUrl { get; set; } = "https://example.com/success";

        [JsonPropertyName("cancel_url")]
        public string CancelUrl { get; set; } = "https://example.com/cancel";

        [JsonPropertyName("brand_name")]
        public string BrandName { get; set; } = "eBook Library";

        [JsonPropertyName("locale")]
        public string Locale { get; set; } = "he-IL";

        [JsonPropertyName("landing_page")]
        public string LandingPage { get; set; } = "LOGIN";

        [JsonPropertyName("user_action")]
        public string UserAction { get; set; } = "PAY_NOW";

        [JsonPropertyName("shipping_preference")]
        public string ShippingPreference { get; set; } = "NO_SHIPPING";
        public CartController.PayPalPaymentMethod? PaymentMethod { get; internal set; }
        public CartController.PayPalExperienceContext? ExperienceContext { get; internal set; }
    }

    public class PayPalOrderResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("links")]
        public List<PayPalLink>? Links { get; set; }

        // מציאת הקישור הרלוונטי לפי סוג
        public string? GetLinkByRel(string rel)
        {
            return Links?.FirstOrDefault(l => l.Rel == rel)?.Href;
        }

        // קישור לאישור התשלום
        public string? ApprovalUrl => GetLinkByRel("approve");

        // קישור ללכידת התשלום
        public string? CaptureUrl => GetLinkByRel("capture");
    }

    public class PayPalLink
    {
        [JsonPropertyName("href")]
        public string? Href { get; set; }

        [JsonPropertyName("rel")]
        public string? Rel { get; set; }

        [JsonPropertyName("method")]
        public string? Method { get; set; }
    }

    public class PayPalVerificationResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("payer")]
        public PayPalPayer? Payer { get; set; }

        [JsonPropertyName("purchase_units")]
        public List<PayPalPurchaseUnitResponse>? PurchaseUnits { get; set; }

        public bool IsCompleted => Status?.ToUpper() == "COMPLETED";
    }

    public class PayPalPayer
    {
        [JsonPropertyName("email_address")]
        public string? EmailAddress { get; set; }

        [JsonPropertyName("payer_id")]
        public string? PayerId { get; set; }
    }

    public class PayPalPurchaseUnitResponse
    {
        [JsonPropertyName("reference_id")]
        public string? ReferenceId { get; set; }

        [JsonPropertyName("payments")]
        public PayPalPayments? Payments { get; set; }
    }

    public class PayPalPayments
    {
        [JsonPropertyName("captures")]
        public List<PayPalCapture>? Captures { get; set; }
    }

    public class PayPalCapture
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("amount")]
        public PayPalAmount? Amount { get; set; }
    }

    // מצבי תשלום PayPal
    public class PayPalOrderStatus
    {
        public const string CREATED = "CREATED";
        public const string SAVED = "SAVED";
        public const string APPROVED = "APPROVED";
        public const string VOIDED = "VOIDED";
        public const string COMPLETED = "COMPLETED";
        public const string PAYER_ACTION_REQUIRED = "PAYER_ACTION_REQUIRED";
    }

    // שגיאות PayPal
    public class PayPalError
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("debug_id")]
        public string? DebugId { get; set; }

        [JsonPropertyName("details")]
        public List<PayPalErrorDetail>? Details { get; set; }
    }

    public class PayPalErrorDetail
    {
        [JsonPropertyName("field")]
        public string? Field { get; set; }

        [JsonPropertyName("value")]
        public string? Value { get; set; }

        [JsonPropertyName("location")]
        public string? Location { get; set; }

        [JsonPropertyName("issue")]
        public string? Issue { get; set; }
    }
}