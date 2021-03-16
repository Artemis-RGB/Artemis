using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Artemis.Core.JsonConverters
{
    /// <summary>
    ///     An int converter that, if required, will round float values
    /// </summary>
    internal class ForgivingIntConverter : JsonConverter<int>
    {
        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, int value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override int ReadJson(JsonReader reader, Type objectType, int existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JValue? jsonValue = serializer.Deserialize<JValue>(reader);
            if (jsonValue == null)
                throw new JsonReaderException("Failed to deserialize forgiving int value");

            if (jsonValue.Type == JTokenType.Float)
                return (int) Math.Round(jsonValue.Value<double>());
            if (jsonValue.Type == JTokenType.Integer) 
                return jsonValue.Value<int>();

            throw new JsonReaderException("Failed to deserialize forgiving int value");
        }
    }
}