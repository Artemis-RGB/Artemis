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
                100f / layerShape.Layer.AbsoluteRectangle.Width * ((float) (rect.Left * renderScale) - layerShape.Layer.AbsoluteRectangle.Left) / 100f,
                100f / layerShape.Layer.AbsoluteRectangle.Height * ((float) (rect.Top * renderScale) - layerShape.Layer.AbsoluteRectangle.Top) / 100f,
                100f / layerShape.Layer.AbsoluteRectangle.Width * (float) (rect.Width * renderScale) / 100f,
                100f / layerShape.Layer.AbsoluteRectangle.Height * (float) (rect.Height * renderScale) / 100f
            );
            layerShape.CalculateRenderProperties();
        }

        /// <inheritdoc />
        public TransformGroup GetLayerTransformGroup(Layer layer)
        {
            var layerRect = GetLayerRenderRect(layer).ToSKRect();
            var shapeRect = GetShapeRenderRect(layer.LayerShape).ToSKRect();

            // Apply transformation like done by the core during layer rendering
            var anchor = layer.AnchorPointProperty.CurrentValue;
            var position = layer.PositionProperty.CurrentValue;
            var size = layer.SizeProperty.CurrentValue;
            var rotation = layer.RotationProperty.CurrentValue;
            // Scale the anchor and make it originate from the center of the untranslated layer shape
            anchor.X = anchor.X * layerRect.Width + shapeRect.MidX;
            anchor.Y = anchor.Y * layerRect.Height + shapeRect.MidY;
            
            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(new ScaleTransform(size.Width, size.Height, anchor.X, anchor.Y));
            transformGroup.Children.Add(new RotateTransform(rotation, anchor.X, anchor.Y));
            transformGroup.Children.Add(new TranslateTransform(position.X * layerRect.Width, position.Y * layerRect.Height));

            return transformGroup;
        }
    }

    public interface ILayerEditorService : IArtemisUIService
    {
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
        /// Creates a WPF transform group that contains all the transformations required to render the provided layer.
        /// Note: Run on UI thread.
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        TransformGroup GetLayerTransformGroup(Layer layer);
    }
}