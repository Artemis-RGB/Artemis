using System;
using Artemis.Core.Models.Profile;
using Artemis.Core.Plugins.LayerElement;
using SkiaSharp;

namespace Artemis.Plugins.LayerElements.Noise
{
    public class NoiseLayerElement : LayerElement
    {
        private static Random _rand = new Random();
        private readonly OpenSimplexNoise _noise;
        private float _z;
        private const int Scale = 6;

        public NoiseLayerElement(Layer layer, Guid guid, NoiseLayerElementSettings settings, LayerElementDescriptor descriptor) : base(layer, guid, settings, descriptor)
        {
            Settings = settings;

            _z = _rand.Next(0, 4096);
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
            // Scale down the render path
            var width = (int) Math.Round(Math.Max(Layer.RenderRectangle.Width, Layer.RenderRectangle.Height) / Scale);
            var height = (int) Math.Round(Math.Max(Layer.RenderRectangle.Width, Layer.RenderRectangle.Height) / Scale);
            
            using (var bitmap = new SKBitmap(new SKImageInfo(width, height)))
            {
                for (var x = 0; x < width; x++)
                {
                    for (var y = 0; y < height; y++)
                    {
                        // This check is actually more expensive then _noise.Evaluate() ^.^'
                        // if (!framePath.Contains(x * Scale, y * Scale)) 
                        //    continue;

                        var v = _noise.Evaluate(Settings.XScale * x / width, Settings.YScale * y / height, _z);
                        bitmap.SetPixel(x, y, new SKColor(0, 0, 0, (byte) ((v + 1) * 127)));
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