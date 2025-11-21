using BookingSystem.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace BookingSystem.DataAccess.Sql
{
    public class SqlBookingRepository : IBookingRepository
    {
        private readonly string _connectionString;

        public SqlBookingRepository(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task<Booking> GetByIdAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(
                    "SELECT Id, UserId, EventId, VenueId, NumberOfSeats, SectionIdentifier, PaymentStatus, " +
                    "PaymentId, TotalAmount, BookingDate, CreatedAt FROM Bookings WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return MapBooking(reader);
                        }
                    }
                }
            }
            return null;
        }

        public async Task<IEnumerable<Booking>> GetAllAsync()
        {
            var bookings = new List<Booking>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(
                    "SELECT Id, UserId, EventId, VenueId, NumberOfSeats, SectionIdentifier, PaymentStatus, " +
                    "PaymentId, TotalAmount, BookingDate, CreatedAt FROM Bookings", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            bookings.Add(MapBooking(reader));
                        }
                    }
                }
            }
            return bookings;
        }

        public async Task<IEnumerable<Booking>> GetByUserIdAsync(int userId)
        {
            var bookings = new List<Booking>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(
                    "SELECT Id, UserId, EventId, VenueId, NumberOfSeats, SectionIdentifier, PaymentStatus, " +
                    "PaymentId, TotalAmount, BookingDate, CreatedAt FROM Bookings WHERE UserId = @UserId", connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            bookings.Add(MapBooking(reader));
                        }
                    }
                }
            }
            return bookings;
        }

        public async Task<IEnumerable<Booking>> GetByVenueIdAsync(int venueId)
        {
            var bookings = new List<Booking>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(
                    "SELECT Id, UserId, EventId, VenueId, NumberOfSeats, SectionIdentifier, PaymentStatus, " +
                    "PaymentId, TotalAmount, BookingDate, CreatedAt FROM Bookings WHERE VenueId = @VenueId", connection))
                {
                    command.Parameters.AddWithValue("@VenueId", venueId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            bookings.Add(MapBooking(reader));
                        }
                    }
                }
            }
            return bookings;
        }

        public async Task<IEnumerable<Booking>> GetByEventIdAsync(int eventId)
        {
            var bookings = new List<Booking>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(
                    "SELECT Id, UserId, EventId, VenueId, NumberOfSeats, SectionIdentifier, PaymentStatus, " +
                    "PaymentId, TotalAmount, BookingDate, CreatedAt FROM Bookings WHERE EventId = @EventId", connection))
                {
                    command.Parameters.AddWithValue("@EventId", eventId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            bookings.Add(MapBooking(reader));
                        }
                    }
                }
            }
            return bookings;
        }

        public async Task<int> AddAsync(Booking booking)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(
                    "INSERT INTO Bookings (UserId, EventId, VenueId, NumberOfSeats, SectionIdentifier, " +
                    "PaymentStatus, PaymentId, TotalAmount, BookingDate, CreatedAt) " +
                    "OUTPUT INSERTED.Id " +
                    "VALUES (@UserId, @EventId, @VenueId, @NumberOfSeats, @SectionIdentifier, " +
                    "@PaymentStatus, @PaymentId, @TotalAmount, @BookingDate, @CreatedAt)", connection))
                {
                    command.Parameters.AddWithValue("@UserId", booking.UserId);
                    command.Parameters.AddWithValue("@EventId", booking.EventId);
                    command.Parameters.AddWithValue("@VenueId", booking.VenueId);
                    command.Parameters.AddWithValue("@NumberOfSeats", booking.NumberOfSeats);
                    command.Parameters.AddWithValue("@SectionIdentifier", (object)booking.SectionIdentifier ?? DBNull.Value);
                    command.Parameters.AddWithValue("@PaymentStatus", booking.PaymentStatus.ToString());
                    command.Parameters.AddWithValue("@PaymentId", (object)booking.PaymentId ?? DBNull.Value);
                    command.Parameters.AddWithValue("@TotalAmount", booking.TotalAmount);
                    command.Parameters.AddWithValue("@BookingDate", booking.BookingDate);
                    command.Parameters.AddWithValue("@CreatedAt", booking.CreatedAt);

                    var id = (int)await command.ExecuteScalarAsync();
                    return id;
                }
            }
        }

        public async Task UpdateAsync(Booking booking)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(
                    "UPDATE Bookings SET UserId = @UserId, EventId = @EventId, VenueId = @VenueId, " +
                    "NumberOfSeats = @NumberOfSeats, SectionIdentifier = @SectionIdentifier, " +
                    "PaymentStatus = @PaymentStatus, PaymentId = @PaymentId, TotalAmount = @TotalAmount, " +
                    "BookingDate = @BookingDate WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", booking.Id);
                    command.Parameters.AddWithValue("@UserId", booking.UserId);
                    command.Parameters.AddWithValue("@EventId", booking.EventId);
                    command.Parameters.AddWithValue("@VenueId", booking.VenueId);
                    command.Parameters.AddWithValue("@NumberOfSeats", booking.NumberOfSeats);
                    command.Parameters.AddWithValue("@SectionIdentifier", (object)booking.SectionIdentifier ?? DBNull.Value);
                    command.Parameters.AddWithValue("@PaymentStatus", booking.PaymentStatus.ToString());
                    command.Parameters.AddWithValue("@PaymentId", (object)booking.PaymentId ?? DBNull.Value);
                    command.Parameters.AddWithValue("@TotalAmount", booking.TotalAmount);
                    command.Parameters.AddWithValue("@BookingDate", booking.BookingDate);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task DeleteAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("DELETE FROM Bookings WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<int> GetBookingCountForEventAsync(int eventId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(
                    "SELECT COALESCE(SUM(NumberOfSeats), 0) FROM Bookings WHERE EventId = @EventId", connection))
                {
                    command.Parameters.AddWithValue("@EventId", eventId);
                    var count = (int)await command.ExecuteScalarAsync();
                    return count;
                }
            }
        }

        public async Task<int> GetBookingCountForEventSectionAsync(int eventId, string sectionIdentifier)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(
                    "SELECT COALESCE(SUM(NumberOfSeats), 0) FROM Bookings " +
                    "WHERE EventId = @EventId AND SectionIdentifier = @SectionIdentifier", connection))
                {
                    command.Parameters.AddWithValue("@EventId", eventId);
                    command.Parameters.AddWithValue("@SectionIdentifier", sectionIdentifier);
                    var count = (int)await command.ExecuteScalarAsync();
                    return count;
                }
            }
        }

        /// <summary>
        /// ADVANCED QUERY 1: Retrieves only bookings for users who have at least one successfully paid booking at the specified venue.
        /// SQL Query:
        /// SELECT b.*
        /// FROM Bookings b
        /// WHERE b.VenueId = @VenueId
        ///   AND b.UserId IN (
        ///     SELECT DISTINCT UserId
        ///     FROM Bookings
        ///     WHERE VenueId = @VenueId
        ///       AND PaymentStatus = 'Paid'
        ///   )
        /// </summary>
        public async Task<IEnumerable<Booking>> FindBookingsForPaidUsersAtVenueAsync(int venueId)
        {
            var bookings = new List<Booking>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query = @"
                    SELECT b.Id, b.UserId, b.EventId, b.VenueId, b.NumberOfSeats, b.SectionIdentifier,
                           b.PaymentStatus, b.PaymentId, b.TotalAmount, b.BookingDate, b.CreatedAt
                    FROM Bookings b
                    WHERE b.VenueId = @VenueId
                      AND b.UserId IN (
                        SELECT DISTINCT UserId
                        FROM Bookings
                        WHERE VenueId = @VenueId
                          AND PaymentStatus = 'Paid'
                      )";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@VenueId", venueId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            bookings.Add(MapBooking(reader));
                        }
                    }
                }
            }
            return bookings;
        }

        /// <summary>
        /// ADVANCED QUERY 2: Retrieves a list of all User IDs who have no bookings whatsoever at the specified venue.
        /// SQL Query:
        /// SELECT u.Id
        /// FROM Users u
        /// WHERE NOT EXISTS (
        ///   SELECT 1
        ///   FROM Bookings b
        ///   WHERE b.UserId = u.Id
        ///     AND b.VenueId = @VenueId
        /// )
        /// </summary>
        public async Task<IEnumerable<int>> FindUsersWithoutBookingsInVenueAsync(int venueId)
        {
            var userIds = new List<int>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query = @"
                    SELECT u.Id
                    FROM Users u
                    WHERE NOT EXISTS (
                      SELECT 1
                      FROM Bookings b
                      WHERE b.UserId = u.Id
                        AND b.VenueId = @VenueId
                    )";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@VenueId", venueId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            userIds.Add(reader.GetInt32(0));
                        }
                    }
                }
            }
            return userIds;
        }

        private Booking MapBooking(SqlDataReader reader)
        {
            return new Booking
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                EventId = reader.GetInt32(reader.GetOrdinal("EventId")),
                VenueId = reader.GetInt32(reader.GetOrdinal("VenueId")),
                NumberOfSeats = reader.GetInt32(reader.GetOrdinal("NumberOfSeats")),
                SectionIdentifier = reader.IsDBNull(reader.GetOrdinal("SectionIdentifier"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("SectionIdentifier")),
                PaymentStatus = Enum.Parse<PaymentStatus>(reader.GetString(reader.GetOrdinal("PaymentStatus"))),
                PaymentId = reader.IsDBNull(reader.GetOrdinal("PaymentId"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("PaymentId")),
                TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
                BookingDate = reader.GetDateTime(reader.GetOrdinal("BookingDate")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            };
        }
    }
}
