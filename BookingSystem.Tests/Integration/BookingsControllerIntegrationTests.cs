using BookingSystem.DataAccess;
using BookingSystem.Models;
using BookingSystem.Services.Booking;
using BookingSystem.Services.Payment;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace BookingSystem.Tests.Integration
{
    public class BookingsControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public BookingsControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    // Remove existing payment gateway registrations
                    var paymentGatewayDescriptors = services
                        .Where(d => d.ServiceType == typeof(IPaymentGateway))
                        .ToList();
                    foreach (var descriptor in paymentGatewayDescriptors)
                    {
                        services.Remove(descriptor);
                    }

                    // Register simulated payment gateway
                    services.AddScoped<IPaymentGateway, SimulatedPaymentGateway>();

                    // Re-register booking service (it depends on the payment gateway)
                    var bookingServiceDescriptors = services
                        .Where(d => d.ServiceType == typeof(IBookingService))
                        .ToList();
                    foreach (var descriptor in bookingServiceDescriptors)
                    {
                        services.Remove(descriptor);
                    }
                    services.AddScoped<IBookingService, BookingService>();
                });
            });

            _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        [Fact]
        public async Task GetAllBookings_ShouldReturnOk()
        {
            // Act
            var response = await _client.GetAsync("/api/bookings");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var bookings = await response.Content.ReadFromJsonAsync<List<Booking>>();
            bookings.Should().NotBeNull();
        }

        [Fact]
        public async Task GetBooking_ShouldReturnOk_WhenExists()
        {
            // Act
            var response = await _client.GetAsync("/api/bookings/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var booking = await response.Content.ReadFromJsonAsync<Booking>();
            booking.Should().NotBeNull();
            booking.Id.Should().Be(1);
        }

        [Fact]
        public async Task GetBooking_ShouldReturnNotFound_WhenNotExists()
        {
            // Act
            var response = await _client.GetAsync("/api/bookings/9999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetBookingsByUser_ShouldReturnOk()
        {
            // Act
            var response = await _client.GetAsync("/api/bookings/user/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var bookings = await response.Content.ReadFromJsonAsync<List<Booking>>();
            bookings.Should().NotBeNull();
            bookings.Should().OnlyContain(b => b.UserId == 1);
        }

        [Fact]
        public async Task GetBookingsByVenue_ShouldReturnOk()
        {
            // Act
            var response = await _client.GetAsync("/api/bookings/venue/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var bookings = await response.Content.ReadFromJsonAsync<List<Booking>>();
            bookings.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateBooking_ShouldReturnCreated_WhenValid()
        {
            // Arrange
            var request = new CreateBookingRequest(
                userId: 1,
                eventId: 1,
                numberOfSeats: 2,
                creditCardNumber: "4111111111111111",
                totalAmount: 150.00m
            );

            // Act
            var response = await _client.PostAsJsonAsync("/api/bookings", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var result = await response.Content.ReadFromJsonAsync<BookingResult>();
            result.Should().NotBeNull();
            result!.Success.Should().BeTrue();
            result.BookingId.Should().HaveValue();
        }

        [Fact]
        public async Task CreateBooking_ShouldReturnBadRequest_WhenInvalidCard()
        {
            // Arrange
            var request = new CreateBookingRequest(
                userId: 1,
                eventId: 1,
                numberOfSeats: 2,
                creditCardNumber: "0000", // Invalid card
                totalAmount: 150.00m
            );

            // Act
            var response = await _client.PostAsJsonAsync("/api/bookings", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var result = await response.Content.ReadFromJsonAsync<BookingResult>();
            result!.Success.Should().BeFalse();
        }

        [Fact]
        public async Task GetBookingsForPaidUsersAtVenue_ShouldReturnOk()
        {
            // Act
            var response = await _client.GetAsync("/api/bookings/venue/1/paid-users");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var bookings = await response.Content.ReadFromJsonAsync<List<Booking>>();
            bookings.Should().NotBeNull();
        }

        [Fact]
        public async Task GetUsersWithoutBookingsInVenue_ShouldReturnOk()
        {
            // Act
            var response = await _client.GetAsync("/api/bookings/venue/1/users-without-bookings");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var userIds = await response.Content.ReadFromJsonAsync<List<int>>();
            userIds.Should().NotBeNull();
        }
    }
}
