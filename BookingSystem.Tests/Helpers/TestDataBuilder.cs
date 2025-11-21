using BookingSystem.Models;
using BookingSystem.Models.Seating;
using System;
using System.Collections.Generic;

namespace BookingSystem.Tests.Helpers
{
    /// <summary>
    /// Builder class for creating test data objects
    /// </summary>
    public static class TestDataBuilder
    {
        public static User CreateUser(
            int id = 1,
            string firstName = "John",
            string lastName = "Doe",
            string email = "john.doe@example.com")
        {
            return new User(id, firstName, lastName, email);
        }

        public static Venue CreateVenue(
            int id = 1,
            string name = "Test Venue",
            string location = "123 Test St",
            int capacity = 1000)
        {
            return new Venue(id, name, location, capacity);
        }

        public static Event CreateEvent(
            int id = 1,
            string name = "Test Event",
            int venueId = 1,
            DateTime? eventDate = null,
            ISeatingType seatingType = null)
        {
            return new Event(
                id,
                name,
                "Test Description",
                venueId,
                eventDate ?? DateTime.UtcNow.AddDays(30),
                "Concert",
                seatingType ?? new OpenSeating()
            );
        }

        public static Booking CreateBooking(
            int id = 1,
            int userId = 1,
            int eventId = 1,
            int venueId = 1,
            int seats = 2,
            PaymentStatus status = PaymentStatus.Paid,
            string paymentId = "PAY-123")
        {
            var booking = new Booking(id, userId, eventId, venueId, seats, null, 100.00m);
            if (status == PaymentStatus.Paid)
            {
                booking.MarkAsPaid(paymentId);
            }
            return booking;
        }

        public static FullReservedSeating CreateFullReservedSeating(int totalSeats = 1000)
        {
            return new FullReservedSeating(totalSeats);
        }

        public static SectionReservedSeating CreateSectionReservedSeating()
        {
            return new SectionReservedSeating(new Dictionary<string, int>
            {
                { "VIP", 100 },
                { "Premium", 200 },
                { "General", 500 }
            });
        }

        public static OpenSeating CreateOpenSeating()
        {
            return new OpenSeating();
        }
    }
}
