using System.Threading.Tasks;

namespace BookingSystem.Services.Booking
{
    public interface IBookingService
    {
        /// <summary>
        /// Creates a new booking with payment validation and capacity checks
        /// </summary>
        Task<BookingResult> CreateBookingAsync(CreateBookingRequest request);
    }
}
