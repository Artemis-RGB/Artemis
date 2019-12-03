using System.Collections.Generic;
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

        public BrushLayerElement(Layer layer, BrushLayerElementSettings settings, LayerElementDescriptor descriptor) : base(layer, settings, descriptor)
        {
            Settings = settings ?? new BrushLayerElementSettings();

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

        private void CreateShader()
        {
            var center = new SKPoint(Layer.AbsoluteRenderRectangle.MidX, Layer.AbsoluteRenderRectangle.MidY);
            var shader = SKShader.CreateSweepGradient(center, _testColors.ToArray(), null, SKShaderTileMode.Clamp, 0, 360);

            var oldShader = _shader;
            _shader = shader;

            oldShader?.Dispose();
        }

        public new BrushLayerElementSettings Settings { get; }

        public override LayerElementViewModel GetViewModel()
        {
            return new BrushLayerElementViewModel(this);
        }

        public override void Render(ArtemisSurface surface, SKCanvas canvas)
        {
            using (var paint = new SKPaint {Shader = _shader, FilterQuality = SKFilterQuality.Low})
            {
                canvas.DrawRect(Layer.AbsoluteRenderRectangle, paint);
            }
        }
    }
}