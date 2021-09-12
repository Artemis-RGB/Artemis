using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Artemis.Core;
using RGB.NET.Core;
using SkiaSharp;
using SkiaSharp.Views.WPF;
using Point = System.Windows.Point;

namespace Artemis.UI.Services
{
    public class LayerEditorService : ILayerEditorService
    {
        /// <inheritdoc />
        public Rect GetLayerBounds(Layer layer)
        {
            return new Rect(
                new Point(
                    layer.Leds.Min(l => l.RgbLed.AbsoluteBoundary.Location.X),
                    layer.Leds.Min(l => l.RgbLed.AbsoluteBoundary.Location.Y
                    )),
                new Point(
                    layer.Leds.Max(l => l.RgbLed.AbsoluteBoundary.Location.X + l.RgbLed.AbsoluteBoundary.Size.Width),
                    layer.Leds.Max(l => l.RgbLed.AbsoluteBoundary.Location.Y + l.RgbLed.AbsoluteBoundary.Size.Height
                    ))
            );
        }

        /// <inheritdoc />
        public Point GetLayerAnchorPosition(Layer layer, SKPoint? positionOverride = null)
        {
            SKRect layerBounds = GetLayerBounds(layer).ToSKRect();
            SKPoint positionProperty = layer.Transform.Position.CurrentValue;
            if (positionOverride != null)
                positionProperty = positionOverride.Value;

            // Start at the center of the shape
            Point position = new(layerBounds.MidX, layerBounds.MidY);

            // Apply translation
            position.X += positionProperty.X * layerBounds.Width;
            position.Y += positionProperty.Y * layerBounds.Height;

            return position;
        }

        /// <inheritdoc />
        public TransformGroup GetLayerTransformGroup(Layer layer)
        {
            SKRect layerBounds = GetLayerBounds(layer).ToSKRect();

            // Apply transformation like done by the core during layer rendering.
            // The order in which translations are applied are different because the UI renders the shape inside
            // the layer using the structure of the XAML while the Core has to deal with that by applying the layer
            // position to the translation
            Point anchorPosition = GetLayerAnchorPosition(layer);
            SKPoint anchorProperty = layer.Transform.AnchorPoint.CurrentValue;

            // Translation originates from the unscaled center of the shape and is tied to the anchor
            double x = anchorPosition.X - layerBounds.MidX - anchorProperty.X * layerBounds.Width;
            double y = anchorPosition.Y - layerBounds.MidY - anchorProperty.Y * layerBounds.Height;

            TransformGroup transformGroup = new();
            transformGroup.Children.Add(new TranslateTransform(x, y));
            transformGroup.Children.Add(new ScaleTransform(layer.Transform.Scale.CurrentValue.Width / 100f, layer.Transform.Scale.CurrentValue.Height / 100f, anchorPosition.X, anchorPosition.Y));
            transformGroup.Children.Add(new RotateTransform(layer.Transform.Rotation.CurrentValue, anchorPosition.X, anchorPosition.Y));

            return transformGroup;
        }

        /// <inheritdoc />
        public SKPath GetLayerPath(Layer layer, bool includeTranslation, bool includeScale, bool includeRotation, SKPoint? anchorOverride = null)
        {
            SKRect layerBounds = GetLayerBounds(layer).ToSKRect();

            // Apply transformation like done by the core during layer rendering (same differences apply as in GetLayerTransformGroup)
            SKPoint anchorPosition = GetLayerAnchorPosition(layer).ToSKPoint();
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

        /// <inheritdoc />
        public SKPoint GetScaledPoint(Layer layer, SKPoint point, bool absolute)
        {
            SKRect bounds = GetLayerBounds(layer).ToSKRect();
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

        public SKPoint GetDragOffset(Layer layer, SKPoint dragStart)
        {
            // Figure out what the top left will be if the shape moves to the current cursor position
            SKPoint scaledDragStart = GetScaledPoint(layer, dragStart, true);
            SKPoint tempAnchor = GetLayerAnchorPosition(layer, scaledDragStart).ToSKPoint();
            SKPoint tempTopLeft = GetLayerPath(layer, true, true, true, tempAnchor)[0];

            // Get the shape's position
            SKPoint topLeft = GetLayerPath(layer, true, true, true)[0];

            // The difference between the two is the offset
            return topLeft - tempTopLeft;
        }
    }
}