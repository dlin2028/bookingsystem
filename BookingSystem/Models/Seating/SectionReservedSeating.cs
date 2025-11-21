using System.Collections.Generic;
using System.Linq;

namespace BookingSystem.Models.Seating
{
    /// <summary>
    /// Represents events with reserved seats per section (e.g., Golden Circle, Balcony)
    /// Each section has its own capacity
    /// </summary>
    public class SectionReservedSeating : ISeatingType
    {
        public string SeatingTypeName => "Section Reserved Seating";

        public Dictionary<string, int> Sections { get; set; }

        public SectionReservedSeating(Dictionary<string, int> sections)
        {
            Sections = sections ?? new Dictionary<string, int>();
        }

        public int GetAvailableCapacity(int totalCapacity, int currentBookings)
        {
            return totalCapacity - currentBookings;
        }

        public bool CanAccommodateBooking(int requestedSeats, int availableCapacity, string sectionIdentifier = null)
        {
            if (string.IsNullOrEmpty(sectionIdentifier))
            {
                return false;
            }

            if (!Sections.ContainsKey(sectionIdentifier))
            {
                return false;
            }

            return requestedSeats <= availableCapacity;
        }

        public string GetSectionInfo()
        {
            var sectionDetails = string.Join(", ", Sections.Select(s => $"{s.Key}: {s.Value} seats"));
            return $"Sections: {sectionDetails}";
        }

        public int GetSectionCapacity(string sectionIdentifier)
        {
            return Sections.ContainsKey(sectionIdentifier) ? Sections[sectionIdentifier] : 0;
        }
    }
}
