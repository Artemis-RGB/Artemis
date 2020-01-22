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
        public Rect GetLayerRenderRect(Layer layer)
        {
            // Adjust the render rectangle for the difference in render scale
            var renderScale = _settingsService.GetSetting("Core.RenderScale", 1.0).Value;
            return new Rect(
                layer.AbsoluteRectangle.Left / renderScale * 1,
                layer.AbsoluteRectangle.Top / renderScale * 1,
                Math.Max(0, layer.AbsoluteRectangle.Width / renderScale * 1),
                Math.Max(0, layer.AbsoluteRectangle.Height / renderScale * 1)
            );
        }

        public Rect GetLayerRect(Layer layer)
        {
            // Adjust the render rectangle for the difference in render scale
            var renderScale = _settingsService.GetSetting("Core.RenderScale", 1.0).Value;
            return new Rect(
                layer.Rectangle.Left / renderScale * 1,
                layer.Rectangle.Top / renderScale * 1,
                Math.Max(0, layer.Rectangle.Width / renderScale * 1),
                Math.Max(0, layer.Rectangle.Height / renderScale * 1)
            );
        }

        /// <inheritdoc />
        public SKPath GetLayerPath(Layer layer, bool includeTranslation, bool includeScale, bool includeRotation)
        {
            var layerRect = GetLayerRenderRect(layer).ToSKRect();
            var shapeRect = GetShapeRenderRect(layer.LayerShape).ToSKRect();

            // Apply transformation like done by the core during layer rendering
            var anchor = GetLayerAnchor(layer, true);
            var relativeAnchor = GetLayerAnchor(layer, false);

            // Translation originates from the unscaled center of the shape and is tied to the anchor
            var x = layer.PositionProperty.CurrentValue.X * layerRect.Width - shapeRect.Width / 2 - relativeAnchor.X;
            var y = layer.PositionProperty.CurrentValue.Y * layerRect.Height - shapeRect.Height / 2 - relativeAnchor.Y;

            var path = new SKPath();
            path.AddRect(shapeRect);
            if (includeTranslation)
                path.Transform(SKMatrix.MakeTranslation(x, y));
            if (includeScale)
                path.Transform(SKMatrix.MakeScale(layer.SizeProperty.CurrentValue.Width, layer.SizeProperty.CurrentValue.Height, anchor.X, anchor.Y));
            if (includeRotation)
                path.Transform(SKMatrix.MakeRotationDegrees(layer.RotationProperty.CurrentValue, anchor.X, anchor.Y));

            return path;
        }

        public void ReverseLayerPath(Layer layer, SKPath path)
        {
            var layerRect = GetLayerRenderRect(layer).ToSKRect();
            var shapeRect = GetShapeRenderRect(layer.LayerShape).ToSKRect();

            // Apply transformation like done by the core during layer rendering
            var anchor = GetLayerAnchor(layer, true);
            var relativeAnchor = GetLayerAnchor(layer, false);

            // Translation originates from the unscaled center of the shape and is tied to the anchor
            var x = layer.PositionProperty.CurrentValue.X * layerRect.Width - shapeRect.Width / 2 - relativeAnchor.X;
            var y = layer.PositionProperty.CurrentValue.Y * layerRect.Height - shapeRect.Height / 2 - relativeAnchor.Y;

            SKMatrix.MakeScale(layer.SizeProperty.CurrentValue.Width, layer.SizeProperty.CurrentValue.Height, anchor.X, anchor.Y).TryInvert(out var scale);

            path.Transform(SKMatrix.MakeRotationDegrees(layer.RotationProperty.CurrentValue * -1, anchor.X, anchor.Y));
            path.Transform(scale);
            path.Transform(SKMatrix.MakeTranslation(x * -1, y * -1));
        }

        /// <inheritdoc />
        public SKPoint GetLayerAnchor(Layer layer, bool absolute)
        {
            var layerRect = GetLayerRect(layer);
            if (absolute)
            {
                var position = layer.PositionProperty.CurrentValue;
                position.X = (float) (position.X * layerRect.Width);
                position.Y = (float) (position.Y * layerRect.Height);
                var shapeRect = GetShapeRenderRect(layer.LayerShape);
                return new SKPoint((float) (position.X + shapeRect.Left), (float) (position.Y + shapeRect.Top));
            }

            var anchor = layer.AnchorPointProperty.CurrentValue;
            anchor.X = (float) (anchor.X * layerRect.Width);
            anchor.Y = (float) (anchor.Y * layerRect.Height);
            return new SKPoint(anchor.X, anchor.Y);
        }

        public void SetLayerAnchor(Layer layer, SKPoint point, bool absolute, TimeSpan? time)
        {
            var layerRect = GetLayerRect(layer);
            if (absolute)
            {
                var shapeRect = GetShapeRenderRect(layer.LayerShape);
                var position = new SKPoint((float) ((point.X - shapeRect.Left) / layerRect.Width), (float) ((point.Y - shapeRect.Top) / layerRect.Height));
                layer.PositionProperty.SetCurrentValue(position, time);
            }

            var anchor = new SKPoint((float) (point.X / layerRect.Width), (float) (point.Y / layerRect.Height));
            layer.AnchorPointProperty.SetCurrentValue(anchor, time);
        }

        /// <inheritdoc />
        public TransformGroup GetLayerTransformGroup(Layer layer)
        {
            var layerRect = GetLayerRenderRect(layer).ToSKRect();
            var shapeRect = GetShapeRenderRect(layer.LayerShape).ToSKRect();

            // Apply transformation like done by the core during layer rendering
            var anchor = GetLayerAnchor(layer, true);

            // Translation originates from the unscaled center of the shape and is tied to the anchor
            var x = layer.PositionProperty.CurrentValue.X * layerRect.Width - shapeRect.Width / 2 - GetLayerAnchor(layer, false).X;
            var y = layer.PositionProperty.CurrentValue.Y * layerRect.Height - shapeRect.Height / 2 - GetLayerAnchor(layer, false).Y;

            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(new TranslateTransform(x, y));
            transformGroup.Children.Add(new ScaleTransform(layer.SizeProperty.CurrentValue.Width, layer.SizeProperty.CurrentValue.Height, anchor.X, anchor.Y));
            transformGroup.Children.Add(new RotateTransform(layer.RotationProperty.CurrentValue, anchor.X, anchor.Y));

            return transformGroup;
        }

        /// <inheritdoc />
        public Rect GetShapeRenderRect(LayerShape layerShape)
        {
            // Adjust the render rectangle for the difference in render scale
            var renderScale = _settingsService.GetSetting("Core.RenderScale", 1.0).Value;
            return new Rect(
                layerShape.RenderRectangle.Left / renderScale * 1,
                layerShape.RenderRectangle.Top / renderScale * 1,
                Math.Max(0, layerShape.RenderRectangle.Width / renderScale * 1),
                Math.Max(0, layerShape.RenderRectangle.Height / renderScale * 1)
            );
        }

        /// <inheritdoc />
        public void SetShapeRenderRect(LayerShape layerShape, Rect rect)
        {
            if (!layerShape.Layer.Leds.Any())
            {
                layerShape.ScaledRectangle = SKRect.Empty;
                return;
            }

            // Adjust the provided rect for the difference in render scale
            var renderScale = _settingsService.GetSetting("Core.RenderScale", 1.0).Value;
            layerShape.ScaledRectangle = SKRect.Create(
                100f / layerShape.Layer.Rectangle.Width * ((float) (rect.Left * renderScale) - layerShape.Layer.Rectangle.Left) / 100f,
                100f / layerShape.Layer.Rectangle.Height * ((float) (rect.Top * renderScale) - layerShape.Layer.Rectangle.Top) / 100f,
                100f / layerShape.Layer.Rectangle.Width * (float) (rect.Width * renderScale) / 100f,
                100f / layerShape.Layer.Rectangle.Height * (float) (rect.Height * renderScale) / 100f
            );
            layerShape.CalculateRenderProperties();
        }

        /// <inheritdoc />
        public SKPoint GetScaledPoint(Layer layer, SKPoint point, bool absolute)
        {
            var renderScale = _settingsService.GetSetting("Core.RenderScale", 1.0).Value;
            if (absolute)
            {
                return  new SKPoint(
                    100f / layer.Rectangle.Width * ((float) (point.X * renderScale) - layer.Rectangle.Left) / 100f,
                    100f / layer.Rectangle.Height * ((float) (point.Y * renderScale) - layer.Rectangle.Top) / 100f
                );
            }

            return new SKPoint(
                100f / layer.Rectangle.Width * (float)(point.X * renderScale) / 100f,
                100f / layer.Rectangle.Height * (float)(point.Y * renderScale) / 100f
            );
        }
    }

    public interface ILayerEditorService : IArtemisUIService
    {
        /// <summary>
        ///     Returns an absolute and scaled rectangle for the given layer that is corrected for the current render scale.
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        Rect GetLayerRenderRect(Layer layer);

        /// <summary>
        ///     Returns an absolute and scaled rectangular path for the given layer that is corrected for the current render scale.
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="includeTranslation"></param>
        /// <param name="includeScale"></param>
        /// <param name="includeRotation"></param>
        /// <returns></returns>
        SKPath GetLayerPath(Layer layer, bool includeTranslation, bool includeScale, bool includeRotation);

        void ReverseLayerPath(Layer layer, SKPath path);

        /// <summary>
        ///     Returns an absolute and scaled anchor for the given layer, optionally with the translation applied.
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="absolute"></param>
        /// <returns></returns>
        SKPoint GetLayerAnchor(Layer layer, bool absolute);
        
        void SetLayerAnchor(Layer layer, SKPoint point, bool absolute, TimeSpan? time);

        /// <summary>
        ///     Creates a WPF transform group that contains all the transformations required to render the provided layer.
        ///     Note: Run on UI thread.
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        TransformGroup GetLayerTransformGroup(Layer layer);

        /// <summary>
        ///     Returns an absolute and scaled rectangle for the given shape that is corrected for the current render scale.
        /// </summary>
        /// <returns></returns>
        Rect GetShapeRenderRect(LayerShape layerShape);

        /// <summary>
        ///     Sets the render rectangle of the given shape to match the provided unscaled rectangle. The rectangle is corrected
        ///     for the current render scale.
        /// </summary>
        /// <param name="layerShape"></param>
        /// <param name="rect"></param>
        void SetShapeRenderRect(LayerShape layerShape, Rect rect);

        /// <summary>
        /// Returns a new point scaled to the layer.
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="point"></param>
        /// <param name="absolute"></param>
        /// <returns></returns>
        SKPoint GetScaledPoint(Layer layer, SKPoint point, bool absolute);
    }
}