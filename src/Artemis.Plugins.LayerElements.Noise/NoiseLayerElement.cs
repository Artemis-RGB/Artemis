using System;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Surface;
using Artemis.Core.Plugins.LayerElement;
using SkiaSharp;

namespace Artemis.Plugins.LayerElements.Noise
{
    public class NoiseLayerElement : LayerElement
    {
        private static Random _rand = new Random();
        private readonly OpenSimplexNoise _noise;
        private float _z;

        public NoiseLayerElement(Layer layer, Guid guid, NoiseLayerElementSettings settings, LayerElementDescriptor descriptor) : base(layer, guid, settings, descriptor)
        {
            Settings = settings;

            _z = 1;
            _noise = new OpenSimplexNoise(Guid.GetHashCode());
        }

        public new NoiseLayerElementSettings Settings { get; }


        public override void Update(double deltaTime)
        {
            // TODO: Come up with a better way to use deltaTime
            _z += Settings.AnimationSpeed / 500f / 0.04f * (float) deltaTime;

            if (_z >= float.MaxValue)
                _z = 0;

            base.Update(deltaTime);
        }

        public override LayerElementViewModel GetViewModel()
        {
            return new NoiseLayerElementViewModel(this);
        }

        public override void Render(ArtemisSurface surface, SKCanvas canvas)
        {
            // Scale down the render path
            var width = Layer.AbsoluteRenderRectangle.Width / 4;
            var height = Layer.AbsoluteRenderRectangle.Height / 4;
            
            using (var bitmap = new SKBitmap(new SKImageInfo((int) Layer.AbsoluteRenderRectangle.Width, (int) Layer.AbsoluteRenderRectangle.Height)))
            {
                for (var x = 0; x < width; x++)
                {
                    for (var y = 0; y < height; y++)
                    {
                        // Not setting pixels outside the layer clip would be faster but right now rotation takes place after the rendering, must change that first
                        var v = _noise.Evaluate(Settings.XScale * x / width, Settings.YScale * y / height, _z);
                        bitmap.SetPixel(x, y, new SKColor(0, 0, 0, (byte) ((v + 1) * 127)));
                    }
                }

                canvas.DrawBitmap(bitmap, SKRect.Create(0, 0, width, height), Layer.AbsoluteRenderRectangle, new SKPaint {BlendMode = Settings.BlendMode});
            }
        }
    }
}