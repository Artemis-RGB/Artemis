using System;
using Artemis.Core.Models.Profile;
using Artemis.Core.Plugins.LayerBrush;
using SkiaSharp;

namespace Artemis.Plugins.LayerBrushes.Color
{
    public class ColorBrush : LayerBrush<ColorBrushProperties>
    {
        private SKColor _color;
        private SKPaint _paint;
        private SKShader _shader;
        private SKRect _shaderBounds;

        public ColorBrush(Layer layer, LayerBrushDescriptor descriptor) : base(layer, descriptor)
        {
            Layer.RenderPropertiesUpdated += (sender, args) => CreateShader();
        }

        public override void Update(double deltaTime)
        {
            // Only check if a solid is being drawn, because that can be changed by keyframes
            if (Properties.GradientType.BaseValue == GradientType.Solid && _color != Properties.Color.CurrentValue)
            {
                // If the color was changed since the last frame, recreate the shader
                _color = Properties.Color.CurrentValue;
                CreateShader();
            }
        }

        public override void Render(SKCanvas canvas, SKImageInfo canvasInfo, SKPath path, SKPaint paint)
        {
            if (path.Bounds != _shaderBounds)
            {
                _shaderBounds = path.Bounds;
                CreateShader();
            }

            paint.Shader = _shader;
            canvas.DrawPath(path, paint);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _paint?.Dispose();
                _shader?.Dispose();
            }

            base.Dispose(disposing);
        }
        
        protected override void OnPropertiesInitialized()
        {
            Properties.GradientType.BaseValueChanged += (sender, args) => CreateShader();
            Properties.Color.BaseValueChanged += (sender, args) => CreateShader();
            Properties.Gradient.BaseValue.PropertyChanged += (sender, args) => CreateShader();
        }

        private void CreateShader()
        {
            var center = new SKPoint(_shaderBounds.MidX, _shaderBounds.MidY);
            var shader = Properties.GradientType.CurrentValue switch
            {
                GradientType.Solid => SKShader.CreateColor(_color),
                GradientType.LinearGradient => SKShader.CreateLinearGradient(
                    new SKPoint(_shaderBounds.Left, _shaderBounds.Top),
                    new SKPoint(_shaderBounds.Right, _shaderBounds.Top),
                    Properties.Gradient.BaseValue.GetColorsArray(),
                    Properties.Gradient.BaseValue.GetPositionsArray(),
                    SKShaderTileMode.Repeat),
                GradientType.RadialGradient => SKShader.CreateRadialGradient(
                    center,
                    Math.Min(_shaderBounds.Width, _shaderBounds.Height),
                    Properties.Gradient.BaseValue.GetColorsArray(),
                    Properties.Gradient.BaseValue.GetPositionsArray(),
                    SKShaderTileMode.Repeat),
                GradientType.SweepGradient => SKShader.CreateSweepGradient(
                    center,
                    Properties.Gradient.BaseValue.GetColorsArray(),
                    Properties.Gradient.BaseValue.GetPositionsArray(),
                    SKShaderTileMode.Clamp,
                    0,
                    360),
                _ => throw new ArgumentOutOfRangeException()
            };

            var oldShader = _shader;
            var oldPaint = _paint;
            _shader = shader;
            _paint = new SKPaint {Shader = _shader, FilterQuality = SKFilterQuality.Low};
            oldShader?.Dispose();
            oldPaint?.Dispose();
        }
    }
}