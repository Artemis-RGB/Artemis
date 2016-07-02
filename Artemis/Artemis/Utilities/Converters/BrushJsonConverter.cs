using System;
using System.Windows.Markup;
using System.Windows.Media;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Artemis.Utilities.Converters
{
    /// <summary>
    ///     Stores a brush as XAML because Json.NET has trouble saving it as JSON
    /// </summary>
    public class BrushJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var jo = new JObject {{"value", XamlWriter.Save(value)}};
            jo.WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            // Load JObject from stream
            var jObject = JObject.Load(reader);
            return XamlReader.Parse(jObject["value"].ToString());
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(Brush).IsAssignableFrom(objectType);
        }
    }
}