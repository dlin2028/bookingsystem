using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BookingSystem.Models.Seating
{
    public class SeatingTypeJsonConverter : JsonConverter<ISeatingType>
    {
        public override ISeatingType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
            {
                var root = doc.RootElement;

                if (!root.TryGetProperty("SeatingTypeName", out var typeNameProperty))
                {
                    throw new JsonException("Missing SeatingTypeName property");
                }

                var typeName = typeNameProperty.GetString();

                return typeName switch
                {
                    "Open Seating" => JsonSerializer.Deserialize<OpenSeating>(root.GetRawText(), options),
                    "Full Reserved Seating" => JsonSerializer.Deserialize<FullReservedSeating>(root.GetRawText(), options),
                    "Section Reserved Seating" => JsonSerializer.Deserialize<SectionReservedSeating>(root.GetRawText(), options),
                    _ => throw new JsonException($"Unknown seating type: {typeName}")
                };
            }
        }

        public override void Write(Utf8JsonWriter writer, ISeatingType value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
        }
    }
}
