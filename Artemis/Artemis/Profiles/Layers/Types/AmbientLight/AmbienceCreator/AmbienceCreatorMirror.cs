using System;

namespace Artemis.Profiles.Layers.Types.AmbientLight.AmbienceCreator
{
    public class AmbienceCreatorMirror : IAmbienceCreator
    {
        #region Properties & Fields

        #endregion

        #region Constructors

        #endregion

        #region Methods

        public byte[] GetAmbience(byte[] data, int sourceWidth, int sourceHeight, int targetWidth, int targetHeight)
        {
            int heightPixelCount = (int)Math.Round(sourceHeight * 0.1);
            int sourceHeightOffset = sourceHeight - heightPixelCount;

            AvgColor[] avgData = new AvgColor[targetWidth * targetHeight];
            double widthPixels = (sourceWidth / (double)targetWidth);
            double heightPixels = (heightPixelCount / (double)targetHeight);
            int targetHeightIndex = 0;
            double heightCounter = heightPixels;

            for (int y = 0; y < heightPixelCount; y += 2)
            {
                if (y >= heightCounter)
                {
                    heightCounter += heightPixels;
                    targetHeightIndex++;
                }

                int targetWidthIndex = 0;
                double widthCounter = widthPixels;

                for (int x = 0; x < sourceWidth; x += 2)
                {
                    if (x >= widthCounter)
                    {
                        widthCounter += widthPixels;
                        targetWidthIndex++;
                    }

                    int newOffset = (targetHeightIndex * targetWidth) + targetWidthIndex;
                    int offset = ((((sourceHeightOffset + y) * sourceWidth) + x) * 4);

                    if (avgData[newOffset] == null)
                        avgData[newOffset] = new AvgColor();

                    AvgColor avgDataObject = avgData[newOffset];

                    avgDataObject.AddB(data[offset]);
                    avgDataObject.AddG(data[offset + 1]);
                    avgDataObject.AddR(data[offset + 2]);
                }
            }

            avgData = FlipVertical(avgData, targetWidth);
            return ToByteArray(avgData, targetWidth, targetHeight);
        }

        private byte[] ToByteArray(AvgColor[] colors, int width, int height)
        {
            byte[] newData = new byte[width * height * 3];
            int counter = 0;
            foreach (AvgColor color in colors)
            {
                newData[counter++] = color.B;
                newData[counter++] = color.G;
                newData[counter++] = color.R;
            }

            return newData;
        }

        private T[] FlipVertical<T>(T[] data, int width)
        {
            T[] flipped = new T[data.Length];
            for (int i = 0, j = data.Length - width; i < data.Length; i += width, j -= width)
                for (int k = 0; k < width; ++k)
                    flipped[i + k] = data[j + k];

            return flipped;
        }

        private T[] FlipHorizontal<T>(T[] data, int width)
        {
            T[] flipped = new T[data.Length];
            for (int i = 0; i < data.Length; i += width)
                for (int j = 0, k = width - 1; j < width; ++j, --k)
                    flipped[i + j] = data[i + k];

            return flipped;
        }

        #endregion
    }
}
