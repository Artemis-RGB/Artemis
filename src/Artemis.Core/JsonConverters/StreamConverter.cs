using System;
using System.IO;
using Newtonsoft.Json;

namespace Artemis.Core.JsonConverters
{
    /// <inheritdoc />
    public class StreamConverter : JsonConverter<Stream>
    {
        #region Overrides of JsonConverter<Stream>

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, Stream? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            using MemoryStream memoryStream = new();
            value.Position = 0;
            value.CopyTo(memoryStream);
            writer.WriteValue(memoryStream.ToArray());
        }

        /// <inheritdoc />
        public override Stream? ReadJson(JsonReader reader, Type objectType, Stream? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value is not string base64)
                return null;

            if (existingValue == null || !hasExistingValue || !existingValue.CanRead)
                return new MemoryStream(Convert.FromBase64String(base64));

            using MemoryStream memoryStream = new(Convert.FromBase64String(base64));
            existingValue.Position = 0;
            memoryStream.CopyTo(existingValue);
            existingValue.Position = 0;
            return existingValue;
        }

        #endregion
    }
}