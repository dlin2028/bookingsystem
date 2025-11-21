using BookingSystem.Models;
using BookingSystem.Models.Seating;
using FluentAssertions;
using Xunit;

namespace BookingSystem.Tests.Models
{
    public class EventTests
    {
        [Fact]
        public void Constructor_ShouldInitializeProperties_WhenValidParametersProvided()
        {
            // Arrange
            var eventDate = DateTime.UtcNow.AddDays(30);
            var seatingType = new OpenSeating();

            // Act
            var eventItem = new Event(1, "Rock Concert", "Amazing show", 10, eventDate, "Concert", seatingType);

            // Assert
            eventItem.Id.Should().Be(1);
            eventItem.Name.Should().Be("Rock Concert");
            eventItem.Description.Should().Be("Amazing show");
            eventItem.VenueId.Should().Be(10);
            eventItem.EventDate.Should().Be(eventDate);
            eventItem.EventType.Should().Be("Concert");
            eventItem.SeatingType.Should().Be(seatingType);
            eventItem.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void IsFutureEvent_ShouldReturnTrue_WhenEventIsInFuture()
        {
            // Arrange
            var futureDate = DateTime.UtcNow.AddDays(10);
            var eventItem = new Event(1, "Future Event", "Test", 1, futureDate, "Concert", new OpenSeating());

            // Act
            var result = eventItem.IsFutureEvent();

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void IsFutureEvent_ShouldReturnFalse_WhenEventIsInPast()
        {
            // Arrange
            var pastDate = DateTime.UtcNow.AddDays(-10);
            var eventItem = new Event(1, "Past Event", "Test", 1, pastDate, "Concert", new OpenSeating());

            // Act
            var result = eventItem.IsFutureEvent();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void IsFutureEvent_ShouldReturnFalse_WhenEventIsNow()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var eventItem = new Event(1, "Current Event", "Test", 1, now, "Concert", new OpenSeating());

            // Act
            var result = eventItem.IsFutureEvent();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void SeatingType_CanBeFullReservedSeating()
        {
            // Arrange
            var seatingType = new FullReservedSeating(1000);

            // Act
            var eventItem = new Event(1, "Concert", "Test", 1, DateTime.UtcNow.AddDays(10), "Concert", seatingType);

            // Assert
            eventItem.SeatingType.Should().BeOfType<FullReservedSeating>();
        }

        [Fact]
        public void SeatingType_CanBeSectionReservedSeating()
        {
            // Arrange
            var seatingType = new SectionReservedSeating(new Dictionary<string, int> { { "VIP", 100 } });

            // Act
            var eventItem = new Event(1, "Concert", "Test", 1, DateTime.UtcNow.AddDays(10), "Concert", seatingType);

            // Assert
            eventItem.SeatingType.Should().BeOfType<SectionReservedSeating>();
        }

        [Fact]
        public void SeatingType_CanBeOpenSeating()
        {
            // Arrange
            var seatingType = new OpenSeating();

            // Act
            var eventItem = new Event(1, "Festival", "Test", 1, DateTime.UtcNow.AddDays(10), "Festival", seatingType);

            // Assert
            eventItem.SeatingType.Should().BeOfType<OpenSeating>();
        }
    }
}
