using BookingSystem.DataAccess;
using BookingSystem.Models;
using BookingSystem.Models.Seating;
using BookingSystem.Services.Booking;
using BookingSystem.Services.Payment;
using BookingSystem.Tests.Helpers;
using FluentAssertions;
using Moq;
using Xunit;

namespace BookingSystem.Tests.Services.Booking
{
    public class BookingServiceTests
    {
        private readonly Mock<IBookingRepository> _bookingRepositoryMock;
        private readonly Mock<IEventRepository> _eventRepositoryMock;
        private readonly Mock<IVenueRepository> _venueRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IPaymentGateway> _paymentGatewayMock;
        private readonly BookingService _service;

        public BookingServiceTests()
        {
            _bookingRepositoryMock = new Mock<IBookingRepository>();
            _eventRepositoryMock = new Mock<IEventRepository>();
            _venueRepositoryMock = new Mock<IVenueRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _paymentGatewayMock = new Mock<IPaymentGateway>();

            _service = new BookingService(
                _bookingRepositoryMock.Object,
                _eventRepositoryMock.Object,
                _venueRepositoryMock.Object,
                _userRepositoryMock.Object,
                _paymentGatewayMock.Object
            );
        }

        [Fact]
        public async Task CreateBookingAsync_ShouldReturnSuccess_WhenAllValidationsPass()
        {
            // Arrange
            var user = TestDataBuilder.CreateUser(1);
            var venue = TestDataBuilder.CreateVenue(1, capacity: 1000);
            var eventItem = TestDataBuilder.CreateEvent(1, venueId: 1);
            var request = new CreateBookingRequest(1, 1, 2, "4111111111111111", 100m);

            _userRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);
            _eventRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(eventItem);
            _venueRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(venue);
            _bookingRepositoryMock.Setup(x => x.GetBookingCountForEventAsync(1)).ReturnsAsync(10);
            _bookingRepositoryMock.Setup(x => x.AddAsync(It.IsAny<BookingSystem.Models.Booking>())).ReturnsAsync(123);
            _paymentGatewayMock.Setup(x => x.ProcessPaymentAsync(It.IsAny<PaymentRequest>()))
                .ReturnsAsync(new PaymentResponse(true, "PAY-123"));

            // Act
            var result = await _service.CreateBookingAsync(request);

            // Assert
            result.Success.Should().BeTrue();
            result.BookingId.Should().Be(123);
            result.PaymentId.Should().Be("PAY-123");
            _bookingRepositoryMock.Verify(x => x.AddAsync(It.IsAny<BookingSystem.Models.Booking>()), Times.Once);
        }

        [Fact]
        public async Task CreateBookingAsync_ShouldReturnFailure_WhenPaymentIsRejected()
        {
            // Arrange
            var user = TestDataBuilder.CreateUser(1);
            var venue = TestDataBuilder.CreateVenue(1);
            var eventItem = TestDataBuilder.CreateEvent(1, venueId: 1);
            var request = new CreateBookingRequest(1, 1, 2, "0000", 100m);

            _userRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);
            _eventRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(eventItem);
            _venueRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(venue);
            _bookingRepositoryMock.Setup(x => x.GetBookingCountForEventAsync(1)).ReturnsAsync(10);
            _paymentGatewayMock.Setup(x => x.ProcessPaymentAsync(It.IsAny<PaymentRequest>()))
                .ReturnsAsync(new PaymentResponse(false, null));

