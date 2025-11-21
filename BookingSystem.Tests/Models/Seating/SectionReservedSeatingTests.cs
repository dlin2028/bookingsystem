using BookingSystem.Models.Seating;
using FluentAssertions;
using Xunit;

namespace BookingSystem.Tests.Models.Seating
{
    public class SectionReservedSeatingTests
    {
        [Fact]
        public void Constructor_ShouldInitializeSections()
        {
            // Arrange
            var sections = new Dictionary<string, int>
            {
                { "VIP", 100 },
                { "Premium", 200 },
                { "General", 500 }
            };

            // Act
            var seating = new SectionReservedSeating(sections);

            // Assert
            seating.Sections.Should().HaveCount(3);
            seating.Sections["VIP"].Should().Be(100);
            seating.Sections["Premium"].Should().Be(200);
            seating.Sections["General"].Should().Be(500);
            seating.SeatingTypeName.Should().Be("Section Reserved Seating");
        }

        [Fact]
        public void Constructor_ShouldHandleNullSections()
        {
            // Arrange & Act
            var seating = new SectionReservedSeating(null);

            // Assert
            seating.Sections.Should().NotBeNull();
            seating.Sections.Should().BeEmpty();
        }

        [Theory]
        [InlineData(1000, 200, 800)]
        [InlineData(500, 500, 0)]
        public void GetAvailableCapacity_ShouldReturnCorrectValue(int totalCapacity, int currentBookings, int expected)
        {
            // Arrange
            var sections = new Dictionary<string, int> { { "VIP", 100 } };
            var seating = new SectionReservedSeating(sections);

            // Act
            var result = seating.GetAvailableCapacity(totalCapacity, currentBookings);

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void CanAccommodateBooking_ShouldReturnTrue_WhenSectionExistsAndHasCapacity()
        {
            // Arrange
            var sections = new Dictionary<string, int> { { "VIP", 100 } };
            var seating = new SectionReservedSeating(sections);

            // Act
            var result = seating.CanAccommodateBooking(10, 50, "VIP");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void CanAccommodateBooking_ShouldReturnFalse_WhenSectionIdentifierIsNull()
        {
            // Arrange
            var sections = new Dictionary<string, int> { { "VIP", 100 } };
            var seating = new SectionReservedSeating(sections);

            // Act
            var result = seating.CanAccommodateBooking(10, 50, null);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void CanAccommodateBooking_ShouldReturnFalse_WhenSectionIdentifierIsEmpty()
        {
            // Arrange
            var sections = new Dictionary<string, int> { { "VIP", 100 } };
            var seating = new SectionReservedSeating(sections);

            // Act
            var result = seating.CanAccommodateBooking(10, 50, "");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void CanAccommodateBooking_ShouldReturnFalse_WhenSectionDoesNotExist()
        {
            // Arrange
            var sections = new Dictionary<string, int> { { "VIP", 100 } };
            var seating = new SectionReservedSeating(sections);

            // Act
            var result = seating.CanAccommodateBooking(10, 50, "NonExistentSection");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void CanAccommodateBooking_ShouldReturnFalse_WhenInsufficientCapacity()
        {
            // Arrange
            var sections = new Dictionary<string, int> { { "VIP", 100 } };
            var seating = new SectionReservedSeating(sections);

            // Act
            var result = seating.CanAccommodateBooking(100, 50, "VIP");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void GetSectionInfo_ShouldReturnFormattedSectionDetails()
        {
            // Arrange
            var sections = new Dictionary<string, int>
            {
                { "VIP", 100 },
                { "Premium", 200 }
            };
            var seating = new SectionReservedSeating(sections);

            // Act
            var info = seating.GetSectionInfo();

            // Assert
            info.Should().Contain("VIP");
            info.Should().Contain("100");
            info.Should().Contain("Premium");
            info.Should().Contain("200");
            info.Should().Contain("Sections");
        }

        [Fact]
        public void GetSectionCapacity_ShouldReturnCorrectCapacity_WhenSectionExists()
        {
            // Arrange
            var sections = new Dictionary<string, int> { { "VIP", 150 } };
            var seating = new SectionReservedSeating(sections);

            // Act
            var capacity = seating.GetSectionCapacity("VIP");

            // Assert
            capacity.Should().Be(150);
        }

        [Fact]
        public void GetSectionCapacity_ShouldReturnZero_WhenSectionDoesNotExist()
        {
            // Arrange
            var sections = new Dictionary<string, int> { { "VIP", 150 } };
            var seating = new SectionReservedSeating(sections);

            // Act
            var capacity = seating.GetSectionCapacity("NonExistent");

            // Assert
            capacity.Should().Be(0);
        }
    }
}
