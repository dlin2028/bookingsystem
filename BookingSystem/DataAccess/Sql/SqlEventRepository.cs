using BookingSystem.Models;
using BookingSystem.Models.Seating;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text.Json;
using System.Threading.Tasks;

namespace BookingSystem.DataAccess.Sql
{
    public class SqlEventRepository : IEventRepository
    {
        private readonly string _connectionString;

        public SqlEventRepository(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task<Event> GetByIdAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(
                    "SELECT Id, Name, Description, VenueId, EventDate, EventType, SeatingTypeName, SeatingConfiguration, CreatedAt " +
                    "FROM Events WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return MapEvent(reader);
                        }
                    }
                }
            }
            return null;
        }

        public async Task<IEnumerable<Event>> GetAllAsync()
        {
            var events = new List<Event>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(
                    "SELECT Id, Name, Description, VenueId, EventDate, EventType, SeatingTypeName, SeatingConfiguration, CreatedAt " +
                    "FROM Events", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            events.Add(MapEvent(reader));
                        }
                    }
                }
            }
            return events;
        }

        public async Task<IEnumerable<Event>> GetFutureEventsAsync()
        {
            var events = new List<Event>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(
                    "SELECT Id, Name, Description, VenueId, EventDate, EventType, SeatingTypeName, SeatingConfiguration, CreatedAt " +
                    "FROM Events WHERE EventDate > GETUTCDATE() ORDER BY EventDate", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            events.Add(MapEvent(reader));
                        }
                    }
                }
            }
            return events;
        }

        public async Task<int> AddAsync(Event eventItem)
        {
            var (seatingTypeName, seatingConfiguration) = SerializeSeatingType(eventItem.SeatingType);

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(
                    "INSERT INTO Events (Name, Description, VenueId, EventDate, EventType, SeatingTypeName, SeatingConfiguration, CreatedAt) " +
                    "OUTPUT INSERTED.Id " +
                    "VALUES (@Name, @Description, @VenueId, @EventDate, @EventType, @SeatingTypeName, @SeatingConfiguration, @CreatedAt)", connection))
                {
                    command.Parameters.AddWithValue("@Name", eventItem.Name);
                    command.Parameters.AddWithValue("@Description", (object)eventItem.Description ?? DBNull.Value);
                    command.Parameters.AddWithValue("@VenueId", eventItem.VenueId);
                    command.Parameters.AddWithValue("@EventDate", eventItem.EventDate);
                    command.Parameters.AddWithValue("@EventType", eventItem.EventType);
                    command.Parameters.AddWithValue("@SeatingTypeName", seatingTypeName);
                    command.Parameters.AddWithValue("@SeatingConfiguration", (object)seatingConfiguration ?? DBNull.Value);
                    command.Parameters.AddWithValue("@CreatedAt", eventItem.CreatedAt);

                    var id = (int)await command.ExecuteScalarAsync();
                    return id;
                }
            }
        }

        public async Task UpdateAsync(Event eventItem)
        {
            var (seatingTypeName, seatingConfiguration) = SerializeSeatingType(eventItem.SeatingType);

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(
                    "UPDATE Events SET Name = @Name, Description = @Description, VenueId = @VenueId, " +
                    "EventDate = @EventDate, EventType = @EventType, SeatingTypeName = @SeatingTypeName, " +
                    "SeatingConfiguration = @SeatingConfiguration WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", eventItem.Id);
                    command.Parameters.AddWithValue("@Name", eventItem.Name);
                    command.Parameters.AddWithValue("@Description", (object)eventItem.Description ?? DBNull.Value);
                    command.Parameters.AddWithValue("@VenueId", eventItem.VenueId);
                    command.Parameters.AddWithValue("@EventDate", eventItem.EventDate);
                    command.Parameters.AddWithValue("@EventType", eventItem.EventType);
                    command.Parameters.AddWithValue("@SeatingTypeName", seatingTypeName);
                    command.Parameters.AddWithValue("@SeatingConfiguration", (object)seatingConfiguration ?? DBNull.Value);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task DeleteAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("DELETE FROM Events WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        private Event MapEvent(SqlDataReader reader)
        {
            var seatingTypeName = reader.GetString(reader.GetOrdinal("SeatingTypeName"));
            var seatingConfigJson = reader.IsDBNull(reader.GetOrdinal("SeatingConfiguration"))
                ? null
                : reader.GetString(reader.GetOrdinal("SeatingConfiguration"));

            var seatingType = DeserializeSeatingType(seatingTypeName, seatingConfigJson);

            return new Event
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                VenueId = reader.GetInt32(reader.GetOrdinal("VenueId")),
                EventDate = reader.GetDateTime(reader.GetOrdinal("EventDate")),
                EventType = reader.GetString(reader.GetOrdinal("EventType")),
                SeatingType = seatingType,
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            };
        }

        private (string typeName, string configuration) SerializeSeatingType(ISeatingType seatingType)
        {
            if (seatingType == null)
                return ("Open", null);

            if (seatingType is FullReservedSeating fullReserved)
            {
                var config = JsonSerializer.Serialize(new { TotalSeats = fullReserved.TotalSeats });
                return ("FullReserved", config);
            }
            else if (seatingType is SectionReservedSeating sectionReserved)
            {
                var config = JsonSerializer.Serialize(new { Sections = sectionReserved.Sections });
                return ("SectionReserved", config);
            }
            else if (seatingType is OpenSeating)
            {
                return ("Open", null);
            }

            return ("Open", null);
        }

        private ISeatingType DeserializeSeatingType(string typeName, string configuration)
        {
            switch (typeName)
            {
                case "FullReserved":
                    if (!string.IsNullOrEmpty(configuration))
                    {
                        var config = JsonSerializer.Deserialize<Dictionary<string, int>>(configuration);
                        var totalSeats = config.ContainsKey("TotalSeats") ? config["TotalSeats"] : 0;
                        return new FullReservedSeating(totalSeats);
                    }
                    return new FullReservedSeating(0);

                case "SectionReserved":
                    if (!string.IsNullOrEmpty(configuration))
                    {
                        var config = JsonSerializer.Deserialize<Dictionary<string, object>>(configuration);
                        if (config.ContainsKey("Sections"))
                        {
                            var sectionsJson = JsonSerializer.Serialize(config["Sections"]);
                            var sections = JsonSerializer.Deserialize<Dictionary<string, int>>(sectionsJson);
                            return new SectionReservedSeating(sections);
                        }
                    }
                    return new SectionReservedSeating(new Dictionary<string, int>());

                case "Open":
                default:
                    return new OpenSeating();
            }
        }
    }
}
