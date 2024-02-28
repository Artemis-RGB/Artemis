using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using SkiaSharp;

namespace Artemis.Core.JsonConverters
{
    internal class SKColorConverter : JsonConverter<SKColor>
    {
        public override void Write(Utf8JsonWriter writer, SKColor value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }

        public override SKColor Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException($"Expected a string token, but got {reader.TokenType}.");
            }

            string colorString = reader.GetString() ?? string.Empty;
            return SKColor.TryParse(colorString, out SKColor color) ? color : SKColor.Empty;
        }
    }
}