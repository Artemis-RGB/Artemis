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
            GradientTypeProperty = RegisterLayerProperty<GradientType>("Brush.GradientType", "Gradient type", "The type of color brush to draw");
            GradientTypeProperty.CanUseKeyframes = false;

            UpdateColorProperties();

            Layer.RenderPropertiesUpdated += (sender, args) => CreateShader(_shaderBounds);
            GradientTypeProperty.ValueChanged += (sender, args) => UpdateColorProperties();
        }

        private void UpdateColorProperties()
        {
            UnRegisterLayerProperty(ColorProperty);
            ColorProperty = null;
            UnRegisterLayerProperty(GradientProperty);
            GradientProperty = null;

            if (GradientTypeProperty.Value == GradientType.Solid)
            {
                ColorProperty = RegisterLayerProperty("Brush.Color", "Color", "The color of the brush", new SKColor(255, 0, 0));
                ColorProperty.ValueChanged += (sender, args) => CreateShader(_shaderBounds);
            }
            else
            {
                GradientProperty = RegisterLayerProperty("Brush.Gradient", "Gradient", "The gradient of the brush", new ColorGradient());
                GradientProperty.Value.PropertyChanged += (sender, args) => CreateShader(_shaderBounds);

                if (!GradientProperty.Value.Stops.Any()) 
                    GradientProperty.Value.MakeFabulous();
            }

            CreateShader(_shaderBounds);
        }

        public LayerProperty<SKColor> ColorProperty { get; set; }
        public LayerProperty<ColorGradient> GradientProperty { get; set; }
        public LayerProperty<GradientType> GradientTypeProperty { get; set; }

        public override void Update(double deltaTime)
        {
            // Only recreate the shader if the color changed
            if (ColorProperty != null && _color != ColorProperty.CurrentValue)
            {
                _color = ColorProperty.CurrentValue;
                CreateShader(_shaderBounds);
            }

            base.Update(deltaTime);
        }

        public override void Render(SKCanvas canvas, SKImageInfo canvasInfo, SKPath path, SKPaint paint)
        {
            if (path.Bounds != _shaderBounds)
                CreateShader(path.Bounds);

            paint.Shader = _shader;
            canvas.DrawPath(path, paint);
        }

        private void CreateShader(SKRect pathBounds)
        {
            var center = new SKPoint(pathBounds.MidX, pathBounds.MidY);
            SKShader shader;
            switch (GradientTypeProperty.CurrentValue)
            {
                case GradientType.Solid:
                    shader = SKShader.CreateColor(_color);
                    break;
                case GradientType.LinearGradient:
                    shader = SKShader.CreateLinearGradient(new SKPoint(pathBounds.Left, pathBounds.Top), new SKPoint(pathBounds.Right, pathBounds.Bottom),
                        GradientProperty.Value.GetColorsArray(),
                        GradientProperty.Value.GetPositionsArray(), SKShaderTileMode.Repeat);
                    break;
                case GradientType.RadialGradient:
                    shader = SKShader.CreateRadialGradient(center, Math.Min(pathBounds.Width, pathBounds.Height),
                        GradientProperty.Value.GetColorsArray(),
                        GradientProperty.Value.GetPositionsArray(), SKShaderTileMode.Repeat);
                    break;
                case GradientType.SweepGradient:
                    shader = SKShader.CreateSweepGradient(center,
                        GradientProperty.Value.GetColorsArray(),
                        GradientProperty.Value.GetPositionsArray(), SKShaderTileMode.Clamp, 0, 360);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var oldShader = _shader;
            var oldPaint = _paint;
            _shader = shader;
            _paint = new SKPaint {Shader = _shader, FilterQuality = SKFilterQuality.Low};
            _shaderBounds = pathBounds;
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