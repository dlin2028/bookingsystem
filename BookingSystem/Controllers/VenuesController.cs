using BookingSystem.DataAccess;
using BookingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VenuesController : ControllerBase
    {
        private readonly IVenueRepository _venueRepository;

        public VenuesController(IVenueRepository venueRepository)
        {
            _venueRepository = venueRepository ?? throw new ArgumentNullException(nameof(venueRepository));
        }

        /// <summary>
        /// Gets all venues
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Venue>>> GetAllVenues()
        {
            var venues = await _venueRepository.GetAllAsync();
            return Ok(venues);
        }

        /// <summary>
        /// Gets a venue by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Venue>> GetVenue(int id)
        {
            var venue = await _venueRepository.GetByIdAsync(id);
            if (venue == null)
            {
                return NotFound(new { message = $"Venue with ID {id} not found" });
            }
            return Ok(venue);
        }

        /// <summary>
        /// Creates a new venue
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Venue>> CreateVenue([FromBody] Venue venue)
        {
            if (venue == null)
            {
                return BadRequest(new { message = "Invalid venue data" });
            }

            if (venue.TotalCapacity <= 0)
            {
                return BadRequest(new { message = "Venue capacity must be greater than zero" });
            }

            var venueId = await _venueRepository.AddAsync(venue);
            venue.Id = venueId;

            return CreatedAtAction(nameof(GetVenue), new { id = venueId }, venue);
        }

        /// <summary>
        /// Updates a venue
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateVenue(int id, [FromBody] Venue venue)
        {
            if (id != venue.Id)
            {
                return BadRequest(new { message = "Venue ID mismatch" });
            }

            var existingVenue = await _venueRepository.GetByIdAsync(id);
            if (existingVenue == null)
            {
                return NotFound(new { message = $"Venue with ID {id} not found" });
            }

            await _venueRepository.UpdateAsync(venue);
            return NoContent();
        }

        /// <summary>
        /// Deletes a venue
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteVenue(int id)
        {
            var venue = await _venueRepository.GetByIdAsync(id);
            if (venue == null)
            {
                return NotFound(new { message = $"Venue with ID {id} not found" });
            }

            await _venueRepository.DeleteAsync(id);
            return NoContent();
        }
    }
}
