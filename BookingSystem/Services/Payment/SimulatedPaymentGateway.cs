using System;
using System.Threading.Tasks;

namespace BookingSystem.Services.Payment
{
    /// <summary>
    /// Simulated payment gateway for testing and development
    /// Validates payments based on simple rules (e.g., card number length)
    /// </summary>
    public class SimulatedPaymentGateway : IPaymentGateway
    {
        public Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrWhiteSpace(request.CreditCardNumber))
                throw new ArgumentException("Credit card number is required", nameof(request));

            bool isValid = ValidateCreditCard(request.CreditCardNumber);
            string paymentId = isValid ? Guid.NewGuid().ToString() : null;

            var response = new PaymentResponse(isValid, paymentId);
            return Task.FromResult(response);
        }

        private bool ValidateCreditCard(string creditCardNumber)
        {
            var cleanedNumber = creditCardNumber.Replace(" ", "").Replace("-", "");

            if (cleanedNumber.Length < 13 || cleanedNumber.Length > 19)
                return false;

            if (!long.TryParse(cleanedNumber, out _))
                return false;

            if (cleanedNumber.StartsWith("0000"))
                return false;

            return true;
        }
    }
}
