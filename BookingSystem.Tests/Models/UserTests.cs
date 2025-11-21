using BookingSystem.Models;
using FluentAssertions;
using Xunit;

namespace BookingSystem.Tests.Models
{
    public class UserTests
    {
        [Fact]
        public void Constructor_ShouldInitializeProperties_WhenValidParametersProvided()
        {
            // Arrange & Act
            var user = new User(1, "John", "Doe", "john.doe@example.com");

            // Assert
            user.Id.Should().Be(1);
            user.FirstName.Should().Be("John");
            user.LastName.Should().Be("Doe");
            user.Email.Should().Be("john.doe@example.com");
            user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void FullName_ShouldReturnCombinedName()
        {
            // Arrange
            var user = new User(1, "John", "Doe", "john.doe@example.com");

            // Act
            var fullName = user.FullName;

            // Assert
            fullName.Should().Be("John Doe");
        }

        [Fact]
        public void DefaultConstructor_ShouldSetCreatedAt()
        {
            // Arrange & Act
            var user = new User();

            // Assert
            user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }
    }
}
