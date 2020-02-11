using System;
using System.Collections.Generic;
using System.ComponentModel;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Profile.LayerProperties;
using Artemis.Core.Plugins.LayerBrush;
using SkiaSharp;

namespace Artemis.Plugins.LayerBrushes.Color
{
    public class ColorBrush : LayerBrush
    {
        private readonly List<SKColor> _testColors;
        private SKColor _color;
        private SKPaint _paint;
        private SKShader _shader;

        public ColorBrush(Layer layer, LayerBrushDescriptor descriptor) : base(layer, descriptor)
        {
            ColorProperty = RegisterLayerProperty<SKColor>("Brush.Color", "Main color", "The color of the brush.");
            GradientTypeProperty = RegisterLayerProperty<GradientType>("Brush.GradientType", "Gradient type", "The scale of the noise.");
            GradientTypeProperty.CanUseKeyframes = false;

            _testColors = new List<SKColor>();
            for (var i = 0; i < 9; i++)
            {
                if (i != 8)
                    _testColors.Add(SKColor.FromHsv(i * 32, 100, 100));
                else
                    _testColors.Add(SKColor.FromHsv(0, 100, 100));
            }

            CreateShader();
            Layer.RenderPropertiesUpdated += (sender, args) => CreateShader();
        }

        public LayerProperty<SKColor> ColorProperty { get; set; }
        public LayerProperty<GradientType> GradientTypeProperty { get; set; }

        private void CreateShader()
        {
            var center = new SKPoint(Layer.Bounds.MidX, Layer.Bounds.MidY);
            SKShader shader;
            switch (GradientTypeProperty.CurrentValue)
            {
                case GradientType.Solid:
                    shader = SKShader.CreateColor(_color);
                    break;
                case GradientType.LinearGradient:
                    shader = SKShader.CreateLinearGradient(new SKPoint(0, 0), new SKPoint(Layer.Bounds.Width, 0), _testColors.ToArray(), SKShaderTileMode.Repeat);
                    break;
                case GradientType.RadialGradient:
                    shader = SKShader.CreateRadialGradient(center, Math.Min(Layer.Bounds.Width, Layer.Bounds.Height), _testColors.ToArray(), SKShaderTileMode.Repeat);
                    break;
                case GradientType.SweepGradient:
                    shader = SKShader.CreateSweepGradient(center, _testColors.ToArray(), null, SKShaderTileMode.Clamp, 0, 360);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var oldShader = _shader;
            var oldPaint = _paint;
            _shader = shader;
            _paint = new SKPaint {Shader = _shader, FilterQuality = SKFilterQuality.Low};
            oldShader?.Dispose();
            oldPaint?.Dispose();
        }

        public override void Update(double deltaTime)
        {
            // Only recreate the shader if the color changed
            if (_color != ColorProperty.CurrentValue)
            {
                _color = ColorProperty.CurrentValue;
                CreateShader();
            }

            base.Update(deltaTime);
        }

        public override void Render(SKCanvas canvas, SKPath path, SKPaint paint)
        {
            paint.Shader = _shader;
            canvas.DrawPath(path, paint);
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