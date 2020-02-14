using System;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Profile.LayerProperties;
using Artemis.Core.Plugins.LayerBrush;
using Artemis.Plugins.LayerBrushes.Noise.Utilities;
using SkiaSharp;

namespace Artemis.Plugins.LayerBrushes.Noise
{
    public class NoiseBrush : LayerBrush
    {
        private static readonly Random Rand = new Random();

        private float _renderScale;
        private readonly OpenSimplexNoise _noise;
        private float _x;
        private float _y;
        private float _z;
        private SKBitmap _bitmap;

        public NoiseBrush(Layer layer, LayerBrushDescriptor descriptor) : base(layer, descriptor)
        {
            MainColorProperty = RegisterLayerProperty<SKColor>("Brush.MainColor", "Main color", "The main color of the noise.");
            SecondaryColorProperty = RegisterLayerProperty<SKColor>("Brush.SecondaryColor", "Secondary color", "The secondary color of the noise.");
            ScaleProperty = RegisterLayerProperty<SKSize>("Brush.Scale", "Scale", "The scale of the noise.");
            ScrollSpeedProperty = RegisterLayerProperty<SKPoint>("Brush.ScrollSpeed", "Movement speed", "The speed at which the noise moves vertically and horizontally.");
            AnimationSpeedProperty = RegisterLayerProperty<float>("Brush.AnimationSpeed", "Animation speed", "The speed at which the noise moves.");
            ScaleProperty.InputAffix = "%";

            _x = Rand.Next(0, 4096);
            _y = Rand.Next(0, 4096);
            _z = Rand.Next(0, 4096);
            _noise = new OpenSimplexNoise(Rand.Next(0, 4096));
        }

        public LayerProperty<SKColor> MainColorProperty { get; set; }
        public LayerProperty<SKColor> SecondaryColorProperty { get; set; }
        public LayerProperty<SKSize> ScaleProperty { get; set; }
        public LayerProperty<SKPoint> ScrollSpeedProperty { get; set; }
        public LayerProperty<float> AnimationSpeedProperty { get; set; }

        public override void Update(double deltaTime)
        {
            _x += ScrollSpeedProperty.CurrentValue.X / 500f / (float) deltaTime;
            _y += ScrollSpeedProperty.CurrentValue.Y / 500f / (float) deltaTime;
            _z += AnimationSpeedProperty.CurrentValue / 500f / 0.04f * (float) deltaTime;

            // A telltale sign of someone who can't do math very well
            if (float.IsPositiveInfinity(_x) || float.IsNegativeInfinity(_x) || float.IsNaN(_x))
                _x = 0;
            if (float.IsPositiveInfinity(_y) || float.IsNegativeInfinity(_y) || float.IsNaN(_y))
                _y = 0;
            if (float.IsPositiveInfinity(_z) || float.IsNegativeInfinity(_z) || float.IsNaN(_z))
                _z = 0;
            base.Update(deltaTime);
        }

        public override void Render(SKCanvas canvas, SKPath path, SKPaint paint)
        {
            var mainColor = MainColorProperty.CurrentValue;
            var scale = ScaleProperty.CurrentValue;
            // Scale down the render path to avoid computing a value for every pixel
            var width = Math.Floor(path.Bounds.Width * RenderScale);
            var height = Math.Floor(path.Bounds.Height * RenderScale);

            CreateBitmap((int) width, (int) height);
            var opacity = (float) Math.Round(mainColor.Alpha / 255.0, 2, MidpointRounding.AwayFromZero);

            _bitmap.Erase(SKColor.Empty);
            for (var x = 0; x < width; x++)
            {
                var scrolledX = x + _x;
                for (var y = 0; y < height; y++)
                {
                    var scrolledY = y + _y;
                    var v = _noise.Evaluate(0.1f * scale.Width * scrolledX / width, 0.1f * scale.Height * scrolledY / height, _z);
                    var alpha = (byte) Math.Max(0, Math.Min(255, v * 1024));
                    _bitmap.SetPixel(x, y, new SKColor(mainColor.Red, mainColor.Green, mainColor.Blue, (byte) (alpha * opacity)));
                }
            }

            var bitmapTransform = SKMatrix.Concat(
                SKMatrix.MakeTranslation(path.Bounds.Left, path.Bounds.Top),
                SKMatrix.MakeScale(1f / RenderScale, 1f / RenderScale)
            );
            using (var backgroundShader = SKShader.CreateColor(SecondaryColorProperty.CurrentValue))
            using (var foregroundShader = SKShader.CreateBitmap(_bitmap, SKShaderTileMode.Clamp, SKShaderTileMode.Clamp, bitmapTransform))
            {
                canvas.ClipPath(path);
                paint.Shader = backgroundShader;
                canvas.DrawRect(path.Bounds, paint);
                paint.Shader = foregroundShader;
                canvas.DrawRect(path.Bounds, paint);
            }
        }

        private void CreateBitmap(int width, int height)
        {
            if (_bitmap == null)
            {
                _bitmap = new SKBitmap(new SKImageInfo(width, height));
            }
            else if (_bitmap.Width != width || _bitmap.Height != height)
            {
                _bitmap.Dispose();
                _bitmap = new SKBitmap(new SKImageInfo(width, height));
            }
        }
    }
}