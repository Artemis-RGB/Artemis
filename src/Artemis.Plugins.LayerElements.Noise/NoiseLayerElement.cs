using System;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Surface;
using Artemis.Core.Plugins.LayerElement;
using SkiaSharp;

namespace Artemis.Plugins.LayerElements.Noise
{
    public class NoiseLayerElement : LayerElement
    {
        private readonly OpenSimplexNoise _noise;
        private float _z;

        public NoiseLayerElement(Layer layer, Guid guid, NoiseLayerElementSettings settings, LayerElementDescriptor descriptor) : base(layer, guid, settings, descriptor)
        {
            Settings = settings;

            _z = 0.001f;
            _noise = new OpenSimplexNoise(Guid.GetHashCode());
        }

        public new NoiseLayerElementSettings Settings { get; }


        public override void Update(double deltaTime)
        {
            _z += Settings.AnimationSpeed;
            base.Update(deltaTime);
        }

        public override LayerElementViewModel GetViewModel()
        {
            return new NoiseLayerElementViewModel(this);
        }

        public override void Render(ArtemisSurface surface, SKCanvas canvas)
        {
            var width = Layer.AbsoluteRenderRectangle.Width / 2;
            var height = Layer.AbsoluteRenderRectangle.Height / 2;
            using (var bitmap = new SKBitmap(new SKImageInfo((int) Layer.AbsoluteRenderRectangle.Width, (int) Layer.AbsoluteRenderRectangle.Height)))
            {
                for (var x = 0; x < width; x++)
                {
                    for (var y = 0; y < height; y++)
                    {
                        var v = _noise.Evaluate(Settings.XScale * x / width, Settings.YScale * y / height, _z);
                        bitmap.SetPixel(x, y, new SKColor(255, 255, 255, (byte) ((v + 1) * 127)));
                    }
                }

                canvas.DrawBitmap(bitmap, SKRect.Create(0, 0, width, height), Layer.AbsoluteRenderRectangle, new SKPaint {BlendMode = Settings.BlendMode});
            }
        }
    }
}