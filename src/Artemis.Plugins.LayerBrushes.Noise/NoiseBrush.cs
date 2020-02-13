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
        private readonly OpenSimplexNoise _noise;
        private float _z;

        public NoiseBrush(Layer layer, LayerBrushDescriptor descriptor) : base(layer, descriptor)
        {
            MainColorProperty = RegisterLayerProperty<SKColor>("Brush.MainColor", "Main color", "The main color of the noise.");
            SecondaryColorProperty = RegisterLayerProperty<SKColor>("Brush.SecondaryColor", "Secondary color", "The secondary color of the noise.");
            ScaleProperty = RegisterLayerProperty<SKSize>("Brush.Scale", "Scale", "The scale of the noise.");
            AnimationSpeedProperty = RegisterLayerProperty<float>("Brush.AnimationSpeed", "Animation speed", "The speed at which the noise moves.");
            ScaleProperty.InputAffix = "%";

            _z = Rand.Next(0, 4096);
            _noise = new OpenSimplexNoise(Rand.Next(0, 4096));
        }

        public LayerProperty<SKColor> MainColorProperty { get; set; }
        public LayerProperty<SKColor> SecondaryColorProperty { get; set; }
        public LayerProperty<SKSize> ScaleProperty { get; set; }
        public LayerProperty<float> AnimationSpeedProperty { get; set; }

        public override void Update(double deltaTime)
        {
            // TODO: Come up with a better way to use deltaTime
            _z += AnimationSpeedProperty.CurrentValue / 500f / 0.04f * (float) deltaTime;

            if (_z >= float.MaxValue)
                _z = 0;

            base.Update(deltaTime);
        }

        public override void Render(SKCanvas canvas, SKPath path, SKPaint paint)
        {
            var mainColor = MainColorProperty.CurrentValue;
            var horizontalScale = ScaleProperty.CurrentValue.Width / 400f;
            var verticalScale = ScaleProperty.CurrentValue.Height / 400f;

            // Scale down the render path to avoid computing a value for every pixel
            var width = (int) (path.Bounds.Width * horizontalScale);
            var height = (int) (path.Bounds.Height * verticalScale);

            var opacity = (float) Math.Round(mainColor.Alpha / 255.0, 2, MidpointRounding.AwayFromZero);
            using (var bitmap = new SKBitmap(new SKImageInfo(width, height)))
            {
                bitmap.Erase(SKColor.Empty);
                for (var x = 0; x < width; x++)
                {
                    for (var y = 0; y < height; y++)
                    {
                        var v = _noise.Evaluate(1f / horizontalScale * x, 1f / verticalScale * y, _z);
                        var alpha = (byte) ((v + 1) * 127 * opacity);
                        // There's some fun stuff we can do here, like creating hard lines
//                        if (alpha > 128)
//                            alpha = 255;
//                        else
//                            alpha = 0;
                        bitmap.SetPixel(x, y, new SKColor(mainColor.Red, mainColor.Green, mainColor.Blue, alpha));
                    }
                }


                var makeTranslation = SKMatrix.MakeTranslation(path.Bounds.Left, path.Bounds.Top);
                SKMatrix.Concat(ref makeTranslation, makeTranslation, SKMatrix.MakeScale(1f / horizontalScale, 1f / verticalScale));
                using (var sh = SKShader.CreateBitmap(bitmap, SKShaderTileMode.Mirror, SKShaderTileMode.Mirror, makeTranslation))
                {
                    paint.FilterQuality = SKFilterQuality.Low;

                    paint.Shader = SKShader.CreateColor(SecondaryColorProperty.CurrentValue);
                    canvas.DrawPath(path, paint);
                    paint.Shader = sh;
                    canvas.DrawPath(path, paint);
                }
            }
        }
    }
}