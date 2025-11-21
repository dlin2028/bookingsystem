using System.Threading.Tasks;

namespace BookingSystem.Services.Payment
{
    /// <summary>
    /// Payment gateway interface for processing payments
    /// </summary>
    public interface IPaymentGateway
    {
        /// <summary>
        /// Processes a payment through the external payment API
        /// </summary>
        /// <param name="request">Payment request containing credit card information</param>
        /// <returns>Payment response indicating if payment was successful</returns>
        Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request);
    }
}
