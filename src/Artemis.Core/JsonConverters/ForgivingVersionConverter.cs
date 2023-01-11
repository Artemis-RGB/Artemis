using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace Artemis.Core.JsonConverters
{
    /// <summary>
    /// Version converter that is forgiving of missing parts of the version string,
    /// setting them to zero instead of -1.
    /// </summary>
    internal class ForgivingVersionConverter : VersionConverter
    {
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            object? obj = base.ReadJson(reader, objectType, existingValue, serializer);
            if (obj is not Version v)
                return obj;

            int major = v.Major == -1 ? 0 : v.Major;
            int minor = v.Minor == -1 ? 0 : v.Minor;
            int build = v.Build == -1 ? 0 : v.Build;
            int revision = v.Revision == -1 ? 0 : v.Revision;
            return new Version(major, minor, build, revision);
        }
    }
}