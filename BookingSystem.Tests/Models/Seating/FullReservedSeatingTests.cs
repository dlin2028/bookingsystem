using BookingSystem.Models.Seating;
using FluentAssertions;
using Xunit;

namespace BookingSystem.Tests.Models.Seating
{
    public class FullReservedSeatingTests
    {
        [Fact]
        public void Constructor_ShouldSetTotalSeats()
        {
            // Arrange & Act
            var seating = new FullReservedSeating(1000);

            // Assert
            seating.TotalSeats.Should().Be(1000);
            seating.SeatingTypeName.Should().Be("Full Reserved Seating");
        }

        [Theory]
        [InlineData(1000, 200, 800)]
        [InlineData(500, 0, 500)]
        [InlineData(1000, 1000, 0)]
        public void GetAvailableCapacity_ShouldReturnCorrectValue(int totalCapacity, int currentBookings, int expected)
        {
            // Arrange
            var seating = new FullReservedSeating(totalCapacity);

            // Act
            var result = seating.GetAvailableCapacity(totalCapacity, currentBookings);

            // Assert
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData(10, 100, true)]
        [InlineData(50, 50, true)]
        [InlineData(51, 50, false)]
        [InlineData(100, 99, false)]
        public void CanAccommodateBooking_ShouldReturnCorrectValue(int requestedSeats, int availableCapacity, bool expected)
        {
            // Arrange
            var seating = new FullReservedSeating(1000);

            // Act
            var result = seating.CanAccommodateBooking(requestedSeats, availableCapacity);

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void CanAccommodateBooking_ShouldIgnoreSectionIdentifier()
        {
            // Arrange
            var seating = new FullReservedSeating(1000);

            // Act
            var result = seating.CanAccommodateBooking(10, 100, "SomeSection");

            // Assert - should work even with section identifier (it's ignored)
            result.Should().BeTrue();
        }

        [Fact]
        public void GetSectionInfo_ShouldReturnTotalSeatsInfo()
        {
            // Arrange
            var seating = new FullReservedSeating(1000);

            // Act
            var info = seating.GetSectionInfo();

            // Assert
            info.Should().Contain("1000");
            info.Should().Contain("Total Seats");
        }
    }
}
