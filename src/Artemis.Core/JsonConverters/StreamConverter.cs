using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Artemis.Core.JsonConverters
{
    internal class StreamConverter : JsonConverter<Stream>
    {
        public override void Write(Utf8JsonWriter writer, Stream value, JsonSerializerOptions options)
        {
            using MemoryStream memoryStream = new();
            value.Position = 0;
            value.CopyTo(memoryStream);
            writer.WriteBase64StringValue(memoryStream.ToArray());
        }

        public override Stream Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
                throw new JsonException($"Expected a string token, but got {reader.TokenType}.");

            string base64 = reader.GetString() ?? string.Empty;

            if (typeToConvert == typeof(MemoryStream))
                return new MemoryStream(Convert.FromBase64String(base64));

            throw new InvalidOperationException("StreamConverter only supports reading to MemoryStream");
        }
    }
}