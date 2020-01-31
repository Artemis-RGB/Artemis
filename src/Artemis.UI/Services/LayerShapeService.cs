using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Profile.LayerShapes;
using Artemis.Core.Services;
using Artemis.UI.Services.Interfaces;
using SkiaSharp;
using SkiaSharp.Views.WPF;

namespace Artemis.UI.Services
{
    public class LayerEditorService : ILayerEditorService
    {
        private readonly ISettingsService _settingsService;

        public LayerEditorService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        /// <inheritdoc />
        public Rect GetLayerBounds(Layer layer)
        {
            // Adjust the render rectangle for the difference in render scale
            var renderScale = _settingsService.GetSetting("Core.RenderScale", 1.0).Value;
            return new Rect(
                layer.Bounds.Left / renderScale * 1,
                layer.Bounds.Top / renderScale * 1,
                Math.Max(0, layer.Bounds.Width / renderScale * 1),
                Math.Max(0, layer.Bounds.Height / renderScale * 1)
            );
        }

        /// <inheritdoc />
        public Rect GetLayerShapeBounds(LayerShape layerShape)
        {
            // Adjust the render rectangle for the difference in render scale
            var renderScale = _settingsService.GetSetting("Core.RenderScale", 1.0).Value;
            return new Rect(
                layerShape.Bounds.Left / renderScale * 1,
                layerShape.Bounds.Top / renderScale * 1,
                Math.Max(0, layerShape.Bounds.Width / renderScale * 1),
                Math.Max(0, layerShape.Bounds.Height / renderScale * 1)
            );
        }

        /// <inheritdoc />
        public Point GetLayerAnchorPosition(Layer layer)
        {
            var layerBounds = GetLayerBounds(layer);
            var shapeBounds = GetLayerShapeBounds(layer.LayerShape);
            var positionProperty = layer.PositionProperty.CurrentValue;

            // Start at the center of the shape
            var position = new Point(shapeBounds.Left, shapeBounds.Top);

            // Apply translation
            position.X += positionProperty.X * layerBounds.Width;
            position.Y += positionProperty.Y * layerBounds.Height;

            return position;
        }

        /// <inheritdoc />
        public TransformGroup GetLayerTransformGroup(Layer layer)
        {
            var layerBounds = GetLayerBounds(layer).ToSKRect();
            var shapeBounds = GetLayerShapeBounds(layer.LayerShape).ToSKRect();

            // Apply transformation like done by the core during layer rendering.
            // The order in which translations are applied are different because the UI renders the shape inside
            // the layer using the structure of the XAML while the Core has to deal with that by applying the layer
            // position to the translation
            var anchorPosition = GetLayerAnchorPosition(layer);
            var anchorProperty = layer.AnchorPointProperty.CurrentValue;

            // Translation originates from the unscaled center of the shape and is tied to the anchor
            var x = anchorPosition.X - shapeBounds.Width / 2 - anchorProperty.X * layerBounds.Width;
            var y = anchorPosition.Y - shapeBounds.Height / 2 - anchorProperty.Y * layerBounds.Height;

            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(new TranslateTransform(x, y));
            transformGroup.Children.Add(new ScaleTransform(layer.SizeProperty.CurrentValue.Width, layer.SizeProperty.CurrentValue.Height, anchorPosition.X, anchorPosition.Y));
            transformGroup.Children.Add(new RotateTransform(layer.RotationProperty.CurrentValue, anchorPosition.X, anchorPosition.Y));

            return transformGroup;
        }

