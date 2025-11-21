using BookingSystem.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace BookingSystem.DataAccess.Sql
{
    public class SqlVenueRepository : IVenueRepository
    {
        private readonly string _connectionString;

        public SqlVenueRepository(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task<Venue> GetByIdAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(
                    "SELECT Id, Name, Location, TotalCapacity, CreatedAt FROM Venues WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return MapVenue(reader);
                        }
                    }
                }
            }
            return null;
        }

        public async Task<IEnumerable<Venue>> GetAllAsync()
        {
            var venues = new List<Venue>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(
                    "SELECT Id, Name, Location, TotalCapacity, CreatedAt FROM Venues", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            venues.Add(MapVenue(reader));
                        }
                    }
                }
            }
            return venues;
        }

        public async Task<int> AddAsync(Venue venue)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(
                    "INSERT INTO Venues (Name, Location, TotalCapacity, CreatedAt) " +
                    "OUTPUT INSERTED.Id " +
                    "VALUES (@Name, @Location, @TotalCapacity, @CreatedAt)", connection))
                {
                    command.Parameters.AddWithValue("@Name", venue.Name);
                    command.Parameters.AddWithValue("@Location", venue.Location);
                    command.Parameters.AddWithValue("@TotalCapacity", venue.TotalCapacity);
                    command.Parameters.AddWithValue("@CreatedAt", venue.CreatedAt);

                    var id = (int)await command.ExecuteScalarAsync();
                    return id;
                }
            }
        }

        public async Task UpdateAsync(Venue venue)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(
                    "UPDATE Venues SET Name = @Name, Location = @Location, TotalCapacity = @TotalCapacity " +
                    "WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", venue.Id);
                    command.Parameters.AddWithValue("@Name", venue.Name);
                    command.Parameters.AddWithValue("@Location", venue.Location);
                    command.Parameters.AddWithValue("@TotalCapacity", venue.TotalCapacity);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task DeleteAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("DELETE FROM Venues WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        private Venue MapVenue(SqlDataReader reader)
        {
            return new Venue
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Location = reader.GetString(reader.GetOrdinal("Location")),
                TotalCapacity = reader.GetInt32(reader.GetOrdinal("TotalCapacity")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            };
        }
    }
}
