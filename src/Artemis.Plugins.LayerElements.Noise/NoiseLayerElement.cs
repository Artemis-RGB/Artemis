using System;
using Artemis.Core.Models.Profile;
using Artemis.Core.Plugins.LayerElement;
using SkiaSharp;

namespace Artemis.Plugins.LayerElements.Noise
{
    public class NoiseLayerElement : LayerElement
    {
        private const int Scale = 6;
        private static readonly Random Rand = new Random();
        private readonly OpenSimplexNoise _noise;
        private float _z;

        public NoiseLayerElement(Layer layer, Guid guid, NoiseLayerElementSettings settings, LayerElementDescriptor descriptor) : base(layer, guid, settings, descriptor)
        {
            Settings = settings;

            _z = Rand.Next(0, 4096);
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

        public override void Render(SKPath framePath, SKCanvas canvas)
        {
            // Scale down the render path to avoid computing a value for every pixel
            var width = (int) (Math.Max(Layer.RenderRectangle.Width, Layer.RenderRectangle.Height) / Scale);
            var height = (int) (Math.Max(Layer.RenderRectangle.Width, Layer.RenderRectangle.Height) / Scale);

            using (var bitmap = new SKBitmap(new SKImageInfo(width, height)))
            {
                // Only compute pixels inside LEDs, due to scaling there may be some rounding issues but it's neglect-able
                foreach (var artemisLed in Layer.Leds)
                {
                    var xStart = artemisLed.AbsoluteRenderRectangle.Left / Scale;
                    var xEnd = artemisLed.AbsoluteRenderRectangle.Right / Scale;
                    var yStart = artemisLed.AbsoluteRenderRectangle.Top / Scale;
                    var yEnd = artemisLed.AbsoluteRenderRectangle.Bottom / Scale;

                    for (var x = xStart; x < xEnd; x++)
                    {
                        for (var y = yStart; y < yEnd; y++)
                        {
                            var v = _noise.Evaluate(Settings.XScale * x / width, Settings.YScale * y / height, _z);
                            bitmap.SetPixel((int) x, (int) y, new SKColor(0, 0, 0, (byte) ((v + 1) * 127)));
                        }
                    }
                }

                using (var sh = SKShader.CreateBitmap(bitmap, SKShaderTileMode.Mirror, SKShaderTileMode.Mirror, SKMatrix.MakeScale(Scale, Scale)))
                using (var paint = new SKPaint {Shader = sh})
                {
                    canvas.DrawPath(framePath, paint);
                }
            }
        }
    }
}