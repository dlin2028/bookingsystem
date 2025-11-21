using BookingSystem.Models.Seating;

namespace BookingSystem.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int VenueId { get; set; }
        public DateTime EventDate { get; set; }
        public string EventType { get; set; }
        public ISeatingType SeatingType { get; set; }
        public DateTime CreatedAt { get; set; }

        public Event()
        {
            CreatedAt = DateTime.UtcNow;
        }

        public Event(int id, string name, string description, int venueId, DateTime eventDate, string eventType, ISeatingType seatingType)
        {
            Id = id;
            Name = name;
            Description = description;
            VenueId = venueId;
            EventDate = eventDate;
            EventType = eventType;
            SeatingType = seatingType;
            CreatedAt = DateTime.UtcNow;
        }

        public bool IsFutureEvent()
        {
            return EventDate > DateTime.UtcNow;
        }
    }
}
