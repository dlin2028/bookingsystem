using BookingSystem.Models.Seating;
using FluentAssertions;
using Xunit;

namespace BookingSystem.Tests.Models.Seating
{
    public class OpenSeatingTests
    {
        [Fact]
        public void Constructor_ShouldSetSeatingTypeName()
        {
            // Arrange & Act
            var seating = new OpenSeating();

            // Assert
            seating.SeatingTypeName.Should().Be("Open Seating");
        }

        [Theory]
        [InlineData(1000, 200, 800)]
        [InlineData(5000, 0, 5000)]
        [InlineData(1000, 1000, 0)]
        public void GetAvailableCapacity_ShouldReturnCorrectValue(int totalCapacity, int currentBookings, int expected)
        {
            // Arrange
            var seating = new OpenSeating();

            // Act
            var result = seating.GetAvailableCapacity(totalCapacity, currentBookings);

            // Assert
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData(10, 100, true)]
        [InlineData(50, 50, true)]
        [InlineData(51, 50, false)]
        [InlineData(1, 0, false)]
        public void CanAccommodateBooking_ShouldReturnCorrectValue(int requestedSeats, int availableCapacity, bool expected)
        {
            // Arrange
            var seating = new OpenSeating();

            // Act
            var result = seating.CanAccommodateBooking(requestedSeats, availableCapacity);

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void CanAccommodateBooking_ShouldIgnoreSectionIdentifier()
        {
            // Arrange
            var seating = new OpenSeating();

            // Act
            var result = seating.CanAccommodateBooking(10, 100, "SomeSection");

            // Assert - should work even with section identifier (it's ignored for open seating)
            result.Should().BeTrue();
        }

        [Fact]
        public void GetSectionInfo_ShouldReturnOpenSeatingMessage()
        {
            // Arrange
            var seating = new OpenSeating();

            // Act
            var info = seating.GetSectionInfo();

            // Assert
            info.Should().Contain("Open seating");
            info.Should().Contain("no specific sections");
        }
    }
}
