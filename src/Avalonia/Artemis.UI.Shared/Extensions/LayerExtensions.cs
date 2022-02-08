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

        // Start at the center of the shape
        SKPoint position = new(layerBounds.MidX, layerBounds.MidY);

        // Apply translation
        position.X += positionProperty.X * layerBounds.Width;
        position.Y += positionProperty.Y * layerBounds.Height;

        return position;
    }

    /// <summary>
    ///     Returns an absolute and scaled rectangular path for the given layer in real coordinates.
    /// </summary>
    public static SKPath GetLayerPath(this Layer layer, bool includeTranslation, bool includeScale, bool includeRotation, SKPoint? anchorOverride = null)
    {
        SKRect layerBounds = GetLayerBounds(layer);

        // Apply transformation like done by the core during layer rendering (same differences apply as in GetLayerTransformGroup)
        SKPoint anchorPosition = GetLayerAnchorPosition(layer);
        if (anchorOverride != null)
            anchorPosition = anchorOverride.Value;

        SKPoint anchorProperty = layer.Transform.AnchorPoint.CurrentValue;

        // Translation originates from the unscaled center of the shape and is tied to the anchor
        float x = anchorPosition.X - layerBounds.MidX - anchorProperty.X * layerBounds.Width;
        float y = anchorPosition.Y - layerBounds.MidY - anchorProperty.Y * layerBounds.Height;

        SKPath path = new();
        path.AddRect(layerBounds);
        if (includeTranslation)
            path.Transform(SKMatrix.CreateTranslation(x, y));
        if (includeScale)
            path.Transform(SKMatrix.CreateScale(layer.Transform.Scale.CurrentValue.Width / 100f, layer.Transform.Scale.CurrentValue.Height / 100f, anchorPosition.X, anchorPosition.Y));
        if (includeRotation)
            path.Transform(SKMatrix.CreateRotationDegrees(layer.Transform.Rotation.CurrentValue, anchorPosition.X, anchorPosition.Y));

        return path;
    }

    /// <summary>
    ///     Returns a new point normalized to 0.0-1.0
    /// </summary>
    public static SKPoint GetScaledPoint(this Layer layer, SKPoint point, bool absolute)
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
    ///     Returns the offset from the given point to the top-left of the layer
    /// </summary>
    public static SKPoint GetDragOffset(this Layer layer, SKPoint dragStart)
    {
        // Figure out what the top left will be if the shape moves to the current cursor position
        SKPoint scaledDragStart = GetScaledPoint(layer, dragStart, true);
        SKPoint tempAnchor = GetLayerAnchorPosition(layer, scaledDragStart);
        SKPoint tempTopLeft = GetLayerPath(layer, true, true, true, tempAnchor)[0];

        // Get the shape's position
        SKPoint topLeft = GetLayerPath(layer, true, true, true)[0];

        // The difference between the two is the offset
        return topLeft - tempTopLeft;
    }
}