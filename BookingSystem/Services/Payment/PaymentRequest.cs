namespace BookingSystem.Services.Payment
{
    public class PaymentRequest
    {
        public string CreditCardNumber { get; set; }

        public PaymentRequest(string creditCardNumber)
        {
            CreditCardNumber = creditCardNumber;
        }
    }
}
