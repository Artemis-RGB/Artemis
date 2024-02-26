using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Artemis.Core;

/// <summary>
///     A static helper class that serializes and deserializes JSON with the Artemis Core JSON settings
/// </summary>
public static class CoreJson
{
    #region Serialize

    /// <summary>
    ///     Serializes the specified object to a JSON string.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <returns>A JSON string representation of the object.</returns>
    [DebuggerStepThrough]
    public static string SerializeObject(object? value)
    {
        return JsonSerializer.Serialize(value, Constants.JsonConvertSettings);
    }

    #endregion

    #region Deserialize
    
    /// <summary>
    ///     Deserializes the JSON to the specified .NET type.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
    /// <param name="value">The JSON to deserialize.</param>
    /// <returns>The deserialized object from the JSON string.</returns>
    [DebuggerStepThrough]
    [return: MaybeNull]
    public static T DeserializeObject<T>(string value)
    {
        return JsonSerializer.Deserialize<T>(value, Constants.JsonConvertSettings);
    }

    #endregion
}