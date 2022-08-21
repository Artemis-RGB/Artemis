using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

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
    /// <param name="handleTypeNames">If set to true sets TypeNameHandling to <see cref="TypeNameHandling.All" /></param>
    /// <returns>A JSON string representation of the object.</returns>
    [DebuggerStepThrough]
    public static string SerializeObject(object? value, bool handleTypeNames = false)
    {
        return JsonConvert.SerializeObject(value, handleTypeNames ? Constants.JsonConvertTypedSettings : Constants.JsonConvertSettings);
    }

    #endregion

    #region Deserialize

    /// <summary>
    ///     Deserializes the JSON to a .NET object.
    /// </summary>
    /// <param name="value">The JSON to deserialize.</param>
    /// <param name="handleTypeNames">If set to true sets TypeNameHandling to <see cref="TypeNameHandling.All" /></param>
    /// <returns>The deserialized object from the JSON string.</returns>
    [DebuggerStepThrough]
    public static object? DeserializeObject(string value, bool handleTypeNames = false)
    {
        return JsonConvert.DeserializeObject(value, handleTypeNames ? Constants.JsonConvertTypedSettings : Constants.JsonConvertSettings);
    }

    /// <summary>
    ///     Deserializes the JSON to the specified .NET type.
    /// </summary>
    /// <param name="value">The JSON to deserialize.</param>
    /// <param name="type">The <see cref="Type" /> of object being deserialized.</param>
    /// <param name="handleTypeNames">If set to true sets TypeNameHandling to <see cref="TypeNameHandling.All" /></param>
    /// <returns>The deserialized object from the JSON string.</returns>
    [DebuggerStepThrough]
    public static object? DeserializeObject(string value, Type type, bool handleTypeNames = false)
    {
        return JsonConvert.DeserializeObject(value, type, handleTypeNames ? Constants.JsonConvertTypedSettings : Constants.JsonConvertSettings);
    }

    /// <summary>
    ///     Deserializes the JSON to the specified .NET type.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
    /// <param name="value">The JSON to deserialize.</param>
    /// <param name="handleTypeNames">If set to true sets TypeNameHandling to <see cref="TypeNameHandling.All" /></param>
    /// <returns>The deserialized object from the JSON string.</returns>
    [DebuggerStepThrough]
    [return: MaybeNull]
    public static T DeserializeObject<T>(string value, bool handleTypeNames = false)
    {
        return JsonConvert.DeserializeObject<T>(value, handleTypeNames ? Constants.JsonConvertTypedSettings : Constants.JsonConvertSettings);
    }

    #endregion
}