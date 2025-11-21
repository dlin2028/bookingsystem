using BookingSystem.Models;
using FluentAssertions;
using Xunit;

namespace BookingSystem.Tests.Models
{
    public class VenueTests
    {
        [Fact]
        public void Constructor_ShouldInitializeProperties_WhenValidParametersProvided()
        {
            // Arrange & Act
            var venue = new Venue(1, "Grand Hall", "123 Main St", 5000);

            // Assert
            venue.Id.Should().Be(1);
            venue.Name.Should().Be("Grand Hall");
            venue.Location.Should().Be("123 Main St");
            venue.TotalCapacity.Should().Be(5000);
            venue.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Theory]
        [InlineData(100)]
        [InlineData(5000)]
        [InlineData(20000)]
        public void Constructor_ShouldAcceptValidCapacity(int capacity)
        {
            // Arrange & Act
            var venue = new Venue(1, "Test Venue", "Test Location", capacity);

            // Assert
            venue.TotalCapacity.Should().Be(capacity);
        }

        [Fact]
        public void DefaultConstructor_ShouldSetCreatedAt()
        {
            // Arrange & Act
            var venue = new Venue();

            // Assert
            venue.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }
    }
}
