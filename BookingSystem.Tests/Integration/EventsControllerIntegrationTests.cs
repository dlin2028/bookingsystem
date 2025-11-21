using BookingSystem.Models;
using BookingSystem.Models.Seating;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace BookingSystem.Tests.Integration
{
    public class EventsControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _jsonOptions;

        public EventsControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            var customFactory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    // Configure for .NET 9 compatibility
                });
            });

            _client = customFactory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            // Configure JSON options with the seating type converter
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = null,
                Converters = { new SeatingTypeJsonConverter() }
            };
        }

        [Fact]
        public async Task GetAllEvents_ShouldReturnOk()
        {
            // Act
            var response = await _client.GetAsync("/api/events");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var events = await response.Content.ReadFromJsonAsync<List<Event>>(_jsonOptions);
            events.Should().NotBeNull();
        }

        [Fact]
        public async Task GetEvent_ShouldReturnOk_WhenExists()
        {
            // Act
            var response = await _client.GetAsync("/api/events/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var eventItem = await response.Content.ReadFromJsonAsync<Event>(_jsonOptions);
            eventItem.Should().NotBeNull();
            eventItem!.Id.Should().Be(1);
        }

        [Fact]
        public async Task GetFutureEvents_ShouldReturnOnlyFutureEvents()
        {
            // Act
            var response = await _client.GetAsync("/api/events/future");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var events = await response.Content.ReadFromJsonAsync<List<Event>>(_jsonOptions);
            events.Should().NotBeNull();
            events!.Should().OnlyContain(e => e.EventDate > DateTime.UtcNow);
        }

        [Fact]
        public async Task GetFutureEventsWithAvailability_ShouldReturnEventDetails()
        {
            // Act
            var response = await _client.GetAsync("/api/events/future-with-availability");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeEmpty();

            // The response should be an array of objects with availability info
            content.Should().Contain("EventId");
            content.Should().Contain("EventName");
            content.Should().Contain("AvailableSeats");
            content.Should().Contain("TotalCapacity");
        }

        [Fact]
        public async Task GetEvent_ShouldReturnNotFound_WhenNotExists()
        {
            // Act
            var response = await _client.GetAsync("/api/events/9999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
