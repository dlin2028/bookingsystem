using BookingSystem.Models;
using BookingSystem.Models.Seating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookingSystem.DataAccess.InMemory
{
    public class InMemoryEventRepository : IEventRepository
    {
        private readonly Dictionary<int, Event> _events;
        private int _nextId;

        public InMemoryEventRepository()
        {
            _events = new Dictionary<int, Event>();
            _nextId = 1;
            SeedData();
        }

        private void SeedData()
        {
            var events = new[]
            {
                new Event(
                    1,
                    "Rock Concert 2025",
                    "Amazing rock concert",
                    1,
                    new DateTime(2025, 12, 15, 20, 0, 0, DateTimeKind.Utc),
                    "Concert",
                    new FullReservedSeating(5000)),
                new Event(
                    2,
                    "Summer Music Festival",
                    "All-day music festival",
                    2,
                    new DateTime(2025, 8, 20, 12, 0, 0, DateTimeKind.Utc),
                    "Festival",
                    new OpenSeating()),
                new Event(
                    3,
                    "Jazz Night",
                    "Evening jazz performance",
                    3,
                    new DateTime(2025, 7, 10, 19, 0, 0, DateTimeKind.Utc),
                    "Concert",
                    new SectionReservedSeating(new Dictionary<string, int>
                    {
                        { "GoldenCircle", 100 },
                        { "Balcony", 200 }
                    }))
            };

            foreach (var evt in events)
            {
                _events[evt.Id] = evt;
                _nextId = Math.Max(_nextId, evt.Id + 1);
            }
        }

        public Task<Event> GetByIdAsync(int id)
        {
            _events.TryGetValue(id, out var evt);
            return Task.FromResult(evt);
        }

        public Task<IEnumerable<Event>> GetAllAsync()
        {
            return Task.FromResult(_events.Values.AsEnumerable());
        }

        public Task<IEnumerable<Event>> GetFutureEventsAsync()
        {
            var futureEvents = _events.Values
                .Where(e => e.IsFutureEvent())
                .OrderBy(e => e.EventDate);
            return Task.FromResult(futureEvents.AsEnumerable());
        }

        public Task<int> AddAsync(Event eventItem)
        {
            eventItem.Id = _nextId++;
            _events[eventItem.Id] = eventItem;
            return Task.FromResult(eventItem.Id);
        }

        public Task UpdateAsync(Event eventItem)
        {
            if (_events.ContainsKey(eventItem.Id))
            {
                _events[eventItem.Id] = eventItem;
            }
            return Task.CompletedTask;
        }

        public Task DeleteAsync(int id)
        {
            _events.Remove(id);
            return Task.CompletedTask;
        }
    }
}
