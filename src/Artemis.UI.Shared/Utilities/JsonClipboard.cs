using System.Diagnostics.CodeAnalysis;
using System.Windows;
using Newtonsoft.Json;

namespace Artemis.UI.Shared
{
    /// <summary>
    ///     Provides access to the clipboard via JSON-serialized objects
    /// </summary>
    public static class JsonClipboard
    {
        private static readonly JsonSerializerSettings JsonSettings = new() {TypeNameHandling = TypeNameHandling.All};

        /// <summary>
        ///     Sets the provided object on the clipboard
        /// </summary>
        /// <param name="clipboardObject">The object to set on the clipboard</param>
        public static void SetObject(object clipboardObject)
        {
            string json = JsonConvert.SerializeObject(clipboardObject, JsonSettings);
            Clipboard.SetData("Artemis", json);
        }

        /// <summary>
        ///     If set, gets the current object off of the clipboard
        /// </summary>
        /// <returns>The object that is on the clipboard, if none is set <see langword="null" />.</returns>
        public static object? GetData()
        {
            string? json = Clipboard.GetData("Artemis")?.ToString();
            return json != null ? JsonConvert.DeserializeObject(json, JsonSettings) : null;
        }

        /// <summary>
        ///     If set, gets the current object of type <typeparamref name="T" /> off of the clipboard
        /// </summary>
        /// <typeparam name="T">The type of object to get</typeparam>
        /// <returns>
        ///     The object that is on the clipboard. If none is set or not of type <typeparamref name="T" />,
        ///     <see langword="null" />.
        /// </returns>
        [return: MaybeNull]
        public static T GetData<T>()
        {
            object? data = GetData();
            return data is T castData ? castData : default;
        }

        /// <summary>
        ///     Determines whether the clipboard currently contains Artemis data
        /// </summary>
        /// <returns><see langword="true" /> if the clipboard contains Artemis data, otherwise <see langword="false" /></returns>
        public static bool ContainsArtemisData()
        {
            return Clipboard.ContainsData("Artemis");
        }
    }
}