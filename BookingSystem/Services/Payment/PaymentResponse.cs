namespace BookingSystem.Services.Payment
{
    public class PaymentResponse
    {
        public bool IsValid { get; set; }
        public string PaymentId { get; set; }

        public PaymentResponse(bool isValid, string paymentId)
        {
            IsValid = isValid;
            PaymentId = paymentId;
        }
    }
}
