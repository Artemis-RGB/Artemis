using System;
using System.Windows;
using System.Windows.Media;
using Artemis.Core;
using Artemis.Core.Models.Profile;
using Artemis.Core.Services;
using Artemis.UI.PropertyInput;
using Artemis.UI.Services.Interfaces;
using Artemis.UI.Shared.Services.Interfaces;
using SkiaSharp;
using SkiaSharp.Views.WPF;

namespace Artemis.UI.Services
{
    public class LayerEditorService : ILayerEditorService
    {
        private readonly IProfileEditorService _profileEditorService;
        private readonly ISettingsService _settingsService;

        public LayerEditorService(ISettingsService settingsService, IProfileEditorService profileEditorService)
        {
            _settingsService = settingsService;
            _profileEditorService = profileEditorService;
            RegisterBuiltInPropertyEditors();
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
        public Point GetLayerAnchorPosition(Layer layer, SKPoint? positionOverride = null)
        {
            var layerBounds = GetLayerBounds(layer).ToSKRect();
            var positionProperty = layer.Transform.Position.CurrentValue;
            if (positionOverride != null)
                positionProperty = positionOverride.Value;

            // Start at the center of the shape
            var position = new Point(layerBounds.MidX, layerBounds.MidY);

            // Apply translation
            position.X += positionProperty.X * layerBounds.Width;
            position.Y += positionProperty.Y * layerBounds.Height;

            return position;
        }

        /// <inheritdoc />
        public TransformGroup GetLayerTransformGroup(Layer layer)
        {
            var layerBounds = GetLayerBounds(layer).ToSKRect();

            // Apply transformation like done by the core during layer rendering.
            // The order in which translations are applied are different because the UI renders the shape inside
            // the layer using the structure of the XAML while the Core has to deal with that by applying the layer
            // position to the translation
            var anchorPosition = GetLayerAnchorPosition(layer);
            var anchorProperty = layer.Transform.AnchorPoint.CurrentValue;

            // Translation originates from the unscaled center of the shape and is tied to the anchor
            var x = anchorPosition.X - layerBounds.MidX - anchorProperty.X * layerBounds.Width;
            var y = anchorPosition.Y - layerBounds.MidY - anchorProperty.Y * layerBounds.Height;

            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(new TranslateTransform(x, y));
            transformGroup.Children.Add(new ScaleTransform(layer.Transform.Scale.CurrentValue.Width / 100f, layer.Transform.Scale.CurrentValue.Height / 100f, anchorPosition.X, anchorPosition.Y));
            transformGroup.Children.Add(new RotateTransform(layer.Transform.Rotation.CurrentValue, anchorPosition.X, anchorPosition.Y));

            return transformGroup;
        }

        /// <inheritdoc />
        public SKPath GetLayerPath(Layer layer, bool includeTranslation, bool includeScale, bool includeRotation, SKPoint? anchorOverride = null)
        {
            var layerBounds = GetLayerBounds(layer).ToSKRect();

            // Apply transformation like done by the core during layer rendering (same differences apply as in GetLayerTransformGroup)
            var anchorPosition = GetLayerAnchorPosition(layer).ToSKPoint();
            if (anchorOverride != null)
                anchorPosition = anchorOverride.Value;

            var anchorProperty = layer.Transform.AnchorPoint.CurrentValue;

            // Translation originates from the unscaled center of the shape and is tied to the anchor
            var x = anchorPosition.X - layerBounds.MidX - anchorProperty.X * layerBounds.Width;
            var y = anchorPosition.Y - layerBounds.MidY - anchorProperty.Y * layerBounds.Height;

            var path = new SKPath();
            path.AddRect(layerBounds);
            if (includeTranslation)
                path.Transform(SKMatrix.MakeTranslation(x, y));
            if (includeScale)
                path.Transform(SKMatrix.MakeScale(layer.Transform.Scale.CurrentValue.Width / 100f, layer.Transform.Scale.CurrentValue.Height / 100f, anchorPosition.X, anchorPosition.Y));
            if (includeRotation)
                path.Transform(SKMatrix.MakeRotationDegrees(layer.Transform.Rotation.CurrentValue, anchorPosition.X, anchorPosition.Y));

            return path;
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

        public SKPoint GetDragOffset(Layer layer, SKPoint dragStart)
        {
            // Figure out what the top left will be if the shape moves to the current cursor position
            var scaledDragStart = GetScaledPoint(layer, dragStart, true);
            var tempAnchor = GetLayerAnchorPosition(layer, scaledDragStart).ToSKPoint();
            var tempTopLeft = GetLayerPath(layer, true, true, true, tempAnchor)[0];

            // Get the shape's position
            var topLeft = GetLayerPath(layer, true, true, true)[0];

            // The difference between the two is the offset
            return topLeft - tempTopLeft;
        }

        private void RegisterBuiltInPropertyEditors()
        {
            _profileEditorService.RegisterPropertyInput<BrushPropertyInputViewModel>(Constants.CorePluginInfo);
            _profileEditorService.RegisterPropertyInput<ColorGradientPropertyInputViewModel>(Constants.CorePluginInfo);
            _profileEditorService.RegisterPropertyInput<FloatPropertyInputViewModel>(Constants.CorePluginInfo);
            _profileEditorService.RegisterPropertyInput<IntPropertyInputViewModel>(Constants.CorePluginInfo);
            _profileEditorService.RegisterPropertyInput<SKColorPropertyInputViewModel>(Constants.CorePluginInfo);
            _profileEditorService.RegisterPropertyInput<SKPointPropertyInputViewModel>(Constants.CorePluginInfo);
            _profileEditorService.RegisterPropertyInput<SKSizePropertyInputViewModel>(Constants.CorePluginInfo);
        }
    }
}