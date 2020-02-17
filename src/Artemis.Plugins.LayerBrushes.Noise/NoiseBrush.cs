using System;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Profile.LayerProperties;
using Artemis.Core.Plugins.LayerBrush;
using Artemis.Core.Services.Interfaces;
using Artemis.Plugins.LayerBrushes.Noise.Utilities;
using SkiaSharp;

namespace Artemis.Plugins.LayerBrushes.Noise
{
    public class NoiseBrush : LayerBrush
    {
        private static readonly Random Rand = new Random();
        private readonly OpenSimplexNoise _noise;
        private readonly IRgbService _rgbService;
        private SKBitmap _bitmap;

        private float _renderScale;
        private float _x;
        private float _y;
        private float _z;

        public NoiseBrush(Layer layer, LayerBrushDescriptor descriptor, IRgbService rgbService) : base(layer, descriptor)
        {
            _rgbService = rgbService;
            _x = Rand.Next(0, 4096);
            _y = Rand.Next(0, 4096);
            _z = Rand.Next(0, 4096);
            _noise = new OpenSimplexNoise(Rand.Next(0, 4096));

            MainColorProperty = RegisterLayerProperty<SKColor>("Brush.MainColor", "Main color", "The main color of the noise.");
            SecondaryColorProperty = RegisterLayerProperty<SKColor>("Brush.SecondaryColor", "Secondary color", "The secondary color of the noise.");
            ScaleProperty = RegisterLayerProperty<SKSize>("Brush.Scale", "Scale", "The scale of the noise.");
            HardnessProperty = RegisterLayerProperty<float>("Brush.Hardness", "Hardness", "The hardness of the noise, lower means there are gradients in the noise, higher means hard lines..");
            ScrollSpeedProperty = RegisterLayerProperty<SKPoint>("Brush.ScrollSpeed", "Movement speed", "The speed at which the noise moves vertically and horizontally.");
            AnimationSpeedProperty = RegisterLayerProperty<float>("Brush.AnimationSpeed", "Animation speed", "The speed at which the noise moves.");
            ScaleProperty.InputAffix = "%";

            DetermineRenderScale();
        }

        public LayerProperty<SKColor> MainColorProperty { get; set; }
        public LayerProperty<SKColor> SecondaryColorProperty { get; set; }
        public LayerProperty<SKSize> ScaleProperty { get; set; }
        public LayerProperty<float> HardnessProperty { get; set; }
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

            DetermineRenderScale();
            base.Update(deltaTime);
        }

        public override void Render(SKCanvas canvas, SKImageInfo canvasInfo, SKPath path, SKPaint paint)
        {
            var mainColor = MainColorProperty.CurrentValue;
            var scale = ScaleProperty.CurrentValue;
            var opacity = (float) Math.Round(mainColor.Alpha / 255.0, 2, MidpointRounding.AwayFromZero);
            var hardness = 127 + HardnessProperty.CurrentValue;

            // Scale down the render path to avoid computing a value for every pixel
            var width = (int) Math.Floor(path.Bounds.Width * _renderScale);
            var height = (int) Math.Floor(path.Bounds.Height * _renderScale);

            CreateBitmap(width, height);
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var scrolledX = x + _x;
                    var scrolledY = y + _y;
                    var evalX = 0.1 * scale.Width * scrolledX / width;
                    var evalY = 0.1 * scale.Height * scrolledY / height;
                    if (double.IsInfinity(evalX) || double.IsNaN(evalX) || double.IsNaN(evalY) || double.IsInfinity(evalY))
                        continue;

                    var v = _noise.Evaluate(evalX, evalY, _z);
                    var alpha = (byte) Math.Max(0, Math.Min(255, v * hardness));
                    _bitmap.SetPixel(x, y, new SKColor(mainColor.Red, mainColor.Green, mainColor.Blue, (byte) (alpha * opacity)));
                }
            }


            var bitmapTransform = SKMatrix.Concat(
                SKMatrix.MakeTranslation(path.Bounds.Left, path.Bounds.Top),
                SKMatrix.MakeScale(1f / _renderScale, 1f / _renderScale)
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


        private void DetermineRenderScale()
        {
            _renderScale = (float) (0.125f / _rgbService.RenderScale);
        }

        private void CreateBitmap(int width, int height)
        {
            if (_bitmap == null)
                _bitmap = new SKBitmap(new SKImageInfo(width, height));
            else if (_bitmap.Width != width || _bitmap.Height != height)
            {
                _bitmap.Dispose();
                _bitmap = new SKBitmap(new SKImageInfo(width, height));
            }
        }
    }
}