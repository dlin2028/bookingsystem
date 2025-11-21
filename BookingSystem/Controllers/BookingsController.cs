using BookingSystem.DataAccess;
using BookingSystem.Models;
using BookingSystem.Services.Booking;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly IBookingRepository _bookingRepository;

        public BookingsController(IBookingService bookingService, IBookingRepository bookingRepository)
        {
            _bookingService = bookingService ?? throw new ArgumentNullException(nameof(bookingService));
            _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
        }

        /// <summary>
        /// Gets all bookings
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Booking>>> GetAllBookings()
        {
            var bookings = await _bookingRepository.GetAllAsync();
            return Ok(bookings);
        }

        /// <summary>
        /// Gets a booking by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Booking>> GetBooking(int id)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null)
            {
                return NotFound(new { message = $"Booking with ID {id} not found" });
            }
            return Ok(booking);
        }

        /// <summary>
        /// Gets bookings by user ID
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Booking>>> GetBookingsByUser(int userId)
        {
            var bookings = await _bookingRepository.GetByUserIdAsync(userId);
            return Ok(bookings);
        }

        /// <summary>
        /// Gets bookings by venue ID
        /// </summary>
        [HttpGet("venue/{venueId}")]
        public async Task<ActionResult<IEnumerable<Booking>>> GetBookingsByVenue(int venueId)
        {
            var bookings = await _bookingRepository.GetByVenueIdAsync(venueId);
            return Ok(bookings);
        }

        /// <summary>
        /// ADVANCED QUERY 1: Gets bookings for paid users at venue
        /// </summary>
        [HttpGet("venue/{venueId}/paid-users")]
        public async Task<ActionResult<IEnumerable<Booking>>> GetBookingsForPaidUsersAtVenue(int venueId)
        {
            var bookings = await _bookingRepository.FindBookingsForPaidUsersAtVenueAsync(venueId);
            return Ok(bookings);
        }

        /// <summary>
        /// ADVANCED QUERY 2: Gets users without bookings in venue
        /// </summary>
        [HttpGet("venue/{venueId}/users-without-bookings")]
        public async Task<ActionResult<IEnumerable<int>>> GetUsersWithoutBookingsInVenue(int venueId)
        {
            var userIds = await _bookingRepository.FindUsersWithoutBookingsInVenueAsync(venueId);
            return Ok(userIds);
        }

        /// <summary>
        /// Creates a new booking with payment validation
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<BookingResult>> CreateBooking([FromBody] CreateBookingRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Invalid booking request" });
            }

            var result = await _bookingService.CreateBookingAsync(request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetBooking), new { id = result.BookingId }, result);
        }

        /// <summary>
        /// Updates a booking
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateBooking(int id, [FromBody] Booking booking)
        {
            if (id != booking.Id)
            {
                return BadRequest(new { message = "Booking ID mismatch" });
            }

            var existingBooking = await _bookingRepository.GetByIdAsync(id);
            if (existingBooking == null)
            {
                return NotFound(new { message = $"Booking with ID {id} not found" });
            }

            await _bookingRepository.UpdateAsync(booking);
            return NoContent();
        }

        /// <summary>
        /// Deletes a booking
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteBooking(int id)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null)
            {
                return NotFound(new { message = $"Booking with ID {id} not found" });
            }

            await _bookingRepository.DeleteAsync(id);
            return NoContent();
        }
    }
}
