using BookingSystem.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookingSystem.DataAccess
{
    public interface IBookingRepository
    {
        Task<Booking> GetByIdAsync(int id);
        Task<IEnumerable<Booking>> GetAllAsync();
        Task<IEnumerable<Booking>> GetByUserIdAsync(int userId);
        Task<IEnumerable<Booking>> GetByVenueIdAsync(int venueId);
        Task<IEnumerable<Booking>> GetByEventIdAsync(int eventId);
        Task<int> AddAsync(Booking booking);
        Task UpdateAsync(Booking booking);
        Task DeleteAsync(int id);
        Task<int> GetBookingCountForEventAsync(int eventId);
        Task<int> GetBookingCountForEventSectionAsync(int eventId, string sectionIdentifier);
        Task<IEnumerable<Booking>> FindBookingsForPaidUsersAtVenueAsync(int venueId);
        Task<IEnumerable<int>> FindUsersWithoutBookingsInVenueAsync(int venueId);
    }
}
