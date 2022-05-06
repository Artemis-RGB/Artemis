using System.Collections.Generic;
using Artemis.Core.SkiaSharp;

namespace Artemis.Core.Services;

public interface IGraphicsContextProvider
{
    IReadOnlyCollection<string> GraphicsContextNames { get; }
    IManagedGraphicsContext? GetGraphicsContext(string name);
}