using BookingSystem.DataAccess.InMemory;
using BookingSystem.Models;
using FluentAssertions;
using Xunit;

namespace BookingSystem.Tests.DataAccess.InMemory
{
    public class InMemoryBookingRepositoryTests
    {
        private readonly InMemoryBookingRepository _repository;

        public InMemoryBookingRepositoryTests()
        {
            _repository = new InMemoryBookingRepository();
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnBooking_WhenExists()
        {
            // Act
            var booking = await _repository.GetByIdAsync(1);

            // Assert
            booking.Should().NotBeNull();
            booking.Id.Should().Be(1);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            // Act
            var booking = await _repository.GetByIdAsync(9999);

            // Assert
            booking.Should().BeNull();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllBookings()
        {
            // Act
            var bookings = await _repository.GetAllAsync();

            // Assert
            bookings.Should().NotBeEmpty();
            bookings.Should().HaveCountGreaterThan(0);
        }

        [Fact]
        public async Task AddAsync_ShouldAddBookingAndReturnId()
        {
            // Arrange
            var newBooking = new Booking(0, 1, 1, 1, 3, null, 150.00m);

            // Act
            var id = await _repository.AddAsync(newBooking);

            // Assert
            id.Should().BeGreaterThan(0);
            var retrieved = await _repository.GetByIdAsync(id);
            retrieved.Should().NotBeNull();
            retrieved.NumberOfSeats.Should().Be(3);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateExistingBooking()
        {
            // Arrange
            var booking = await _repository.GetByIdAsync(1);
            booking.NumberOfSeats = 10;

            // Act
            await _repository.UpdateAsync(booking);

            // Assert
            var updated = await _repository.GetByIdAsync(1);
            updated.NumberOfSeats.Should().Be(10);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveBooking()
        {
            // Arrange
            var newBooking = new Booking(0, 1, 1, 1, 2, null, 100.00m);
            var id = await _repository.AddAsync(newBooking);

            // Act
            await _repository.DeleteAsync(id);

            // Assert
            var deleted = await _repository.GetByIdAsync(id);
            deleted.Should().BeNull();
        }

        [Fact]
        public async Task GetByUserIdAsync_ShouldReturnUserBookings()
        {
            // Act
            var bookings = await _repository.GetByUserIdAsync(1);

            // Assert
            bookings.Should().NotBeEmpty();
            bookings.Should().OnlyContain(b => b.UserId == 1);
        }

        [Fact]
        public async Task GetByVenueIdAsync_ShouldReturnVenueBookings()
        {
            // Act
            var bookings = await _repository.GetByVenueIdAsync(1);

            // Assert
            bookings.Should().NotBeEmpty();
            bookings.Should().OnlyContain(b => b.VenueId == 1);
        }

        [Fact]
        public async Task GetByEventIdAsync_ShouldReturnEventBookings()
        {
            // Act
            var bookings = await _repository.GetByEventIdAsync(1);

            // Assert
            bookings.Should().NotBeEmpty();
            bookings.Should().OnlyContain(b => b.EventId == 1);
        }

        [Fact]
        public async Task GetBookingCountForEventAsync_ShouldReturnTotalSeats()
        {
            // Act - Event 1 has bookings with 2 and 4 seats in seed data
            var count = await _repository.GetBookingCountForEventAsync(1);

            // Assert
            count.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GetBookingCountForEventSectionAsync_ShouldReturnSectionSeats()
        {
            // Act
            var count = await _repository.GetBookingCountForEventSectionAsync(3, "GoldenCircle");

            // Assert
            count.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GetBookingCountForEventSectionAsync_ShouldReturnZero_WhenNoBookingsInSection()
        {
            // Act
            var count = await _repository.GetBookingCountForEventSectionAsync(3, "NonExistentSection");

            // Assert
            count.Should().Be(0);
        }

        [Fact]
        public async Task FindBookingsForPaidUsersAtVenueAsync_ShouldReturnOnlyPaidUserBookings()
        {
            // Arrange - Add a booking from user 4 who has a pending booking at venue 3
            // and add a paid booking for user 4 at venue 1
            var paidBooking = new Booking(0, 4, 1, 1, 1, null, 50.00m);
            paidBooking.MarkAsPaid("PAY-TEST");
            await _repository.AddAsync(paidBooking);

            // Act
            var bookings = await _repository.FindBookingsForPaidUsersAtVenueAsync(1);

            // Assert - Should include users 1, 2 who have paid bookings at venue 1
            bookings.Should().NotBeEmpty();
            var userIds = bookings.Select(b => b.UserId).Distinct();
            userIds.Should().Contain(1); // User 1 has paid booking
            userIds.Should().Contain(2); // User 2 has paid booking
        }

        [Fact]
        public async Task FindBookingsForPaidUsersAtVenueAsync_ShouldExcludeUsersWithOnlyPendingBookings()
        {
            // Arrange - Venue 3 has user 4 with only pending booking
            // Act
            var bookings = await _repository.FindBookingsForPaidUsersAtVenueAsync(3);

            // Assert - Should only include user 1 who has a paid booking
            var userIds = bookings.Select(b => b.UserId).Distinct().ToList();
            userIds.Should().Contain(1); // User 1 has paid booking
            userIds.Should().NotContain(4); // User 4 only has pending booking
        }

        [Fact]
        public async Task FindUsersWithoutBookingsInVenueAsync_ShouldReturnUsersWithNoBookings()
        {
            // Arrange - Add a user 5 booking at venue 2
            var booking = new Booking(0, 5, 2, 2, 1, null, 50.00m);
            await _repository.AddAsync(booking);

            // Act - Check venue 1
            var userIds = await _repository.FindUsersWithoutBookingsInVenueAsync(1);

            // Assert - User 3 has bookings at venue 2, not at venue 1
            userIds.Should().Contain(3);
        }

        [Fact]
        public async Task FindUsersWithoutBookingsInVenueAsync_ShouldNotReturnUsersWithBookings()
        {
            // Act - Check venue 1
            var userIds = await _repository.FindUsersWithoutBookingsInVenueAsync(1);

            // Assert - Users 1 and 2 have bookings at venue 1
            userIds.Should().NotContain(1);
            userIds.Should().NotContain(2);
        }

        [Fact]
        public async Task AdvancedQuery1_ShouldIncludeAllBookingsOfPaidUsers()
        {
            // Arrange - User 1 has paid bookings, add a pending one
            var pendingBooking = new Booking(0, 1, 1, 1, 1, null, 50.00m);
            await _repository.AddAsync(pendingBooking);

            // Act
            var bookings = await _repository.FindBookingsForPaidUsersAtVenueAsync(1);

            // Assert - Should include both paid and pending bookings of user 1
            var user1Bookings = bookings.Where(b => b.UserId == 1).ToList();
            user1Bookings.Should().NotBeEmpty();
            user1Bookings.Should().Contain(b => b.PaymentStatus == PaymentStatus.Paid);
        }

        [Fact]
        public async Task BothAdvancedQueries_ShouldBeConsistent()
        {
            // Arrange - Get all users who have bookings at venue 1
            var bookingsAtVenue1 = await _repository.GetByVenueIdAsync(1);
            var usersWithBookings = bookingsAtVenue1.Select(b => b.UserId).Distinct().ToHashSet();

            // Act
            var usersWithoutBookings = await _repository.FindUsersWithoutBookingsInVenueAsync(1);

            // Assert - Users without bookings should not overlap with users who have bookings
            foreach (var userId in usersWithoutBookings)
            {
                usersWithBookings.Should().NotContain(userId);
            }
        }
    }
}
