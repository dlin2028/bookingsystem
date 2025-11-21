using BookingSystem.DataAccess;
using BookingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly IEventRepository _eventRepository;
        private readonly IVenueRepository _venueRepository;
        private readonly IBookingRepository _bookingRepository;

        public EventsController(
            IEventRepository eventRepository,
            IVenueRepository venueRepository,
            IBookingRepository bookingRepository)
        {
            _eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
            _venueRepository = venueRepository ?? throw new ArgumentNullException(nameof(venueRepository));
            _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
        }

        /// <summary>
        /// Gets all events
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Event>>> GetAllEvents()
        {
            var events = await _eventRepository.GetAllAsync();
            return Ok(events);
        }

        /// <summary>
        /// Gets an event by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Event>> GetEvent(int id)
        {
            var eventItem = await _eventRepository.GetByIdAsync(id);
            if (eventItem == null)
            {
                return NotFound(new { message = $"Event with ID {id} not found" });
            }
            return Ok(eventItem);
        }

        /// <summary>
        /// CATALOG RETRIEVAL: Gets future events with available seat information
        /// </summary>
        [HttpGet("future-with-availability")]
        public async Task<ActionResult<IEnumerable<object>>> GetFutureEventsWithAvailability()
        {
            var futureEvents = await _eventRepository.GetFutureEventsAsync();
            var eventsWithAvailability = new List<object>();

            foreach (var evt in futureEvents)
            {
                var venue = await _venueRepository.GetByIdAsync(evt.VenueId);
                if (venue == null)
                    continue;

                var bookedSeats = await _bookingRepository.GetBookingCountForEventAsync(evt.Id);
                var availableSeats = venue.TotalCapacity - bookedSeats;

                eventsWithAvailability.Add(new
                {
                    EventId = evt.Id,
                    EventName = evt.Name,
                    Description = evt.Description,
                    EventDate = evt.EventDate,
                    EventType = evt.EventType,
                    SeatingTypeName = evt.SeatingType?.SeatingTypeName ?? "Unknown",
                    SeatingInfo = evt.SeatingType?.GetSectionInfo() ?? "No seating info",
                    VenueId = venue.Id,
                    VenueName = venue.Name,
                    VenueLocation = venue.Location,
                    TotalCapacity = venue.TotalCapacity,
                    BookedSeats = bookedSeats,
                    AvailableSeats = availableSeats,
                    IsAvailable = availableSeats > 0
                });
            }

            return Ok(eventsWithAvailability);
        }

        /// <summary>
        /// Gets future events
        /// </summary>
        [HttpGet("future")]
        public async Task<ActionResult<IEnumerable<Event>>> GetFutureEvents()
        {
            var events = await _eventRepository.GetFutureEventsAsync();
            return Ok(events);
        }

        /// <summary>
        /// Creates a new event
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Event>> CreateEvent([FromBody] Event eventItem)
        {
            if (eventItem == null)
            {
                return BadRequest(new { message = "Invalid event data" });
            }

            var venue = await _venueRepository.GetByIdAsync(eventItem.VenueId);
            if (venue == null)
            {
                return BadRequest(new { message = $"Venue with ID {eventItem.VenueId} not found" });
            }

            var eventId = await _eventRepository.AddAsync(eventItem);
            eventItem.Id = eventId;

            return CreatedAtAction(nameof(GetEvent), new { id = eventId }, eventItem);
        }

        /// <summary>
        /// Updates an event
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateEvent(int id, [FromBody] Event eventItem)
        {
            if (id != eventItem.Id)
            {
                return BadRequest(new { message = "Event ID mismatch" });
            }

            var existingEvent = await _eventRepository.GetByIdAsync(id);
            if (existingEvent == null)
            {
                return NotFound(new { message = $"Event with ID {id} not found" });
            }

            await _eventRepository.UpdateAsync(eventItem);
            return NoContent();
        }

        /// <summary>
        /// Deletes an event
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteEvent(int id)
        {
            var eventItem = await _eventRepository.GetByIdAsync(id);
            if (eventItem == null)
            {
                return NotFound(new { message = $"Event with ID {id} not found" });
            }

            await _eventRepository.DeleteAsync(id);
            return NoContent();
        }
    }
}
