using BookingSystem.DataAccess;
using BookingSystem.Models;
using BookingSystem.Models.Seating;
using BookingSystem.Services.Booking;
using BookingSystem.Services.Payment;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookingSystem.Examples
{
    /// <summary>
    /// Demonstrates how to use the booking system programmatically
    /// </summary>
    public class BookingExamples
    {
        /// <summary>
        /// Example 1: Create a booking for a full reserved seating event
        /// </summary>
        public static async Task<BookingResult> CreateFullReservedBookingExample(
            IBookingService bookingService)
        {
            var request = new CreateBookingRequest(
                userId: 1,
                eventId: 1,
                numberOfSeats: 2,
                creditCardNumber: "4111111111111111",
                totalAmount: 150.00m,
                sectionIdentifier: null // No section for full reserved
            );

            var result = await bookingService.CreateBookingAsync(request);

            if (result.Success)
            {
                Console.WriteLine($"Booking created successfully!");
                Console.WriteLine($"Booking ID: {result.BookingId}");
                Console.WriteLine($"Payment ID: {result.PaymentId}");
            }
            else
            {
                Console.WriteLine($"Booking failed: {result.Message}");
            }

            return result;
        }

        /// <summary>
        /// Example 2: Create a booking for a section reserved seating event
        /// </summary>
        public static async Task<BookingResult> CreateSectionReservedBookingExample(
            IBookingService bookingService)
        {
            var request = new CreateBookingRequest(
                userId: 2,
                eventId: 3, // Jazz Night with sections
                numberOfSeats: 2,
                creditCardNumber: "4532111111111111",
                totalAmount: 200.00m,
                sectionIdentifier: "GoldenCircle" // Must specify section
            );

            var result = await bookingService.CreateBookingAsync(request);

            if (result.Success)
            {
                Console.WriteLine($"Section booking created successfully!");
                Console.WriteLine($"Booking ID: {result.BookingId}");
                Console.WriteLine($"Section: GoldenCircle");
            }
            else
            {
                Console.WriteLine($"Booking failed: {result.Message}");
            }

            return result;
        }

        /// <summary>
        /// Example 3: Create a booking for an open seating event
        /// </summary>
        public static async Task<BookingResult> CreateOpenSeatingBookingExample(
            IBookingService bookingService)
        {
            var request = new CreateBookingRequest(
                userId: 3,
                eventId: 2, // Summer Music Festival
                numberOfSeats: 5,
                creditCardNumber: "5555555555554444",
                totalAmount: 250.00m,
                sectionIdentifier: null // No section for open seating
            );

            var result = await bookingService.CreateBookingAsync(request);

            if (result.Success)
            {
                Console.WriteLine($"Open seating booking created successfully!");
                Console.WriteLine($"Booking ID: {result.BookingId}");
            }
            else
            {
                Console.WriteLine($"Booking failed: {result.Message}");
            }

            return result;
        }

        /// <summary>
        /// Example 4: Create an event with full reserved seating
        /// </summary>
        public static async Task<int> CreateFullReservedEventExample(IEventRepository eventRepository)
        {
            var seatingType = new FullReservedSeating(totalSeats: 1000);

            var newEvent = new Event(
                id: 0, // Will be assigned by repository
                name: "Classical Orchestra Performance",
                description: "An evening of classical music",
                venueId: 1,
                eventDate: DateTime.UtcNow.AddMonths(2),
                eventType: "Concert",
                seatingType: seatingType
            );

            var eventId = await eventRepository.AddAsync(newEvent);
            Console.WriteLine($"Event created with ID: {eventId}");
            Console.WriteLine($"Seating: {seatingType.SeatingTypeName}");

            return eventId;
        }

        /// <summary>
        /// Example 5: Create an event with section reserved seating
        /// </summary>
        public static async Task<int> CreateSectionReservedEventExample(IEventRepository eventRepository)
        {
            var sections = new Dictionary<string, int>
            {
                { "VIP", 50 },
                { "Premium", 150 },
                { "General", 300 }
            };

            var seatingType = new SectionReservedSeating(sections);

            var newEvent = new Event(
                id: 0,
                name: "Rock Festival 2025",
                description: "Multi-stage rock festival",
                venueId: 2,
                eventDate: DateTime.UtcNow.AddMonths(3),
                eventType: "Festival",
                seatingType: seatingType
            );

            var eventId = await eventRepository.AddAsync(newEvent);
            Console.WriteLine($"Event created with ID: {eventId}");
            Console.WriteLine($"Sections: {seatingType.GetSectionInfo()}");

            return eventId;
        }

        /// <summary>
        /// Example 6: Create an event with open seating
        /// </summary>
        public static async Task<int> CreateOpenSeatingEventExample(IEventRepository eventRepository)
        {
            var seatingType = new OpenSeating();

            var newEvent = new Event(
                id: 0,
                name: "Outdoor Jazz Festival",
                description: "Relaxed outdoor jazz event",
                venueId: 2,
                eventDate: DateTime.UtcNow.AddMonths(4),
                eventType: "Festival",
                seatingType: seatingType
            );

            var eventId = await eventRepository.AddAsync(newEvent);
            Console.WriteLine($"Event created with ID: {eventId}");
            Console.WriteLine($"Seating: {seatingType.SeatingTypeName}");

            return eventId;
        }

        /// <summary>
        /// Example 7: Query bookings for paid users at a venue (Advanced Query 1)
        /// </summary>
        public static async Task DemonstratePaidUsersQuery(IBookingRepository bookingRepository)
        {
            int venueId = 1;
            var bookings = await bookingRepository.FindBookingsForPaidUsersAtVenueAsync(venueId);

            Console.WriteLine($"Bookings for paid users at venue {venueId}:");
            foreach (var booking in bookings)
            {
                Console.WriteLine($"  - Booking ID: {booking.Id}, User: {booking.UserId}, " +
                                $"Status: {booking.PaymentStatus}, Amount: ${booking.TotalAmount}");
            }
        }

        /// <summary>
        /// Example 8: Query users without bookings in a venue (Advanced Query 2)
        /// </summary>
        public static async Task DemonstrateUsersWithoutBookingsQuery(
            IBookingRepository bookingRepository,
            IUserRepository userRepository)
        {
            int venueId = 1;
            var userIds = await bookingRepository.FindUsersWithoutBookingsInVenueAsync(venueId);

            Console.WriteLine($"Users without bookings at venue {venueId}:");
            foreach (var userId in userIds)
            {
                var user = await userRepository.GetByIdAsync(userId);
                if (user != null)
                {
                    Console.WriteLine($"  - User ID: {userId}, Name: {user.FullName}, Email: {user.Email}");
                }
            }
        }

        /// <summary>
        /// Example 9: Get all bookings by user
        /// </summary>
        public static async Task GetUserBookingsExample(
            IBookingRepository bookingRepository,
            IUserRepository userRepository,
            int userId)
        {
            var user = await userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                Console.WriteLine($"User {userId} not found");
                return;
            }

            var bookings = await bookingRepository.GetByUserIdAsync(userId);
            Console.WriteLine($"Bookings for {user.FullName}:");

            foreach (var booking in bookings)
            {
                Console.WriteLine($"  - Event ID: {booking.EventId}, Seats: {booking.NumberOfSeats}, " +
                                $"Status: {booking.PaymentStatus}, Amount: ${booking.TotalAmount}");
            }
        }

        /// <summary>
        /// Example 10: Get all bookings by venue
        /// </summary>
        public static async Task GetVenueBookingsExample(
            IBookingRepository bookingRepository,
            IVenueRepository venueRepository,
            int venueId)
        {
            var venue = await venueRepository.GetByIdAsync(venueId);
            if (venue == null)
            {
                Console.WriteLine($"Venue {venueId} not found");
                return;
            }

            var bookings = await bookingRepository.GetByVenueIdAsync(venueId);
            Console.WriteLine($"Bookings at {venue.Name}:");

            int totalSeatsBooked = 0;
            foreach (var booking in bookings)
            {
                totalSeatsBooked += booking.NumberOfSeats;
                Console.WriteLine($"  - Booking ID: {booking.Id}, User: {booking.UserId}, " +
                                $"Seats: {booking.NumberOfSeats}, Status: {booking.PaymentStatus}");
            }

            Console.WriteLine($"Total seats booked: {totalSeatsBooked} / {venue.TotalCapacity}");
            Console.WriteLine($"Available: {venue.TotalCapacity - totalSeatsBooked}");
        }

        /// <summary>
        /// Example 11: Demonstrate payment rejection
        /// </summary>
        public static async Task<BookingResult> DemonstratePaymentRejectionExample(
            IBookingService bookingService)
        {
            // Using an invalid card number
            var request = new CreateBookingRequest(
                userId: 1,
                eventId: 1,
                numberOfSeats: 2,
                creditCardNumber: "0000", // Invalid - too short
                totalAmount: 150.00m,
                sectionIdentifier: null
            );

            var result = await bookingService.CreateBookingAsync(request);

            Console.WriteLine($"Expected failure - Result: {result.Message}");
            return result;
        }

        /// <summary>
        /// Example 12: Demonstrate capacity limit enforcement
        /// </summary>
        public static async Task<BookingResult> DemonstrateCapacityLimitExample(
            IBookingService bookingService)
        {
            // Try to book more seats than available
            var request = new CreateBookingRequest(
                userId: 1,
                eventId: 3, // Jazz Club with only 300 capacity
                numberOfSeats: 400, // Exceeds capacity
                creditCardNumber: "4111111111111111",
                totalAmount: 5000.00m,
                sectionIdentifier: "GoldenCircle"
            );

            var result = await bookingService.CreateBookingAsync(request);

            Console.WriteLine($"Expected failure - Result: {result.Message}");
            return result;
        }
    }
}
