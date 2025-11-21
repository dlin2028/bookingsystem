namespace BookingSystem.Services.Booking
{
    /// <summary>
    /// Represents the result of a booking operation
    /// </summary>
    public class BookingResult
    {
        /// <summary>
        /// Indicates whether the booking operation was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Descriptive message about the booking operation result
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The unique identifier of the booking if successful
        /// </summary>
        public int? BookingId { get; set; }

        /// <summary>
        /// The payment transaction identifier associated with the booking
        /// </summary>
        public string PaymentId { get; set; }

        /// <summary>
        /// Initializes a new instance of the BookingResult class
        /// </summary>
        /// <param name="success">Indicates if the operation was successful</param>
        /// <param name="message">Descriptive message about the result</param>
        /// <param name="bookingId">Optional booking identifier</param>
        /// <param name="paymentId">Optional payment identifier</param>
        public BookingResult(bool success, string message, int? bookingId = null, string paymentId = null)
        {
            Success = success;
            Message = message;
            BookingId = bookingId;
            PaymentId = paymentId;
        }

        /// <summary>
        /// Creates a successful booking result
        /// </summary>
        /// <param name="bookingId">The booking identifier</param>
        /// <param name="paymentId">The payment identifier</param>
        /// <param name="message">Optional success message</param>
        /// <returns>A BookingResult indicating success</returns>
        public static BookingResult SuccessResult(int bookingId, string paymentId, string message = "Booking created successfully")
        {
            return new BookingResult(true, message, bookingId, paymentId);
        }

        /// <summary>
        /// Creates a failed booking result
        /// </summary>
        /// <param name="message">Error or failure message</param>
        /// <returns>A BookingResult indicating failure</returns>
        public static BookingResult FailureResult(string message)
        {
            return new BookingResult(false, message);
        }
    }
}
