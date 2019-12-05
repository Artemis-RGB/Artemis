using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Surface;
using Artemis.Core.Plugins.LayerElement;
using SkiaSharp;

namespace Artemis.Plugins.LayerElements.Noise
{
    public class NoiseLayerElement : LayerElement
    {
        private SKShader _shader;
        private List<SKColor> _testColors;
        private SKPaint _paint;

        public NoiseLayerElement(Layer layer, Guid guid, NoiseLayerElementSettings settings, LayerElementDescriptor descriptor) : base(layer, guid, settings, descriptor)
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
            var shader = SKShader.CreatePerlinNoiseFractalNoise(1, 1, 1, 1);

            var oldShader = _shader;
            var oldPaint = _paint;
            _shader = shader;
            _paint = new SKPaint {Shader = _shader, FilterQuality = SKFilterQuality.Low};
            oldShader?.Dispose();
            oldPaint?.Dispose();
        }

        public new NoiseLayerElementSettings Settings { get; }

        public override LayerElementViewModel GetViewModel()
        {
            return new NoiseLayerElementViewModel(this);
        }

        public override void Render(ArtemisSurface surface, SKCanvas canvas)
        {
            canvas.DrawRect(Layer.AbsoluteRenderRectangle, _paint);
        }
    }
}