using System;
using Artemis.Core.Models.Profile;
using Artemis.Core.Plugins.LayerBrush;
using Artemis.Plugins.LayerBrushes.Noise.Utilities;
using SkiaSharp;

namespace Artemis.Plugins.LayerBrushes.Noise
{
    public class NoiseBrush : LayerBrush
    {
        private const int Scale = 6;
        private static readonly Random Rand = new Random();
        private readonly OpenSimplexNoise _noise;
        private float _z;

        public NoiseBrush(Layer layer, NoiseBrushSettings settings, LayerBrushDescriptor descriptor) : base(layer, settings, descriptor)
        {
            Settings = settings;

            _z = Rand.Next(0, 4096);
            _noise = new OpenSimplexNoise(Rand.Next(0, 4096));
        }

        public new NoiseBrushSettings Settings { get; }

        public override void Update(double deltaTime)
        {
            // TODO: Come up with a better way to use deltaTime
            _z += Settings.AnimationSpeed / 500f / 0.04f * (float) deltaTime;

            if (_z >= float.MaxValue)
                _z = 0;

            base.Update(deltaTime);
        }

        public override LayerBrushViewModel GetViewModel()
        {
            return new NoiseBrushViewModel(this);
        }

        public override void Render(SKCanvas canvas, SKPath path, SKPaint paint)
        {
            // Scale down the render path to avoid computing a value for every pixel
            var width = (int) (Math.Max(path.Bounds.Width, path.Bounds.Height) / Scale);
            var height = (int) (Math.Max(path.Bounds.Width, path.Bounds.Height) / Scale);
            var opacity = (float) Math.Round(Settings.Color.Alpha / 255.0, 2, MidpointRounding.AwayFromZero);
            using (var bitmap = new SKBitmap(new SKImageInfo(width, height)))
            {
                bitmap.Erase(new SKColor(0, 0, 0, 0));
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
                            var alpha = (byte) ((v + 1) * 127 * opacity);
                            // There's some fun stuff we can do here, like creating hard lines
                            // if (alpha > 128)
                            //     alpha = 255;
                            // else
                            //     alpha = 0;
                            var color = new SKColor(Settings.Color.Red, Settings.Color.Green, Settings.Color.Blue, alpha);
                            bitmap.SetPixel((int) x, (int) y, color);
                        }
                    }
                }

                using (var sh = SKShader.CreateBitmap(bitmap, SKShaderTileMode.Mirror, SKShaderTileMode.Mirror, SKMatrix.MakeScale(Scale, Scale)))
                {
                    paint.Shader = sh;
                    canvas.DrawPath(Layer.LayerShape.Path, paint);
                }
            }
        }
    }
}