using System;
using Artemis.Profiles.Layers.Types.AmbientLight.Model;
using Artemis.Profiles.Layers.Types.AmbientLight.Model.Extensions;

namespace Artemis.Profiles.Layers.Types.AmbientLight.AmbienceCreator
{
    public class AmbienceCreatorMirror : IAmbienceCreator
    {
        #region Methods

        public byte[] GetAmbience(byte[] pixels, int sourceWidth, int sourceHeight,
                                                 int targetWidth, int targetHeight,
                                                 AmbientLightPropertiesModel settings)
        {
            AvgColor[] colors = new AvgColor[targetWidth * targetHeight];
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

            int targetHeightIndex = 0;
            double heightCounter = heightPixels;

            for (int y = 0; y < relevantSourceHeight; y += 2)
            {
                if (y >= heightCounter)
                {
                    heightCounter += heightPixels;
                    targetHeightIndex++;
                }

                int targetWidthIndex = 0;
                double widthCounter = widthPixels;

                for (int x = 0; x < effectiveSourceWidth; x += 2)
                {
                    if (x >= widthCounter)
                    {
                        widthCounter += widthPixels;
                        targetWidthIndex++;
                    }

                    int colorsOffset = (targetHeightIndex * targetWidth) + targetWidthIndex;
                    int sourceOffset = ((((relevantOffsetTop + y) * sourceWidth) + offsetLeft + x) * 4);

                    AvgColor color = colors[colorsOffset];
                    color.AddB(pixels[sourceOffset]);
                    color.AddG(pixels[sourceOffset + 1]);
                    color.AddR(pixels[sourceOffset + 2]);
                }
            }

            colors = colors.Flip(targetWidth, settings.FlipMode);
            return colors.ToBGRArray();
        }

        #endregion
    }
}
