using BookingSystem.Models;
using FluentAssertions;
using Xunit;

namespace BookingSystem.Tests.Models
{
    public class BookingTests
    {
        [Fact]
        public void Constructor_ShouldInitializeProperties_WhenValidParametersProvided()
        {
            // Arrange & Act
            var booking = new Booking(1, 10, 20, 30, 2, "VIP", 150.00m);

            // Assert
            booking.Id.Should().Be(1);
            booking.UserId.Should().Be(10);
            booking.EventId.Should().Be(20);
            booking.VenueId.Should().Be(30);
            booking.NumberOfSeats.Should().Be(2);
            booking.SectionIdentifier.Should().Be("VIP");
            booking.TotalAmount.Should().Be(150.00m);
            booking.PaymentStatus.Should().Be(PaymentStatus.Pending);
            booking.BookingDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void MarkAsPaid_ShouldUpdatePaymentStatus()
        {
            // Arrange
            var booking = new Booking(1, 10, 20, 30, 2, null, 100.00m);

            // Act
            booking.MarkAsPaid("PAY-12345");

            // Assert
            booking.PaymentStatus.Should().Be(PaymentStatus.Paid);
            booking.PaymentId.Should().Be("PAY-12345");
        }

        [Fact]
        public void MarkAsFailed_ShouldUpdatePaymentStatus()
        {
            // Arrange
            var booking = new Booking(1, 10, 20, 30, 2, null, 100.00m);

            // Act
            booking.MarkAsFailed();

            // Assert
            booking.PaymentStatus.Should().Be(PaymentStatus.Failed);
        }

        [Fact]
        public void IsPaid_ShouldReturnTrue_WhenPaymentStatusIsPaid()
        {
            // Arrange
            var booking = new Booking(1, 10, 20, 30, 2, null, 100.00m);
            booking.MarkAsPaid("PAY-12345");

            // Act
            var result = booking.IsPaid();

            // Assert
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData(PaymentStatus.Pending)]
        [InlineData(PaymentStatus.Failed)]
        [InlineData(PaymentStatus.Refunded)]
        public void IsPaid_ShouldReturnFalse_WhenPaymentStatusIsNotPaid(PaymentStatus status)
        {
            // Arrange
            var booking = new Booking(1, 10, 20, 30, 2, null, 100.00m)
            {
                PaymentStatus = status
            };

            // Act
            var result = booking.IsPaid();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void DefaultConstructor_ShouldSetDefaultValues()
        {
            // Arrange & Act
            var booking = new Booking();

            // Assert
            booking.PaymentStatus.Should().Be(PaymentStatus.Pending);
            booking.BookingDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            booking.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }
    }
}
