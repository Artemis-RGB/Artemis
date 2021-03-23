using System;
using SkiaSharp;

namespace Artemis.Core.SkiaSharp
{
    /// <summary>
    ///     Represents a managed wrapper around a SkiaSharp context
    /// </summary>
    public interface IManagedGraphicsContext : IDisposable
    {
        /// <summary>
        ///     Gets the graphics context created by this wrapper
        /// </summary>
        GRContext GraphicsContext { get; }
    }
}