using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Artemis.Core.JsonConverters
{
    /// <summary>
    /// An int converter that, if required, will round float values
    /// </summary>
    internal class ForgivingIntConverter : JsonConverter<int>
    {
        public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                throw new JsonException("Cannot convert null value.");

            if (reader.TokenType == JsonTokenType.Number)
            {
                if (reader.TryGetInt32(out int intValue))
                    return intValue;

                if (reader.TryGetDouble(out double doubleValue))
                    return (int)Math.Round(doubleValue);
            }

            throw new JsonException("Failed to deserialize forgiving int value");
        }

        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}