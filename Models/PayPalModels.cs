// Models/PayPalModels.cs
using System.Text.Json.Serialization;

namespace MyEBookLibrary.Models
{
    public class PayPalOrderResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("links")]
        public List<PayPalLink>? Links { get; set; }
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
    }

}