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
        private SKRect _shaderBounds;

        public ColorBrush(Layer layer, LayerBrushDescriptor descriptor) : base(layer, descriptor)
        {
            ColorProperty = RegisterLayerProperty("Brush.Color", "Color", "The color of the brush", new SKColor(255,0,0));
            GradientTypeProperty = RegisterLayerProperty<GradientType>("Brush.GradientType", "Gradient type", "The type of color brush to draw");
            GradientTypeProperty.CanUseKeyframes = false;

            _testColors = new List<SKColor>();
            for (var i = 0; i < 9; i++)
            {
                if (i != 8)
                    _testColors.Add(SKColor.FromHsv(i * 32, 100, 100));
                else
                    _testColors.Add(SKColor.FromHsv(0, 100, 100));
            }

            CreateShader(_shaderBounds);
            Layer.RenderPropertiesUpdated += (sender, args) => CreateShader(_shaderBounds);
            GradientTypeProperty.ValueChanged += (sender, args) => CreateShader(_shaderBounds);
        }

        public LayerProperty<SKColor> ColorProperty { get; set; }
        public LayerProperty<GradientType> GradientTypeProperty { get; set; }

        public override void Update(double deltaTime)
        {
            // Only recreate the shader if the color changed
            if (_color != ColorProperty.CurrentValue)
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
            var center = new SKPoint(Layer.Bounds.MidX, Layer.Bounds.MidY);
            SKShader shader;
            switch (GradientTypeProperty.CurrentValue)
            {
                case GradientType.Solid:
                    shader = SKShader.CreateColor(_color);
                    break;
                case GradientType.LinearGradient:
                    shader = SKShader.CreateLinearGradient(new SKPoint(0, 0), new SKPoint(pathBounds.Width, 0), _testColors.ToArray(), SKShaderTileMode.Repeat);
                    break;
                case GradientType.RadialGradient:
                    shader = SKShader.CreateRadialGradient(center, Math.Min(pathBounds.Width, pathBounds.Height), _testColors.ToArray(), SKShaderTileMode.Repeat);
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