using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Artemis.Core.JsonConverters
{
    internal class NumericJsonConverter : JsonConverter<Numeric>
    {
        public override void Write(Utf8JsonWriter writer, Numeric value, JsonSerializerOptions options)
        {
            float floatValue = value;
            writer.WriteNumberValue(floatValue);
        }

        public override Numeric Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.Number)
            {
                throw new JsonException($"Expected a number token, but got {reader.TokenType}.");
            }

            float floatValue = reader.GetSingle();
            return new Numeric(floatValue);
        }
    }
}