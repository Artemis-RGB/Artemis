using System;
using Artemis.Core.Plugins.LayerEffect.Abstract;
using SkiaSharp;

namespace Artemis.Plugins.LayerEffects.Filter
{
    public class BlurEffect : LayerEffect<BlurEffectProperties>
    {
        private double _lastWidth;
        private double _lastHeight;
        private SKImageFilter _imageFilter;

        public override void EnableLayerEffect()
        {
        }

        public override void DisableLayerEffect()
        {
        }

        public override void Update(double deltaTime)
        {
            if (Math.Abs(Properties.BlurAmount.CurrentValue.Width - _lastWidth) > 0.01 || Math.Abs(Properties.BlurAmount.CurrentValue.Height - _lastHeight) > 0.01)
            {
                if (Properties.BlurAmount.CurrentValue.Width <= 0 && Properties.BlurAmount.CurrentValue.Height <= 0)
                    _imageFilter = null;
                else
                {
                    _imageFilter = SKImageFilter.CreateBlur(
                        Properties.BlurAmount.CurrentValue.Width,
                        Properties.BlurAmount.CurrentValue.Height
                    );
                }

                _lastWidth = Properties.BlurAmount.CurrentValue.Width;
                _lastHeight = Properties.BlurAmount.CurrentValue.Height;
            }
        }

        public override void PreProcess(SKCanvas canvas, SKImageInfo canvasInfo, SKPath renderBounds, SKPaint paint)
        {
          
        }

        public override void PostProcess(SKCanvas canvas, SKImageInfo canvasInfo, SKPath renderBounds, SKPaint paint)
        {
            if (_imageFilter != null)
                paint.ImageFilter = SKImageFilter.CreateMerge(paint.ImageFilter, _imageFilter);
        }

        private void UpdateFilterType()
        {
        }
    }
}