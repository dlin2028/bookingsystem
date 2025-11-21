using BookingSystem.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookingSystem.DataAccess.InMemory
{
    public class InMemoryVenueRepository : IVenueRepository
    {
        private readonly Dictionary<int, Venue> _venues;
        private int _nextId;

        public InMemoryVenueRepository()
        {
            _venues = new Dictionary<int, Venue>();
            _nextId = 1;
            SeedData();
        }

        private void SeedData()
        {
            var venues = new[]
            {
                new Venue(1, "Grand Concert Hall", "123 Music Street, New York", 5000),
                new Venue(2, "Open Air Festival Grounds", "456 Park Avenue, Los Angeles", 20000),
                new Venue(3, "Jazz Club Downtown", "789 Blues Road, Chicago", 300)
            };

            foreach (var venue in venues)
            {
                _venues[venue.Id] = venue;
                _nextId = Math.Max(_nextId, venue.Id + 1);
            }
        }

        public Task<Venue> GetByIdAsync(int id)
        {
            _venues.TryGetValue(id, out var venue);
            return Task.FromResult(venue);
        }

        public Task<IEnumerable<Venue>> GetAllAsync()
        {
            return Task.FromResult(_venues.Values.AsEnumerable());
        }

        public Task<int> AddAsync(Venue venue)
        {
            venue.Id = _nextId++;
            _venues[venue.Id] = venue;
            return Task.FromResult(venue.Id);
        }

        public Task UpdateAsync(Venue venue)
        {
            if (_venues.ContainsKey(venue.Id))
            {
                _venues[venue.Id] = venue;
            }
            return Task.CompletedTask;
        }

        public Task DeleteAsync(int id)
        {
            _venues.Remove(id);
            return Task.CompletedTask;
        }
    }
}
