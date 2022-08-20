using System;
using Newtonsoft.Json;

namespace Artemis.Core.JsonConverters
{
    internal class NumericJsonConverter : JsonConverter<Numeric>
    {
        #region Overrides of JsonConverter<Numeric>

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, Numeric value, JsonSerializer serializer)
        {
            float floatValue = value;
            writer.WriteValue(floatValue);
        }

        /// <inheritdoc />
        public override Numeric ReadJson(JsonReader reader, Type objectType, Numeric existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return new Numeric(reader.Value);
        }

        #endregion
    }
}