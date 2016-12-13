using System;
using Artemis.Profiles.Layers.Types.AmbientLight.Model;
using Artemis.Profiles.Layers.Types.AmbientLight.Model.Extensions;

namespace Artemis.Profiles.Layers.Types.AmbientLight.AmbienceCreator
{
    public class AmbienceCreatorExtend : IAmbienceCreator
    {
        #region Methods

        public byte[] GetAmbience(byte[] pixels, int sourceWidth, int sourceHeight,
            int targetWidth, int targetHeight,
            AmbientLightPropertiesModel settings)
        {
            var colors = new AvgColor[targetWidth];
            for (var i = 0; i < colors.Length; i++)
                colors[i] = new AvgColor();

            var offsetLeft = settings.OffsetLeft + (settings.BlackBarDetectionMode.HasFlag(BlackBarDetectionMode.Left)
                ? pixels.DetectBlackBarLeft(sourceWidth, sourceHeight, settings.OffsetLeft, settings.OffsetRight,
                    settings.OffsetTop, settings.OffsetBottom)
                : 0);
            var offsetRight = settings.OffsetRight +
                              (settings.BlackBarDetectionMode.HasFlag(BlackBarDetectionMode.Right)
                                  ? pixels.DetectBlackBarRight(sourceWidth, sourceHeight, settings.OffsetLeft,
                                      settings.OffsetRight, settings.OffsetTop, settings.OffsetBottom)
                                  : 0);
            var offsetTop = settings.OffsetTop + (settings.BlackBarDetectionMode.HasFlag(BlackBarDetectionMode.Top)
                ? pixels.DetectBlackBarTop(sourceWidth, sourceHeight, settings.OffsetLeft, settings.OffsetRight,
                    settings.OffsetTop, settings.OffsetBottom)
                : 0);
            var offsetBottom = settings.OffsetBottom +
                               (settings.BlackBarDetectionMode.HasFlag(BlackBarDetectionMode.Bottom)
                                   ? pixels.DetectBlackBarBottom(sourceWidth, sourceHeight, settings.OffsetLeft,
                                       settings.OffsetRight, settings.OffsetTop, settings.OffsetBottom)
                                   : 0);

            var effectiveSourceWidth = sourceWidth - offsetLeft - offsetRight;
            var effectiveSourceHeight = sourceHeight - offsetTop - offsetBottom;

            var relevantSourceHeight = (int) Math.Round(effectiveSourceHeight*(settings.MirroredAmount/100.0));
            var relevantOffsetTop = sourceHeight - offsetBottom - relevantSourceHeight;

            var widthPixels = effectiveSourceWidth/(double) targetWidth;
            var heightPixels = relevantSourceHeight/(double) targetHeight;

            if (widthPixels <= 0 || heightPixels <= 0 || (relevantSourceHeight + relevantOffsetTop > sourceHeight) ||
                effectiveSourceWidth > sourceWidth)
            {
                colors = colors.ExtendHeight(targetHeight);
                return colors.ToBGRArray();
            }

            var increment = Math.Max(1, Math.Min(20, settings.Downsampling));
            for (var y = 0; y < relevantSourceHeight; y += increment)
            {
                var targetWidthIndex = 0;
                var widthCounter = widthPixels;

                for (var x = 0; x < effectiveSourceWidth; x += increment)
                {
                    if (x >= widthCounter)
                    {
                        widthCounter += widthPixels;
                        targetWidthIndex++;
                    }

                    var colorsOffset = targetWidthIndex;
                    var sourceOffset = ((relevantOffsetTop + y)*sourceWidth + offsetLeft + x)*4;

                    var color = colors[colorsOffset];
                    color.AddB(pixels[sourceOffset]);
                    color.AddG(pixels[sourceOffset + 1]);
                    color.AddR(pixels[sourceOffset + 2]);
                }
            }

            colors = colors.Flip(targetWidth, settings.FlipMode);
            colors = colors.ExtendHeight(targetHeight);
            return colors.ToBGRArray();
        }

        #endregion
    }
}