            // Act
            var result = await _service.CreateBookingAsync(request);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Payment was rejected");
            _bookingRepositoryMock.Verify(x => x.AddAsync(It.IsAny<BookingSystem.Models.Booking>()), Times.Never);
        }

        [Fact]
        public async Task CreateBookingAsync_ShouldReturnFailure_WhenUserNotFound()
        {
            // Arrange
            var request = new CreateBookingRequest(999, 1, 2, "4111111111111111", 100m);
            _userRepositoryMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((User)null);

            // Act
            var result = await _service.CreateBookingAsync(request);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("User with ID 999 not found");
        }

        [Fact]
        public async Task CreateBookingAsync_ShouldReturnFailure_WhenEventNotFound()
        {
            // Arrange
            var user = TestDataBuilder.CreateUser(1);
            var request = new CreateBookingRequest(1, 999, 2, "4111111111111111", 100m);

            _userRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);
            _eventRepositoryMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Event)null);

            // Act
            var result = await _service.CreateBookingAsync(request);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Event with ID 999 not found");
        }

        [Fact]
        public async Task CreateBookingAsync_ShouldReturnFailure_WhenVenueNotFound()
        {
            // Arrange
            var user = TestDataBuilder.CreateUser(1);
            var eventItem = TestDataBuilder.CreateEvent(1, venueId: 999);
            var request = new CreateBookingRequest(1, 1, 2, "4111111111111111", 100m);

            _userRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);
            _eventRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(eventItem);
            _venueRepositoryMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Venue)null);

            // Act
            var result = await _service.CreateBookingAsync(request);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Venue with ID 999 not found");
        }

        [Fact]
        public async Task CreateBookingAsync_ShouldReturnFailure_WhenEventIsInPast()
        {
            // Arrange
            var user = TestDataBuilder.CreateUser(1);
            var venue = TestDataBuilder.CreateVenue(1);
            var pastEvent = TestDataBuilder.CreateEvent(1, venueId: 1, eventDate: DateTime.UtcNow.AddDays(-10));
            var request = new CreateBookingRequest(1, 1, 2, "4111111111111111", 100m);

            _userRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);
            _eventRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(pastEvent);
            _venueRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(venue);

            // Act
            var result = await _service.CreateBookingAsync(request);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Cannot book tickets for past events");
        }

        [Fact]
        public async Task CreateBookingAsync_ShouldReturnFailure_WhenInsufficientCapacity()
        {
            // Arrange
            var user = TestDataBuilder.CreateUser(1);
            var venue = TestDataBuilder.CreateVenue(1, capacity: 100);
            var eventItem = TestDataBuilder.CreateEvent(1, venueId: 1);
            var request = new CreateBookingRequest(1, 1, 50, "4111111111111111", 500m);

            _userRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);
            _eventRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(eventItem);
            _venueRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(venue);
            _bookingRepositoryMock.Setup(x => x.GetBookingCountForEventAsync(1)).ReturnsAsync(60); // 60 already booked

            // Act
            var result = await _service.CreateBookingAsync(request);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Insufficient capacity");
            result.Message.Should().Contain("Requested: 50");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-10)]
        public async Task CreateBookingAsync_ShouldReturnFailure_WhenNumberOfSeatsIsInvalid(int seats)
        {
            // Arrange
            var request = new CreateBookingRequest(1, 1, seats, "4111111111111111", 100m);

            // Act
            var result = await _service.CreateBookingAsync(request);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Number of seats must be greater than zero");
        }

        [Fact]
        public async Task CreateBookingAsync_ShouldThrowException_WhenRequestIsNull()
        {
            // Arrange
            CreateBookingRequest request = null;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.CreateBookingAsync(request));
        }

        [Fact]
        public async Task CreateBookingAsync_ShouldHandleSectionReservedSeating()
        {
            // Arrange
            var user = TestDataBuilder.CreateUser(1);
            var venue = TestDataBuilder.CreateVenue(1, capacity: 300);
            var seatingType = TestDataBuilder.CreateSectionReservedSeating();
            var eventItem = TestDataBuilder.CreateEvent(1, venueId: 1, seatingType: seatingType);
            var request = new CreateBookingRequest(1, 1, 2, "4111111111111111", 100m, "VIP");

            _userRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);
            _eventRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(eventItem);
            _venueRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(venue);
            _bookingRepositoryMock.Setup(x => x.GetBookingCountForEventAsync(1)).ReturnsAsync(10);
            _bookingRepositoryMock.Setup(x => x.GetBookingCountForEventSectionAsync(1, "VIP")).ReturnsAsync(5);
            _bookingRepositoryMock.Setup(x => x.AddAsync(It.IsAny<BookingSystem.Models.Booking>())).ReturnsAsync(123);
            _paymentGatewayMock.Setup(x => x.ProcessPaymentAsync(It.IsAny<PaymentRequest>()))
                .ReturnsAsync(new PaymentResponse(true, "PAY-123"));

            // Act
            var result = await _service.CreateBookingAsync(request);

            // Assert
            result.Success.Should().BeTrue();
            _bookingRepositoryMock.Verify(x => x.GetBookingCountForEventSectionAsync(1, "VIP"), Times.Once);
        }

        [Fact]
        public async Task CreateBookingAsync_ShouldReturnFailure_WhenPaymentGatewayThrowsException()
        {
            // Arrange
            var user = TestDataBuilder.CreateUser(1);
            var venue = TestDataBuilder.CreateVenue(1);
            var eventItem = TestDataBuilder.CreateEvent(1, venueId: 1);
            var request = new CreateBookingRequest(1, 1, 2, "4111111111111111", 100m);

            _userRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);
            _eventRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(eventItem);
            _venueRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(venue);
            _bookingRepositoryMock.Setup(x => x.GetBookingCountForEventAsync(1)).ReturnsAsync(10);
            _paymentGatewayMock.Setup(x => x.ProcessPaymentAsync(It.IsAny<PaymentRequest>()))
                .ThrowsAsync(new InvalidOperationException("Payment service unavailable"));

            // Act
            var result = await _service.CreateBookingAsync(request);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Payment processing failed");
            _bookingRepositoryMock.Verify(x => x.AddAsync(It.IsAny<BookingSystem.Models.Booking>()), Times.Never);
        }

        [Fact]
        public async Task CreateBookingAsync_ShouldStoreBookingWithCorrectPaymentDetails()
        {
            // Arrange
            var user = TestDataBuilder.CreateUser(1);
            var venue = TestDataBuilder.CreateVenue(1);
            var eventItem = TestDataBuilder.CreateEvent(1, venueId: 1);
            var request = new CreateBookingRequest(1, 1, 2, "4111111111111111", 150.00m);

            BookingSystem.Models.Booking capturedBooking = null;
            _userRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);
            _eventRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(eventItem);
            _venueRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(venue);
            _bookingRepositoryMock.Setup(x => x.GetBookingCountForEventAsync(1)).ReturnsAsync(10);
            _bookingRepositoryMock.Setup(x => x.AddAsync(It.IsAny<BookingSystem.Models.Booking>()))
                .Callback<BookingSystem.Models.Booking>(b => capturedBooking = b)
                .ReturnsAsync(123);
            _paymentGatewayMock.Setup(x => x.ProcessPaymentAsync(It.IsAny<PaymentRequest>()))
                .ReturnsAsync(new PaymentResponse(true, "PAY-XYZ-789"));

            // Act
            var result = await _service.CreateBookingAsync(request);

            // Assert
            capturedBooking.Should().NotBeNull();
            capturedBooking.PaymentStatus.Should().Be(PaymentStatus.Paid);
            capturedBooking.PaymentId.Should().Be("PAY-XYZ-789");
            capturedBooking.TotalAmount.Should().Be(150.00m);
            capturedBooking.NumberOfSeats.Should().Be(2);
        }
    }
}
