using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Artemis.Core;

/// <summary>
///     A static helper class that serializes and deserializes JSON with the Artemis Core JSON settings
/// </summary>
public static class CoreJson
{
    /// <summary>
    ///     Serializes the specified object to a JSON string.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <returns>A JSON string representation of the object.</returns>
    [DebuggerStepThrough]
    public static string Serialize(object? value)
    {
        return JsonSerializer.Serialize(value, Constants.JsonConvertSettings);
    }
    
    /// <summary>
    ///     Deserializes the JSON to the specified .NET type.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
    /// <param name="value">The JSON to deserialize.</param>
    /// <returns>The deserialized object from the JSON string.</returns>
    [DebuggerStepThrough]
    [return: MaybeNull]
    public static T Deserialize<T>(string value)
    {
        return JsonSerializer.Deserialize<T>(value, Constants.JsonConvertSettings);
    }
    
    /// <summary>
    /// Gets a copy of the JSON serializer options used by Artemis Core
    /// </summary>
    /// <returns>A copy of the JSON serializer options used by Artemis Core</returns>
    public static JsonSerializerOptions GetJsonSerializerOptions()
    {
        return new JsonSerializerOptions(Constants.JsonConvertSettings);
    }
}