        /// <inheritdoc />
        public SKPath GetLayerPath(Layer layer, bool includeTranslation, bool includeScale, bool includeRotation)
        {
            var layerBounds = GetLayerBounds(layer).ToSKRect();
            var shapeBounds = GetLayerShapeBounds(layer.LayerShape).ToSKRect();

            // Apply transformation like done by the core during layer rendering (same differences apply as in GetLayerTransformGroup)
            var anchorPosition = GetLayerAnchorPosition(layer).ToSKPoint();
            var anchorProperty = layer.AnchorPointProperty.CurrentValue;

            // Translation originates from the unscaled center of the shape and is tied to the anchor
            var x = anchorPosition.X - shapeBounds.Width / 2 - anchorProperty.X * layerBounds.Width;
            var y = anchorPosition.Y - shapeBounds.Height / 2 - anchorProperty.Y * layerBounds.Height;

            var path = new SKPath();
            path.AddRect(shapeBounds);
            if (includeTranslation)
                path.Transform(SKMatrix.MakeTranslation(x, y));
            if (includeScale)
                path.Transform(SKMatrix.MakeScale(layer.SizeProperty.CurrentValue.Width, layer.SizeProperty.CurrentValue.Height, anchorPosition.X, anchorPosition.Y));
            if (includeRotation)
                path.Transform(SKMatrix.MakeRotationDegrees(layer.RotationProperty.CurrentValue, anchorPosition.X, anchorPosition.Y));

            return path;
        }


        /// <inheritdoc />
        public void SetShapeBaseFromRectangle(LayerShape layerShape, Rect rect)
        {
            if (!layerShape.Layer.Leds.Any())
            {
                layerShape.ScaledRectangle = SKRect.Empty;
                return;
            }

            var layerBounds = GetLayerBounds(layerShape.Layer).ToSKRect();

            // Compensate for the current value of the position transformation
            rect.X += rect.Width / 2;
            rect.X -= layerBounds.Width * layerShape.Layer.PositionProperty.CurrentValue.X;
            rect.X += layerBounds.Width * layerShape.Layer.AnchorPointProperty.CurrentValue.X * layerShape.Layer.SizeProperty.CurrentValue.Width;

            rect.Y += rect.Height / 2;
            rect.Y -= layerBounds.Height * layerShape.Layer.PositionProperty.CurrentValue.Y;
            rect.Y += layerBounds.Height * layerShape.Layer.AnchorPointProperty.CurrentValue.Y * layerShape.Layer.SizeProperty.CurrentValue.Height;

            // Compensate for the current value of the size transformation
            rect.Height /= layerShape.Layer.SizeProperty.CurrentValue.Height;
            rect.Width /= layerShape.Layer.SizeProperty.CurrentValue.Width;

            // Adjust the provided rect for the difference in render scale
            var renderScale = _settingsService.GetSetting("Core.RenderScale", 1.0).Value;
            layerShape.ScaledRectangle = SKRect.Create(
                100f / layerShape.Layer.Bounds.Width * ((float) (rect.Left * renderScale) - layerShape.Layer.Bounds.Left) / 100f,
                100f / layerShape.Layer.Bounds.Height * ((float) (rect.Top * renderScale) - layerShape.Layer.Bounds.Top) / 100f,
                100f / layerShape.Layer.Bounds.Width * (float) (rect.Width * renderScale) / 100f,
                100f / layerShape.Layer.Bounds.Height * (float) (rect.Height * renderScale) / 100f
            );
            layerShape.CalculateRenderProperties();
        }

        /// <inheritdoc />
        public SKPoint GetScaledPoint(Layer layer, SKPoint point, bool absolute)
        {
            var renderScale = _settingsService.GetSetting("Core.RenderScale", 1.0).Value;
            if (absolute)
            {
                return new SKPoint(
                    100f / layer.Bounds.Width * ((float) (point.X * renderScale) - layer.Bounds.Left) / 100f,
                    100f / layer.Bounds.Height * ((float) (point.Y * renderScale) - layer.Bounds.Top) / 100f
                );
            }

            return new SKPoint(
                100f / layer.Bounds.Width * (float) (point.X * renderScale) / 100f,
                100f / layer.Bounds.Height * (float) (point.Y * renderScale) / 100f
            );
        }
    }
}