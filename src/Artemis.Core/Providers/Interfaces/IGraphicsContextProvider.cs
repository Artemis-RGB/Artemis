using Artemis.Core.SkiaSharp;

namespace Artemis.Core.Providers;

/// <summary>
///     Represents a class that can provide one or more graphics <see cref="IManagedGraphicsContext" /> instances by name.
/// </summary>
public interface IGraphicsContextProvider
{
    /// <summary>
    ///     Gets the name of the graphics context.
    /// </summary>
    string GraphicsContextName { get; }

    /// <summary>
    ///     Creates an instance of the managed graphics context this provider provides.
    /// </summary>
    /// <returns>
    ///     An instance of the resulting managed graphics context if successfully created; otherwise
    ///     <see langword="null" />.
    /// </returns>
    IManagedGraphicsContext? GetGraphicsContext();
}