using System.Collections.Generic;
using Artemis.Core.SkiaSharp;

namespace Artemis.Core.Services;

/// <summary>
/// Represents a class that can provide one or more graphics <see cref="IManagedGraphicsContext"/> instances by name.
/// </summary>
public interface IGraphicsContextProvider
{
    /// <summary>
    /// Gets a read only collection containing the names of all the graphics contexts supported by this provider.
    /// </summary>
    IReadOnlyCollection<string> GraphicsContextNames { get; }

    /// <summary>
    /// Gets a managed graphics context by name.
    /// </summary>
    /// <param name="name">The name of the graphics context.</param>
    /// <returns>If found, an instance of the managed graphics context with the given <paramref name="name"/>; otherwise <see langword="null"/>.</returns>
    IManagedGraphicsContext? GetGraphicsContext(string name);
}