namespace BookingSystem.Models
{
    public class Venue
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public int TotalCapacity { get; set; }
        public DateTime CreatedAt { get; set; }

        public Venue()
        {
            CreatedAt = DateTime.UtcNow;
        }

        public Venue(int id, string name, string location, int totalCapacity)
        {
            Id = id;
            Name = name;
            Location = location;
            TotalCapacity = totalCapacity;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
