using System;
using System.ComponentModel;
using Artemis.Core.Models.Profile;
using Artemis.Core.Plugins.LayerBrush.Abstract;
using SkiaSharp;

namespace Artemis.Plugins.LayerBrushes.Color
{
    public class ColorBrush : LayerBrush<ColorBrushProperties>
    {
        private SKColor _color;
        private SKPaint _paint;
        private SKShader _shader;
        private SKRect _shaderBounds;

        public override void EnableLayerBrush()
        {
            Layer.RenderPropertiesUpdated += HandleShaderChange;
            Properties.GradientType.BaseValueChanged += HandleShaderChange;
            Properties.Color.BaseValueChanged += HandleShaderChange;
            Properties.GradientTileMode.BaseValueChanged += HandleShaderChange;
            Properties.GradientRepeat.BaseValueChanged += HandleShaderChange;
            Properties.Gradient.BaseValue.PropertyChanged += BaseValueOnPropertyChanged;
        }

        public override void DisableLayerBrush()
        {
            Layer.RenderPropertiesUpdated -= HandleShaderChange;
            Properties.GradientType.BaseValueChanged -= HandleShaderChange;
            Properties.Color.BaseValueChanged -= HandleShaderChange;
            Properties.GradientTileMode.BaseValueChanged -= HandleShaderChange;
            Properties.GradientRepeat.BaseValueChanged -= HandleShaderChange;
            Properties.Gradient.BaseValue.PropertyChanged -= BaseValueOnPropertyChanged;

            _paint?.Dispose();
            _shader?.Dispose();
            _paint = null;
            _shader = null;
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
            if (Layer.General.FillType.CurrentValue == LayerFillType.Clip)
            {
                var layerBounds = new SKRect(0, 0, Layer.Bounds.Width, Layer.Bounds.Height);
                if (layerBounds != _shaderBounds)
                {
                    _shaderBounds = layerBounds;
                    CreateShader();
                }
            }
            else
            {
                if (path.Bounds != _shaderBounds)
                {
                    _shaderBounds = path.Bounds;
                    CreateShader();
                }
            }

            paint.Shader = _shader;
            canvas.DrawPath(path, paint);
        }

        private void BaseValueOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            CreateShader();
        }

        private void HandleShaderChange(object? sender, EventArgs e)
        {
            CreateShader();
        }

        private void CreateShader()
        {
            var center = new SKPoint(_shaderBounds.MidX, _shaderBounds.MidY);
            var repeat = Properties.GradientRepeat.CurrentValue;
            var shader = Properties.GradientType.CurrentValue switch
            {
                GradientType.Solid => SKShader.CreateColor(_color),
                GradientType.LinearGradient => SKShader.CreateLinearGradient(
                    new SKPoint(_shaderBounds.Left, _shaderBounds.Top),
                    new SKPoint(_shaderBounds.Right, _shaderBounds.Top),
                    Properties.Gradient.BaseValue.GetColorsArray(repeat),
                    Properties.Gradient.BaseValue.GetPositionsArray(repeat),
                    Properties.GradientTileMode.CurrentValue),
                GradientType.RadialGradient => SKShader.CreateRadialGradient(
                    center,
                    Math.Max(_shaderBounds.Width, _shaderBounds.Height) / 2f,
                    Properties.Gradient.BaseValue.GetColorsArray(repeat),
                    Properties.Gradient.BaseValue.GetPositionsArray(repeat),
                    Properties.GradientTileMode.CurrentValue),
                GradientType.SweepGradient => SKShader.CreateSweepGradient(
                    center,
                    Properties.Gradient.BaseValue.GetColorsArray(repeat),
                    Properties.Gradient.BaseValue.GetPositionsArray(repeat),
                    Properties.GradientTileMode.CurrentValue,
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