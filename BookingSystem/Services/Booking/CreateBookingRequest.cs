namespace BookingSystem.Services.Booking
{
    public class CreateBookingRequest
    {
        public int UserId { get; set; }
        public int EventId { get; set; }
        public int NumberOfSeats { get; set; }
        public string? SectionIdentifier { get; set; }
        public decimal TotalAmount { get; set; }
        public string CreditCardNumber { get; set; }

        public CreateBookingRequest(int userId, int eventId, int numberOfSeats, string creditCardNumber, decimal totalAmount, string? sectionIdentifier = null)
        {
            UserId = userId;
            EventId = eventId;
            NumberOfSeats = numberOfSeats;
            CreditCardNumber = creditCardNumber;
            TotalAmount = totalAmount;
            SectionIdentifier = sectionIdentifier;
        }
    }
}
