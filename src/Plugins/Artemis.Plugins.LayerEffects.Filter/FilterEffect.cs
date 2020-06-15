using System;
using Artemis.Core.Plugins.LayerEffect.Abstract;
using Artemis.UI.Shared.Utilities;
using SkiaSharp;

namespace Artemis.Plugins.LayerEffects.Filter
{
    public class FilterEffect : LayerEffect<FilterEffectProperties>
    {
        public override void EnableLayerEffect()
        {
            Properties.FilterType.BaseValueChanged += (sender, args) => UpdateFilterType();

            UpdateFilterType();
        }

        private void UpdateFilterType()
        {
            if (HasBeenRenamed)
                return;

            Name = EnumUtilities.HumanizeValue(Properties.FilterType);
        }

        public override void DisableLayerEffect()
        {
        }

        public override void Update(double deltaTime)
        {
        }

        public override void PreProcess(SKCanvas canvas, SKImageInfo canvasInfo, SKPath path, SKPaint paint)
        {
            if (Properties.FilterType == FilterType.Blur)
            {
                paint.ImageFilter = SKImageFilter.CreateBlur(
                    Properties.BlurAmount.CurrentValue.Width,
                    Properties.BlurAmount.CurrentValue.Height,
                    paint.ImageFilter
                );
            }
            else if (Properties.FilterType == FilterType.Dilate)
            {
                paint.ImageFilter = SKImageFilter.CreateDilate(
                    (int) Properties.DilateRadius.CurrentValue.Width,
                    (int) Properties.DilateRadius.CurrentValue.Height,
                    paint.ImageFilter
                );
            }
            else if (Properties.FilterType == FilterType.Erode)
            {
                paint.ImageFilter = SKImageFilter.CreateErode(
                    (int) Properties.ErodeRadius.CurrentValue.Width,
                    (int) Properties.ErodeRadius.CurrentValue.Height,
                    paint.ImageFilter
                );
            }
            else if (Properties.FilterType == FilterType.Erode)
            {
                paint.ImageFilter = SKImageFilter.CreateErode(
                    (int) Properties.ErodeRadius.CurrentValue.Width,
                    (int) Properties.ErodeRadius.CurrentValue.Height,
                    paint.ImageFilter
                );
            }
            else if (Properties.FilterType == FilterType.DropShadow)
            {
                paint.ImageFilter = SKImageFilter.CreateDropShadow(
                    Properties.ShadowOffset.CurrentValue.X,
                    Properties.ShadowOffset.CurrentValue.Y,
                    Properties.ShadowBlurAmount.CurrentValue.Width,
                    Properties.ShadowBlurAmount.CurrentValue.Height, Properties.ShadowColor,
                    SKDropShadowImageFilterShadowMode.DrawShadowAndForeground,
                    paint.ImageFilter
                );
            }
        }

        public override void PostProcess(SKCanvas canvas, SKImageInfo canvasInfo, SKPath path, SKPaint paint)
        {
        }
    }
}