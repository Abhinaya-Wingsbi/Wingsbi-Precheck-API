using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Godrej.Precheck.Models.Json
{
    /// <summary>
    /// System.Text.Json's built-in DateTime? converter rejects an empty string, but callers that treat
    /// "no filter" as "" (rather than omitting the field, or null) need that to bind to null instead of
    /// a 400. Every other value is parsed exactly like the default converter.
    /// </summary>
    public class NullableDateTimeConverter : JsonConverter<DateTime?>
    {
        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                var value = reader.GetString();
                if (string.IsNullOrWhiteSpace(value))
                {
                    return null;
                }
                return DateTime.Parse(value);
            }

            return reader.GetDateTime();
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteStringValue(value.Value);
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}
