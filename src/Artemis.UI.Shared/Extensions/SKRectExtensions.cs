using Avalonia;
using SkiaSharp;

namespace Artemis.UI.Shared.Extensions;

public static class SKRectExtensions
{
    public static Rect ToRect(this SKRect rect)
    {
        return new Rect(rect.Left, rect.Top, rect.Width, rect.Height);
    }

    public static Rect ToRect(this SKRectI rect)
    {
        return new Rect(rect.Left, rect.Top, rect.Width, rect.Height);
    }
}