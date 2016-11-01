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
            AvgColor[] colors = new AvgColor[targetWidth];
            for (int i = 0; i < colors.Length; i++)
                colors[i] = new AvgColor();

            int offsetLeft = settings.OffsetLeft + (settings.BlackBarDetectionMode.HasFlag(BlackBarDetectionMode.Left)
                ? pixels.DetectBlackBarLeft(sourceWidth, sourceHeight, settings.OffsetLeft, settings.OffsetRight, settings.OffsetTop, settings.OffsetBottom)
                : 0);
            int offsetRight = settings.OffsetRight + (settings.BlackBarDetectionMode.HasFlag(BlackBarDetectionMode.Right)
                ? pixels.DetectBlackBarRight(sourceWidth, sourceHeight, settings.OffsetLeft, settings.OffsetRight, settings.OffsetTop, settings.OffsetBottom)
                : 0);
            int offsetTop = settings.OffsetTop + (settings.BlackBarDetectionMode.HasFlag(BlackBarDetectionMode.Top)
                ? pixels.DetectBlackBarTop(sourceWidth, sourceHeight, settings.OffsetLeft, settings.OffsetRight, settings.OffsetTop, settings.OffsetBottom)
                : 0);
            int offsetBottom = settings.OffsetBottom + (settings.BlackBarDetectionMode.HasFlag(BlackBarDetectionMode.Bottom)
                ? pixels.DetectBlackBarBottom(sourceWidth, sourceHeight, settings.OffsetLeft, settings.OffsetRight, settings.OffsetTop, settings.OffsetBottom)
                : 0);

            int effectiveSourceWidth = sourceWidth - offsetLeft - offsetRight;
            int effectiveSourceHeight = sourceHeight - offsetTop - offsetBottom;

            int relevantSourceHeight = (int)Math.Round(effectiveSourceHeight * (settings.MirroredAmount / 100.0));
            int relevantOffsetTop = sourceHeight - offsetBottom - relevantSourceHeight;

            double widthPixels = effectiveSourceWidth / (double)targetWidth;
            double heightPixels = relevantSourceHeight / (double)targetHeight;

            if (widthPixels <= 0 || heightPixels <= 0 || (relevantSourceHeight + relevantOffsetTop > sourceHeight) || effectiveSourceWidth > sourceWidth)
                return colors.ToBGRArray();

            int increment = Math.Max(1, Math.Min(20, settings.Downsampling));
            for (int y = 0; y < relevantSourceHeight; y += increment)
            {
                int targetWidthIndex = 0;
                double widthCounter = widthPixels;

                for (int x = 0; x < effectiveSourceWidth; x += increment)
                {
                    if (x >= widthCounter)
                    {
                        widthCounter += widthPixels;
                        targetWidthIndex++;
                    }

                    int colorsOffset = targetWidthIndex;
                    int sourceOffset = ((((relevantOffsetTop + y) * sourceWidth) + offsetLeft + x) * 4);

                    AvgColor color = colors[colorsOffset];
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
