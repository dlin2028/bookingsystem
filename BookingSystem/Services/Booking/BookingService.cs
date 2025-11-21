using BookingSystem.DataAccess;
using BookingSystem.Models;
using BookingSystem.Services.Payment;
using System;
using System.Threading.Tasks;

namespace BookingSystem.Services.Booking
{
    /// <summary>
    /// Booking service with capacity checks and payment integration
    /// </summary>
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IEventRepository _eventRepository;
        private readonly IVenueRepository _venueRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPaymentGateway _paymentGateway;

        public BookingService(
            IBookingRepository bookingRepository,
            IEventRepository eventRepository,
            IVenueRepository venueRepository,
            IUserRepository userRepository,
            IPaymentGateway paymentGateway)
        {
            _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
            _eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
            _venueRepository = venueRepository ?? throw new ArgumentNullException(nameof(venueRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _paymentGateway = paymentGateway ?? throw new ArgumentNullException(nameof(paymentGateway));
        }

        public async Task<BookingResult> CreateBookingAsync(CreateBookingRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.NumberOfSeats <= 0)
                return BookingResult.FailureResult("Number of seats must be greater than zero");

            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
                return BookingResult.FailureResult($"User with ID {request.UserId} not found");

            var eventItem = await _eventRepository.GetByIdAsync(request.EventId);
            if (eventItem == null)
                return BookingResult.FailureResult($"Event with ID {request.EventId} not found");

            if (!eventItem.IsFutureEvent())
                return BookingResult.FailureResult("Cannot book tickets for past events");

            var venue = await _venueRepository.GetByIdAsync(eventItem.VenueId);
            if (venue == null)
                return BookingResult.FailureResult($"Venue with ID {eventItem.VenueId} not found");

            var currentBookings = await _bookingRepository.GetBookingCountForEventAsync(request.EventId);
            var availableCapacity = venue.TotalCapacity - currentBookings;

            if (eventItem.SeatingType != null)
            {
                // For section-reserved seating, we need to check section-specific capacity
                var sectionAvailableCapacity = availableCapacity;

                if (eventItem.SeatingType is Models.Seating.SectionReservedSeating &&
                    !string.IsNullOrEmpty(request.SectionIdentifier))
                {
                    var sectionBookings = await _bookingRepository.GetBookingCountForEventSectionAsync(
                        request.EventId,
                        request.SectionIdentifier);

                    var sectionReservedSeating = (Models.Seating.SectionReservedSeating)eventItem.SeatingType;
                    var sectionCapacity = sectionReservedSeating.GetSectionCapacity(request.SectionIdentifier);
                    sectionAvailableCapacity = sectionCapacity - sectionBookings;
                }

                bool canAccommodate = eventItem.SeatingType.CanAccommodateBooking(
                    request.NumberOfSeats,
                    sectionAvailableCapacity,
                    request.SectionIdentifier);

                if (!canAccommodate)
                {
                    if (!string.IsNullOrEmpty(request.SectionIdentifier))
                    {
                        return BookingResult.FailureResult(
                            $"Insufficient capacity in section '{request.SectionIdentifier}'. " +
                            $"Requested: {request.NumberOfSeats}, Available: {sectionAvailableCapacity}");
                    }

                    return BookingResult.FailureResult(
                        $"Insufficient capacity. Requested: {request.NumberOfSeats}, Available: {availableCapacity}");
                }
            }
            else
            {
                if (request.NumberOfSeats > availableCapacity)
                {
                    return BookingResult.FailureResult(
                        $"Insufficient capacity. Requested: {request.NumberOfSeats}, Available: {availableCapacity}");
                }
            }

            PaymentResponse paymentResponse;
            try
            {
                var paymentRequest = new PaymentRequest(request.CreditCardNumber);
                paymentResponse = await _paymentGateway.ProcessPaymentAsync(paymentRequest);
            }
            catch (Exception ex)
            {
                return BookingResult.FailureResult($"Payment processing failed: {ex.Message}");
            }

            if (!paymentResponse.IsValid)
            {
                return BookingResult.FailureResult("Payment was rejected. Please check your payment information");
            }

            var booking = new Models.Booking
            {
                UserId = request.UserId,
                EventId = request.EventId,
                VenueId = eventItem.VenueId,
                NumberOfSeats = request.NumberOfSeats,
                SectionIdentifier = request.SectionIdentifier,
                TotalAmount = request.TotalAmount,
                PaymentStatus = PaymentStatus.Paid,
                PaymentId = paymentResponse.PaymentId,
                BookingDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            var bookingId = await _bookingRepository.AddAsync(booking);

            return BookingResult.SuccessResult(bookingId, paymentResponse.PaymentId);
        }
    }
}
