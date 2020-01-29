using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core.Models.Profile;
using Artemis.Core.Plugins.LayerBrush;
using SkiaSharp;

namespace Artemis.Plugins.LayerBrushes.Color
{
    public class ColorBrush : LayerBrush
    {
        private SKPaint _paint;
        private SKShader _shader;
        private readonly List<SKColor> _testColors;

        public ColorBrush(Layer layer, ColorBrushSettings settings, LayerBrushDescriptor descriptor) : base(layer, settings, descriptor)
        {
            Settings = settings;

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
            Settings.PropertyChanged += (sender, args) => CreateShader();
        }

        public new ColorBrushSettings Settings { get; }

        private void CreateShader()
        {
            var center = new SKPoint(Layer.AbsoluteRectangle.MidX, Layer.AbsoluteRectangle.MidY);
            SKShader shader;
            switch (Settings.GradientType)
            {
                case GradientType.Solid:
                    shader = SKShader.CreateColor(_testColors.First());
                    break;
                case GradientType.LinearGradient:
                    shader = SKShader.CreateLinearGradient(new SKPoint(0, 0), new SKPoint(Layer.AbsoluteRectangle.Width, 0), _testColors.ToArray(), SKShaderTileMode.Repeat);
                    break;
                case GradientType.RadialGradient:
                    shader = SKShader.CreateRadialGradient(center, Math.Min(Layer.AbsoluteRectangle.Width, Layer.AbsoluteRectangle.Height), _testColors.ToArray(), SKShaderTileMode.Repeat);
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

        public override LayerBrushViewModel GetViewModel()
        {
            return new ColorBrushViewModel(this);
        }

        public override void Render(SKCanvas canvas)
        {
            canvas.DrawPath(Layer.LayerShape.RenderPath, _paint);
        }
    }
}