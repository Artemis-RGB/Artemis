using System.Text;
using System.Threading.Tasks;
using Artemis.Core;
using Avalonia.Input.Platform;

namespace Artemis.UI.Shared.Extensions;

/// <summary>
///     Provides extension methods for Avalonia's <see cref="IClipboard" /> type.
/// </summary>
public static class ClipboardExtensions
{
    /// <summary>
    ///     Retrieves clipboard JSON data representing <typeparamref name="T" /> and deserializes it into an instance of
    ///     <typeparamref name="T" />.
    /// </summary>
    /// <param name="clipboard">The clipboard to retrieve the data off.</param>
    /// <param name="format">The data format to retrieve data for.</param>
    /// <typeparam name="T">The type of data to retrieve</typeparam>
    /// <returns>
    ///     The resulting value or if the clipboard did not contain data for the provided <paramref name="format" />;
    ///     <see langword="null" />.
    /// </returns>
    public static async Task<T?> GetJsonAsync<T>(this IClipboard clipboard, string format)
    {
        byte[]? bytes = (byte[]?) await clipboard.GetDataAsync(format);
        return bytes == null ? default : CoreJson.Deserialize<T>(Encoding.Unicode.GetString(bytes).TrimEnd('\0'));
    }
}