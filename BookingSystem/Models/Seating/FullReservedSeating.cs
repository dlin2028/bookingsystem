namespace BookingSystem.Models.Seating
{
    /// <summary>
    /// Represents events with fully reserved seats (e.g., full seated concerts)
    /// Each booking has a specific seat number
    /// </summary>
    public class FullReservedSeating : ISeatingType
    {
        public string SeatingTypeName => "Full Reserved Seating";

        public int TotalSeats { get; set; }

        public FullReservedSeating(int totalSeats)
        {
            TotalSeats = totalSeats;
        }

        public int GetAvailableCapacity(int totalCapacity, int currentBookings)
        {
            return totalCapacity - currentBookings;
        }

        public bool CanAccommodateBooking(int requestedSeats, int availableCapacity, string sectionIdentifier = null)
        {
            return requestedSeats <= availableCapacity;
        }

        public string GetSectionInfo()
        {
            return $"Total Seats: {TotalSeats}";
        }
    }
}
