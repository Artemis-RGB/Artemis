using System.Linq;
using Artemis.Core;
using SkiaSharp;

namespace Artemis.UI.Shared.Extensions;

/// <summary>
///     Provides utilities when working with layers in UI elements.
/// </summary>
public static class LayerExtensions
{
    /// <summary>
    ///     Returns the layer's bounds in real coordinates.
    /// </summary>
    public static SKRect GetLayerBounds(this Layer layer)
    {
        if (!layer.Leds.Any())
            return SKRect.Empty;

        return new SKRect(
            layer.Leds.Min(l => l.RgbLed.AbsoluteBoundary.Location.X),
            layer.Leds.Min(l => l.RgbLed.AbsoluteBoundary.Location.Y),
            layer.Leds.Max(l => l.RgbLed.AbsoluteBoundary.Location.X + l.RgbLed.AbsoluteBoundary.Size.Width),
            layer.Leds.Max(l => l.RgbLed.AbsoluteBoundary.Location.Y + l.RgbLed.AbsoluteBoundary.Size.Height)
        );
    }

    /// <summary>
    ///     Returns the layer's anchor in real coordinates.
    /// </summary>
    public static SKPoint GetLayerAnchorPosition(this Layer layer, SKPoint? positionOverride = null)
    {
        SKRect layerBounds = GetLayerBounds(layer);
        SKPoint positionProperty = layer.Transform.Position.CurrentValue;
        if (positionOverride != null)
            positionProperty = positionOverride.Value;

        // Start at the top left of the shape
        SKPoint position = new(layerBounds.Left, layerBounds.Top);

        // Apply translation
        position.X += positionProperty.X * layerBounds.Width;
        position.Y += positionProperty.Y * layerBounds.Height;

        return position;
    }

    /// <summary>
    ///     Returns an absolute and scaled rectangular path for the given layer in real coordinates.
    /// </summary>
    public static SKPath GetLayerPath(this Layer layer, bool includeTranslation, bool includeScale, bool includeRotation)
    {
        SKRect layerBounds = GetLayerBounds(layer);

        SKMatrix transform = layer.GetTransformMatrix(false, includeTranslation, includeScale, includeRotation, layerBounds);
        SKPath path = new();
        path.AddRect(layerBounds);
        path.Transform(transform);
        return path;
    }

    /// <summary>
    ///     Returns a new point normalized to 0.0-1.0
    /// </summary>
    public static SKPoint GetNormalizedPoint(this Layer layer, SKPoint point, bool absolute)
    {
        SKRect bounds = GetLayerBounds(layer);
        if (absolute)
            return new SKPoint(
                100 / bounds.Width * (point.X - bounds.Left) / 100,
                100 / bounds.Height * (point.Y - bounds.Top) / 100
            );

        return new SKPoint(
            100 / bounds.Width * point.X / 100,
            100 / bounds.Height * point.Y / 100
        );
    }

    /// <summary>
    ///     Returns the offset from the given point to the closest sides of the layer's shape bounds
    /// </summary>
    public static SKPoint GetDragOffset(this Layer layer, SKPoint dragStart)
    {
        SKRect bounds = layer.GetLayerPath(true, true, false).Bounds;
        SKPoint anchor = layer.GetLayerAnchorPosition();

        float xOffset = 0f, yOffset = 0f;

        // X offset
        if (dragStart.X < anchor.X)
            xOffset = bounds.Left - dragStart.X;
        else if (dragStart.X > anchor.X)
            xOffset = bounds.Right - dragStart.X;

        // Y offset
        if (dragStart.Y < anchor.Y)
            yOffset = bounds.Top - dragStart.Y;
        else if (dragStart.Y > anchor.Y)
            yOffset = bounds.Bottom - dragStart.Y;

        return new SKPoint(xOffset, yOffset);
    }
}