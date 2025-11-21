namespace BookingSystem.Models.Seating
{
    /// <summary>
    /// Represents events without any reservation type (e.g., open air festivals)
    /// No specific seat assignments, only capacity limits
    /// </summary>
    public class OpenSeating : ISeatingType
    {
        public string SeatingTypeName => "Open Seating";

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
            return "Open seating - no specific sections";
        }
    }
}
