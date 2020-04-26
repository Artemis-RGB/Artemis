using System;
using System.ComponentModel;
using System.Linq;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Profile.Colors;
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
        private SKColor[] _colorMap;

        public NoiseBrush(Layer layer, LayerBrushDescriptor descriptor, IRgbService rgbService) : base(layer, descriptor)
        {
            _rgbService = rgbService;
            _x = Rand.Next(0, 4096);
            _y = Rand.Next(0, 4096);
            _z = Rand.Next(0, 4096);
            _noise = new OpenSimplexNoise(Rand.Next(0, 4096));
            DetermineRenderScale();

            ColorTypeProperty = RegisterLayerProperty("Brush.ColorType", "Color mapping type", "The way the noise is converted to colors", ColorMappingType.Simple);
            ColorTypeProperty.CanUseKeyframes = false;
            ColorTypeProperty.ValueChanged += (sender, args) => UpdateColorProperties();
            ScaleProperty = RegisterLayerProperty("Brush.Scale", "Scale", "The scale of the noise.", new SKSize(100, 100));
            ScaleProperty.MinInputValue = 0f;
            HardnessProperty = RegisterLayerProperty("Brush.Hardness", "Hardness", "The hardness of the noise, lower means there are gradients in the noise, higher means hard lines", 500f);
            HardnessProperty.MinInputValue = 0f;
            HardnessProperty.MaxInputValue = 2048f;
            ScrollSpeedProperty = RegisterLayerProperty<SKPoint>("Brush.ScrollSpeed", "Movement speed", "The speed at which the noise moves vertically and horizontally");
            ScrollSpeedProperty.MinInputValue = -64f;
            ScrollSpeedProperty.MaxInputValue = 64f;
            AnimationSpeedProperty = RegisterLayerProperty("Brush.AnimationSpeed", "Animation speed", "The speed at which the noise moves", 25f);
            AnimationSpeedProperty.MinInputValue = 0f;
            AnimationSpeedProperty.MaxInputValue = 64f;
            ScaleProperty.InputAffix = "%";
            MainColorProperty = RegisterLayerProperty("Brush.MainColor", "Main color", "The main color of the noise", new SKColor(255, 0, 0));
            SecondaryColorProperty = RegisterLayerProperty("Brush.SecondaryColor", "Secondary color", "The secondary color of the noise", new SKColor(0, 0, 255));
            GradientColorProperty = RegisterLayerProperty("Brush.GradientColor", "Noise gradient map", "The gradient the noise will map it's value to", new ColorGradient());
            GradientColorProperty.CanUseKeyframes = false;
            if (!GradientColorProperty.Value.Stops.Any())
                GradientColorProperty.Value.MakeFabulous();
            GradientColorProperty.Value.PropertyChanged += CreateColorMap;

            UpdateColorProperties();
            CreateColorMap(null, null);
        }


        public LayerProperty<ColorMappingType> ColorTypeProperty { get; set; }
        public LayerProperty<SKColor> MainColorProperty { get; set; }
        public LayerProperty<SKColor> SecondaryColorProperty { get; set; }
        public LayerProperty<ColorGradient> GradientColorProperty { get; set; }

        public LayerProperty<SKSize> ScaleProperty { get; set; }
        public LayerProperty<float> HardnessProperty { get; set; }
        public LayerProperty<SKPoint> ScrollSpeedProperty { get; set; }
        public LayerProperty<float> AnimationSpeedProperty { get; set; }

        private void UpdateColorProperties()
        {
            GradientColorProperty.IsHidden = ColorTypeProperty.Value != ColorMappingType.Gradient;
            MainColorProperty.IsHidden = ColorTypeProperty.Value != ColorMappingType.Simple;
            SecondaryColorProperty.IsHidden = ColorTypeProperty.Value != ColorMappingType.Simple;
        }

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
            var mainColor = MainColorProperty?.CurrentValue;
            var gradientColor = GradientColorProperty?.CurrentValue;
            var scale = ScaleProperty.CurrentValue;
            var opacity = mainColor != null ? (float) Math.Round(mainColor.Value.Alpha / 255.0, 2, MidpointRounding.AwayFromZero) : 0;
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
                    if (ColorTypeProperty.Value == ColorMappingType.Simple && mainColor != null)
                    {
                        _bitmap.SetPixel(x, y, new SKColor(mainColor.Value.Red, mainColor.Value.Green, mainColor.Value.Blue, (byte) (alpha * opacity)));
                    }
                    else if (gradientColor != null && _colorMap.Length == 101)
                    {
                        var color = _colorMap[(int) Math.Round(alpha / 255f * 100, MidpointRounding.AwayFromZero)];
                        _bitmap.SetPixel(x, y, color);
                    }
                }
            }


            var bitmapTransform = SKMatrix.Concat(
                SKMatrix.MakeTranslation(path.Bounds.Left, path.Bounds.Top),
                SKMatrix.MakeScale(1f / _renderScale, 1f / _renderScale)
            );

            canvas.ClipPath(path);
            if (ColorTypeProperty.Value == ColorMappingType.Simple)
            {
                using var backgroundShader = SKShader.CreateColor(SecondaryColorProperty.CurrentValue);
                paint.Shader = backgroundShader;
                canvas.DrawRect(path.Bounds, paint);
            }

            using var foregroundShader = SKShader.CreateBitmap(_bitmap, SKShaderTileMode.Clamp, SKShaderTileMode.Clamp, bitmapTransform);
            paint.Shader = foregroundShader;
            canvas.DrawRect(path.Bounds, paint);
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

        private void CreateColorMap(object sender, EventArgs e)
        {
            var colorMap = new SKColor[101];
            for (var i = 0; i < 101; i++)
                colorMap[i] = GradientColorProperty.Value.GetColor(i / 100f);

            _colorMap = colorMap;
        }
    }

    public enum ColorMappingType
    {
        Simple,
        Gradient
    }
}