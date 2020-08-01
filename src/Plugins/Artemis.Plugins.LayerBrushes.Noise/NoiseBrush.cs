using System;
using System.ComponentModel;
using Artemis.Core.Extensions;
using Artemis.Core.Models.Surface;
using Artemis.Core.Plugins.LayerBrush.Abstract;
using Artemis.Core.Services.Interfaces;
using Artemis.Plugins.LayerBrushes.Noise.Utilities;
using SkiaSharp;

namespace Artemis.Plugins.LayerBrushes.Noise
{
    public class NoiseBrush : PerLedLayerBrush<NoiseBrushProperties>
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

        // public override void Render(SKCanvas canvas, SKImageInfo canvasInfo, SKPath path, SKPaint paint)
        // {
        //     var mainColor = Properties.MainColor.CurrentValue;
        //     var secondColor = Properties.SecondaryColor.CurrentValue;
        //     var gradientColor = Properties.GradientColor.CurrentValue;
        //     var scale = Properties.Scale.CurrentValue;
        //     var hardness = Properties.Hardness.CurrentValue / 100f;
        //
        //     // Scale down the render path to avoid computing a value for every pixel
        //     var width = (int) Math.Floor(path.Bounds.Width * _renderScale);
        //     var height = (int) Math.Floor(path.Bounds.Height * _renderScale);
        //
        //     // This lil' snippet renders per LED, it's neater but doesn't support translations
        //     Layer.ExcludeCanvasFromTranslation(canvas);
        //
        //     var shapePath = new SKPath(Layer.LayerShape.Path);
        //     Layer.IncludePathInTranslation(shapePath);
        //     canvas.ClipPath(shapePath);
        //
        //     using var ledPaint = new SKPaint();
        //     foreach (var artemisLed in Layer.Leds)
        //     {
        //         var ledRectangle = SKRect.Create(
        //             artemisLed.AbsoluteRenderRectangle.Left - Layer.Bounds.Left,
        //             artemisLed.AbsoluteRenderRectangle.Top - Layer.Bounds.Top,
        //             artemisLed.AbsoluteRenderRectangle.Width,
        //             artemisLed.AbsoluteRenderRectangle.Height
        //         );
        //
        //         var scrolledX = ledRectangle.MidX + _x;
        //         if (float.IsNaN(scrolledX))
        //             scrolledX = 0;
        //         var scrolledY = ledRectangle.MidY + _y;
        //         if (float.IsNaN(scrolledY))
        //             scrolledY = 0;
        //
        //
        //         var evalPath = new SKPath();
        //         evalPath.AddPoly(new[] {new SKPoint(0, 0), new SKPoint(scrolledX, scrolledY)});
        //         Layer.ExcludePathFromTranslation(evalPath);
        //
        //         if (evalPath.Bounds.IsEmpty)
        //             continue;
        //
        //         scrolledX = evalPath.Points[1].X;
        //         scrolledY = evalPath.Points[1].Y;
        //
        //         var evalX = 0.1 * scale.Width * scrolledX / width;
        //         var evalY = 0.1 * scale.Height * scrolledY / height;
        //
        //         var v = (float) _noise.Evaluate(evalX, evalY, _z) * hardness;
        //         var amount = Math.Max(0f, Math.Min(1f, v));
        //
        //         if (Properties.ColorType.BaseValue == ColorMappingType.Simple)
        //             ledPaint.Color = mainColor.Interpolate(secondColor, amount);
        //         else if (gradientColor != null && _colorMap.Length == 101)
        //             ledPaint.Color = _colorMap[(int) Math.Round(amount * 100, MidpointRounding.AwayFromZero)];
        //         else
        //             ledPaint.Color = SKColor.Empty;
        //
        //         canvas.DrawRect(ledRectangle, ledPaint);
        //     }
        // }

        private void GradientColorChanged(object sender, PropertyChangedEventArgs e)
        {
            CreateColorMap();
        }


        private void DetermineRenderScale()
        {
            _renderScale = (float) (0.25f / _rgbService.RenderScale);
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

        public override SKColor GetColor(ArtemisLed led, SKPoint renderPoint)
        {
            var mainColor = Properties.MainColor.CurrentValue;
            var secondColor = Properties.SecondaryColor.CurrentValue;
            var gradientColor = Properties.GradientColor.CurrentValue;
            var scale = Properties.Scale.CurrentValue;
            var hardness = Properties.Hardness.CurrentValue / 100f;

            var scrolledX = renderPoint.X + _x;
            if (float.IsNaN(scrolledX))
                scrolledX = 0;
            var scrolledY = renderPoint.Y + _y;
            if (float.IsNaN(scrolledY))
                scrolledY = 0;


            var evalX = scrolledX * (scale.Width *-1) / 1000f;
            var evalY = scrolledY * (scale.Height*-1) / 1000f;

            var v = (float) _noise.Evaluate(evalX, evalY, _z) * hardness;
            var amount = Math.Max(0f, Math.Min(1f, v));

            if (Properties.ColorType.BaseValue == ColorMappingType.Simple)
                return mainColor.Interpolate(secondColor, amount);
            else if (gradientColor != null && _colorMap.Length == 101)
                return _colorMap[(int) Math.Round(amount * 100, MidpointRounding.AwayFromZero)];
            else
                return SKColor.Empty;
        }
    }

    public enum ColorMappingType
    {
        Simple,
        Gradient
    }
}