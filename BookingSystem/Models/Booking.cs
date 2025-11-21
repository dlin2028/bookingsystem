namespace BookingSystem.Models
{
    public enum PaymentStatus
    {
        Pending,
        Paid,
        Failed,
        Refunded
    }

    public class Booking
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int EventId { get; set; }
        public int VenueId { get; set; }
        public int NumberOfSeats { get; set; }
        public string SectionIdentifier { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public string PaymentId { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime BookingDate { get; set; }
        public DateTime CreatedAt { get; set; }

        public Booking()
        {
            CreatedAt = DateTime.UtcNow;
            BookingDate = DateTime.UtcNow;
            PaymentStatus = PaymentStatus.Pending;
        }

        public Booking(int id, int userId, int eventId, int venueId, int numberOfSeats, string sectionIdentifier, decimal totalAmount)
        {
            Id = id;
            UserId = userId;
            EventId = eventId;
            VenueId = venueId;
            NumberOfSeats = numberOfSeats;
            SectionIdentifier = sectionIdentifier;
            TotalAmount = totalAmount;
            PaymentStatus = PaymentStatus.Pending;
            BookingDate = DateTime.UtcNow;
            CreatedAt = DateTime.UtcNow;
        }

        public bool IsPaid()
        {
            return PaymentStatus == PaymentStatus.Paid;
        }

        public void MarkAsPaid(string paymentId)
        {
            PaymentStatus = PaymentStatus.Paid;
            PaymentId = paymentId;
        }

        public void MarkAsFailed()
        {
            PaymentStatus = PaymentStatus.Failed;
        }
    }
}
