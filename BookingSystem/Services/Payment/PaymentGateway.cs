using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BookingSystem.Services.Payment
{
    /// <summary>
    /// Implementation of payment gateway that communicates with external payment API
    /// POST /payments/ request: { creditCardNumber: string } response: { isValid: true|false, paymentId: string }
    /// </summary>
    public class PaymentGateway : IPaymentGateway
    {
        private readonly HttpClient _httpClient;
        private readonly string _paymentApiBaseUrl;

        public PaymentGateway(HttpClient httpClient, string paymentApiBaseUrl)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _paymentApiBaseUrl = paymentApiBaseUrl ?? throw new ArgumentNullException(nameof(paymentApiBaseUrl));
        }

        public async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrWhiteSpace(request.CreditCardNumber))
                throw new ArgumentException("Credit card number is required", nameof(request));

            try
            {
                var jsonContent = JsonSerializer.Serialize(request);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_paymentApiBaseUrl}/payments/", httpContent);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var paymentResponse = JsonSerializer.Deserialize<PaymentResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return paymentResponse ?? new PaymentResponse(false, null);
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException("Failed to communicate with payment gateway", ex);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("Failed to parse payment gateway response", ex);
            }
        }
    }
}
