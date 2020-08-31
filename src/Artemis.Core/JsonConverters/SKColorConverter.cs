using System;
using Newtonsoft.Json;
using SkiaSharp;

namespace Artemis.Core.JsonConverters
{
    internal class SKColorConverter : JsonConverter<SKColor>
    {
        public override void WriteJson(JsonWriter writer, SKColor value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public override SKColor ReadJson(JsonReader reader, Type objectType, SKColor existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value is string value && !string.IsNullOrWhiteSpace(value))
                return SKColor.Parse(value);

            return SKColor.Empty;
        }
    }
}