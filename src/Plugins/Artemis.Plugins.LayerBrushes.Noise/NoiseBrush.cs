using System;
using System.ComponentModel;
using Artemis.Core.Extensions;
using Artemis.Core.Plugins.LayerBrush.Abstract;
using Artemis.Core.Services.Interfaces;
using Artemis.Plugins.LayerBrushes.Noise.Utilities;
using SkiaSharp;

namespace Artemis.Plugins.LayerBrushes.Noise
{
    public class NoiseBrush : LayerBrush<NoiseBrushProperties>
    {
        private static readonly Random Rand = new Random();
        private readonly IRgbService _rgbService;
        private SKBitmap _bitmap;
        private SKColor[] _colorMap;
        private OpenSimplexNoise _noise;
        private float _renderScale;
        private float _x;
        private float _y;
        private float _z;

        public NoiseBrush(IRgbService rgbService)
        {
            _rgbService = rgbService;
        }

        public override void EnableLayerBrush()
        {
            _x = Rand.Next(0, 4096);
            _y = Rand.Next(0, 4096);
            _z = Rand.Next(0, 4096);
            _noise = new OpenSimplexNoise(Rand.Next(0, 4096));

            Properties.GradientColor.BaseValue.PropertyChanged += GradientColorChanged;
            CreateColorMap();
            DetermineRenderScale();
        }

        public override void DisableLayerBrush()
        {
            _bitmap?.Dispose();
            _bitmap = null;
        }

        public override void Update(double deltaTime)
        {
            _x += Properties.ScrollSpeed.CurrentValue.X / 500f / (float) deltaTime;
            _y += Properties.ScrollSpeed.CurrentValue.Y / 500f / (float) deltaTime;
            _z += Properties.AnimationSpeed.CurrentValue / 500f / 0.04f * (float) deltaTime;

            // A telltale sign of someone who can't do math very well
            if (float.IsPositiveInfinity(_x) || float.IsNegativeInfinity(_x) || float.IsNaN(_x))
                _x = 0;
            if (float.IsPositiveInfinity(_y) || float.IsNegativeInfinity(_y) || float.IsNaN(_y))
                _y = 0;
            if (float.IsPositiveInfinity(_z) || float.IsNegativeInfinity(_z) || float.IsNaN(_z))
                _z = 0;

            DetermineRenderScale();
        }

        public override void Render(SKCanvas canvas, SKImageInfo canvasInfo, SKPath path, SKPaint paint)
        {
            var mainColor = Properties.MainColor.CurrentValue;
            var secondColor = Properties.SecondaryColor.CurrentValue;
            var gradientColor = Properties.GradientColor.CurrentValue;
            var scale = Properties.Scale.CurrentValue;
            var hardness = Properties.Hardness.CurrentValue / 100f;

            // Scale down the render path to avoid computing a value for every pixel
            var width = (int) Math.Floor(path.Bounds.Width * _renderScale);
            var height = (int) Math.Floor(path.Bounds.Height * _renderScale);

            CreateBitmap(width, height);

            var clipPath = new SKPath(Layer.Path);
            Layer.ExcludePathFromTranslation(clipPath);
            clipPath = new SKPath(clipPath);
            clipPath.Transform(SKMatrix.MakeTranslation(Layer.Path.Bounds.Left * -1, Layer.Path.Bounds.Top * -1));

            // Fill a canvas matching the final area that will be rendered
            using var bitmapCanvas = new SKCanvas(_bitmap);
            using var clipPaint = new SKPaint {Color = new SKColor(0, 0, 0, 255)};
            bitmapCanvas.Clear();
            bitmapCanvas.Scale(_renderScale);
            bitmapCanvas.DrawPath(clipPath, clipPaint);

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

                    var pixel = _bitmap.GetPixel(x, y);
                    if (pixel.Alpha != 255)
                        continue;

                    var v = (float) _noise.Evaluate(evalX, evalY, _z) * hardness;
                    var amount = Math.Max(0f, Math.Min(1f, v));
                    if (Properties.ColorType.BaseValue == ColorMappingType.Simple)
                        _bitmap.SetPixel(x, y, mainColor.Interpolate(secondColor, amount));
                    else if (gradientColor != null && _colorMap.Length == 101)
                    {
                        var color = _colorMap[(int) Math.Round(amount * 100, MidpointRounding.AwayFromZero)];
                        _bitmap.SetPixel(x, y, color);
                    }
                }
            }

            var bitmapTransform = SKMatrix.Concat(
                SKMatrix.MakeTranslation(path.Bounds.Left, path.Bounds.Top),
                SKMatrix.MakeScale(1f / _renderScale, 1f / _renderScale)
            );

            if (Properties.ColorType.BaseValue == ColorMappingType.Simple)
                paint.Color = Properties.SecondaryColor.CurrentValue;

            using var foregroundShader = SKShader.CreateBitmap(_bitmap, SKShaderTileMode.Clamp, SKShaderTileMode.Clamp, bitmapTransform);
            paint.Shader = foregroundShader;
            canvas.DrawPath(path, paint);
        }

        private void GradientColorChanged(object sender, PropertyChangedEventArgs e)
        {
            CreateColorMap();
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

        private void CreateColorMap()
        {
            var colorMap = new SKColor[101];
            for (var i = 0; i < 101; i++)
                colorMap[i] = Properties.GradientColor.BaseValue.GetColor(i / 100f);

            _colorMap = colorMap;
        }
    }

    public enum ColorMappingType
    {
        Simple,
        Gradient
    }
}