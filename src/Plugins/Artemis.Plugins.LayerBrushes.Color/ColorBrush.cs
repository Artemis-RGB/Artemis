using System;
using System.ComponentModel;
using System.Linq;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Profile.LayerProperties;
using Artemis.Core.Plugins.LayerBrush;
using SkiaSharp;

namespace Artemis.Plugins.LayerBrushes.Color
{
    public class ColorBrush : LayerBrush
    {
        private SKColor _color;
        private SKPaint _paint;
        private SKShader _shader;
        private SKRect _shaderBounds;

        public ColorBrush(Layer layer, LayerBrushDescriptor descriptor) : base(layer, descriptor)
        {
            GradientTypeProperty = RegisterLayerProperty("Brush.GradientType", "Gradient type", "The type of color brush to draw", GradientType.Solid);
            GradientTypeProperty.CanUseKeyframes = false;

            UpdateColorProperties();

            Layer.RenderPropertiesUpdated += (sender, args) => CreateShader();
            GradientTypeProperty.ValueChanged += (sender, args) => UpdateColorProperties();
        }

        public LayerProperty<SKColor> ColorProperty { get; set; }
        public LayerProperty<ColorGradient> GradientProperty { get; set; }
        public LayerProperty<GradientType> GradientTypeProperty { get; set; }

        public override void Update(double deltaTime)
        {
            // Only check if a solid is being drawn, because that can be changed by keyframes
            if (GradientTypeProperty.Value == GradientType.Solid && _color != ColorProperty.CurrentValue)
            {
                // If the color was changed since the last frame, recreate the shader
                _color = ColorProperty.CurrentValue;
                CreateShader();
            }

            base.Update(deltaTime);
        }

        public override void Render(SKCanvas canvas, SKImageInfo canvasInfo, SKPath path, SKPaint paint)
        {
            if (path.Bounds != _shaderBounds)
            {
                _shaderBounds = path.Bounds;
                CreateShader();
            }

            paint.Shader = _shader;
            canvas.DrawPath(path, paint);
        }

        private void UpdateColorProperties()
        {
            if (GradientTypeProperty.Value == GradientType.Solid)
            {
                UnRegisterLayerProperty(GradientProperty);
                ColorProperty = RegisterLayerProperty("Brush.Color", "Color", "The color of the brush", new SKColor(255, 0, 0));
                ColorProperty.ValueChanged += (sender, args) => CreateShader();
            }
            else
            {
                UnRegisterLayerProperty(ColorProperty);
                GradientProperty = RegisterLayerProperty("Brush.Gradient", "Gradient", "The gradient of the brush", new ColorGradient());
                GradientProperty.CanUseKeyframes = false;
                GradientProperty.Value.PropertyChanged += (sender, args) => CreateShader();

                if (!GradientProperty.Value.Stops.Any())
                    GradientProperty.Value.MakeFabulous();
            }

            CreateShader();
        }

        private void CreateShader()
        {
            var center = new SKPoint(_shaderBounds.MidX, _shaderBounds.MidY);
            var shader = GradientTypeProperty.CurrentValue switch
            {
                GradientType.Solid => SKShader.CreateColor(_color),
                GradientType.LinearGradient => SKShader.CreateLinearGradient(
                    new SKPoint(_shaderBounds.Left, _shaderBounds.Top),
                    new SKPoint(_shaderBounds.Right, _shaderBounds.Bottom),
                    GradientProperty.Value.GetColorsArray(),
                    GradientProperty.Value.GetPositionsArray(),
                    SKShaderTileMode.Repeat),
                GradientType.RadialGradient => SKShader.CreateRadialGradient(
                    center,
                    Math.Min(_shaderBounds.Width, _shaderBounds.Height),
                    GradientProperty.Value.GetColorsArray(),
                    GradientProperty.Value.GetPositionsArray(),
                    SKShaderTileMode.Repeat),
                GradientType.SweepGradient => SKShader.CreateSweepGradient(
                    center,
                    GradientProperty.Value.GetColorsArray(),
                    GradientProperty.Value.GetPositionsArray(),
                    SKShaderTileMode.Clamp,
                    0,
                    360),
                _ => throw new ArgumentOutOfRangeException()
            };

            var oldShader = _shader;
            var oldPaint = _paint;
            _shader = shader;
            _paint = new SKPaint {Shader = _shader, FilterQuality = SKFilterQuality.Low};
            oldShader?.Dispose();
            oldPaint?.Dispose();
        }
    }

    public enum GradientType
    {
        [Description("Solid")]
        Solid,

        [Description("Linear Gradient")]
        LinearGradient,

        [Description("Radial Gradient")]
        RadialGradient,

        [Description("Sweep Gradient")]
        SweepGradient
    }
}