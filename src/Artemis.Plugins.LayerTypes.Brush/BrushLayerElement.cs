using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Surface;
using Artemis.Core.Plugins.LayerElement;
using SkiaSharp;

namespace Artemis.Plugins.LayerElements.Brush
{
    public class BrushLayerElement : LayerElement
    {
        private SKShader _shader;
        private List<SKColor> _testColors;
        private SKPaint _paint;

        public BrushLayerElement(Layer layer, Guid guid, BrushLayerElementSettings settings, LayerElementDescriptor descriptor) : base(layer, guid, settings, descriptor)
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

        private void CreateShader()
        {
            var center = new SKPoint(Layer.AbsoluteRenderRectangle.MidX, Layer.AbsoluteRenderRectangle.MidY);
            SKShader shader;
            switch (Settings.BrushType)
            {
                case BrushType.Solid:
                    shader = SKShader.CreateColor(_testColors.First());
                    break;
                case BrushType.LinearGradient:
                    shader = SKShader.CreateLinearGradient(new SKPoint(0, 0), new SKPoint(Layer.AbsoluteRenderRectangle.Width, 0), _testColors.ToArray(), SKShaderTileMode.Clamp);
                    break;
                case BrushType.RadialGradient:
                    shader = SKShader.CreateRadialGradient(center, Math.Min(Layer.AbsoluteRenderRectangle.Width, Layer.AbsoluteRenderRectangle.Height), _testColors.ToArray(), SKShaderTileMode.Clamp);
                    break;
                case BrushType.SweepGradient:
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

        public new BrushLayerElementSettings Settings { get; }

        public override LayerElementViewModel GetViewModel()
        {
            return new BrushLayerElementViewModel(this);
        }

        public override void Render(ArtemisSurface surface, SKCanvas canvas)
        {
            canvas.DrawRect(Layer.AbsoluteRenderRectangle, _paint);
        }
    }
}