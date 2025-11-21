namespace BookingSystem.Models.Seating
{
    /// <summary>
    /// Abstraction for different event seating types
    /// </summary>
    public interface ISeatingType
    {
        /// <summary>
        /// Gets the type of seating arrangement
        /// </summary>
        string SeatingTypeName { get; }

        /// <summary>
        /// Calculates available capacity for this seating type
        /// </summary>
        int GetAvailableCapacity(int totalCapacity, int currentBookings);

        /// <summary>
        /// Validates if a booking can be made with specific seating requirements
        /// </summary>
        bool CanAccommodateBooking(int requestedSeats, int availableCapacity, string sectionIdentifier = null);

        /// <summary>
        /// Gets section information if applicable
        /// </summary>
        string GetSectionInfo();
    }
}
