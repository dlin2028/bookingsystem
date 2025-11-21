using BookingSystem.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookingSystem.DataAccess.InMemory
{
    public class InMemoryBookingRepository : IBookingRepository
    {
        private readonly Dictionary<int, Booking> _bookings;
        private int _nextId;

        public InMemoryBookingRepository()
        {
            _bookings = new Dictionary<int, Booking>();
            _nextId = 1;
            SeedData();
        }

        private void SeedData()
        {
            var bookings = new[]
            {
                new Booking(1, 1, 1, 1, 2, null, 150.00m) { PaymentStatus = PaymentStatus.Paid, PaymentId = "PAY-123456" },
                new Booking(2, 2, 1, 1, 4, null, 300.00m) { PaymentStatus = PaymentStatus.Paid, PaymentId = "PAY-123457" },
                new Booking(3, 3, 2, 2, 5, null, 250.00m) { PaymentStatus = PaymentStatus.Paid, PaymentId = "PAY-123458" },
                new Booking(4, 1, 3, 3, 2, "GoldenCircle", 200.00m) { PaymentStatus = PaymentStatus.Paid, PaymentId = "PAY-123459" },
                new Booking(5, 4, 3, 3, 3, "Balcony", 120.00m) { PaymentStatus = PaymentStatus.Pending }
            };

            foreach (var booking in bookings)
            {
                _bookings[booking.Id] = booking;
                _nextId = Math.Max(_nextId, booking.Id + 1);
            }
        }

        public Task<Booking> GetByIdAsync(int id)
        {
            _bookings.TryGetValue(id, out var booking);
            return Task.FromResult(booking);
        }

        public Task<IEnumerable<Booking>> GetAllAsync()
        {
            return Task.FromResult(_bookings.Values.AsEnumerable());
        }

        public Task<IEnumerable<Booking>> GetByUserIdAsync(int userId)
        {
            var bookings = _bookings.Values.Where(b => b.UserId == userId);
            return Task.FromResult(bookings.AsEnumerable());
        }

        public Task<IEnumerable<Booking>> GetByVenueIdAsync(int venueId)
        {
            var bookings = _bookings.Values.Where(b => b.VenueId == venueId);
            return Task.FromResult(bookings.AsEnumerable());
        }

        public Task<IEnumerable<Booking>> GetByEventIdAsync(int eventId)
        {
            var bookings = _bookings.Values.Where(b => b.EventId == eventId);
            return Task.FromResult(bookings.AsEnumerable());
        }

        public Task<int> AddAsync(Booking booking)
        {
            booking.Id = _nextId++;
            _bookings[booking.Id] = booking;
            return Task.FromResult(booking.Id);
        }

        public Task UpdateAsync(Booking booking)
        {
            if (_bookings.ContainsKey(booking.Id))
            {
                _bookings[booking.Id] = booking;
            }
            return Task.CompletedTask;
        }

        public Task DeleteAsync(int id)
        {
            _bookings.Remove(id);
            return Task.CompletedTask;
        }

        public Task<int> GetBookingCountForEventAsync(int eventId)
        {
            var count = _bookings.Values
                .Where(b => b.EventId == eventId)
                .Sum(b => b.NumberOfSeats);
            return Task.FromResult(count);
        }

        public Task<int> GetBookingCountForEventSectionAsync(int eventId, string sectionIdentifier)
        {
            var count = _bookings.Values
                .Where(b => b.EventId == eventId && b.SectionIdentifier == sectionIdentifier)
                .Sum(b => b.NumberOfSeats);
            return Task.FromResult(count);
        }

        /// <summary>
        /// Retrieves only bookings for users who have at least one successfully paid booking at the specified venue.
        /// Simulates: SELECT b.* FROM Bookings b WHERE b.VenueId = @venueId
        ///            AND b.UserId IN (SELECT DISTINCT UserId FROM Bookings WHERE VenueId = @venueId AND PaymentStatus = 'Paid')
        /// </summary>
        public Task<IEnumerable<Booking>> FindBookingsForPaidUsersAtVenueAsync(int venueId)
        {
            var usersWithPaidBookings = _bookings.Values
                .Where(b => b.VenueId == venueId && b.PaymentStatus == PaymentStatus.Paid)
                .Select(b => b.UserId)
                .Distinct()
                .ToHashSet();

            var result = _bookings.Values
                .Where(b => b.VenueId == venueId && usersWithPaidBookings.Contains(b.UserId));

            return Task.FromResult(result.AsEnumerable());
        }

        /// <summary>
        /// Retrieves a list of all User IDs who have no bookings whatsoever at the specified venue.
        /// This requires access to all users, so it returns user IDs that should be cross-referenced with UserRepository.
        /// Simulates: SELECT u.Id FROM Users u WHERE NOT EXISTS
        ///            (SELECT 1 FROM Bookings b WHERE b.UserId = u.Id AND b.VenueId = @venueId)
        /// </summary>
        public Task<IEnumerable<int>> FindUsersWithoutBookingsInVenueAsync(int venueId)
        {
            var usersWithBookingsAtVenue = _bookings.Values
                .Where(b => b.VenueId == venueId)
                .Select(b => b.UserId)
                .Distinct()
                .ToHashSet();

            var allUserIds = _bookings.Values
                .Select(b => b.UserId)
                .Distinct()
                .Where(userId => !usersWithBookingsAtVenue.Contains(userId));

            return Task.FromResult(allUserIds);
        }
    }
}
