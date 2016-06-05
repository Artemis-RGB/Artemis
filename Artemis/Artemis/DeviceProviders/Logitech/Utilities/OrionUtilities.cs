using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Artemis.DeviceProviders.Logitech.Utilities
{
    public static class OrionUtilities
    {
        public static KeyMapping[] Keymappings =
        {
            // First row
            new KeyMapping(0, 0),
            new KeyMapping(1, 1),
            new KeyMapping(2, 1),
            new KeyMapping(3, 2),
            new KeyMapping(4, 3),
            new KeyMapping(5, 4),
            new KeyMapping(6, 5),
            new KeyMapping(7, 6),
            new KeyMapping(8, 7),
            new KeyMapping(9, 8),
            new KeyMapping(10, 9),
            new KeyMapping(11, 9),
            new KeyMapping(12, 10),
            new KeyMapping(13, 11),
            new KeyMapping(13, 12),
            new KeyMapping(14, 13),
            new KeyMapping(15, 14),
            new KeyMapping(16, 15),
            new KeyMapping(17, 16),
            new KeyMapping(18, 17),
            new KeyMapping(19, 18),

            // Second row
            new KeyMapping(21, 21),
            new KeyMapping(22, 22),
            new KeyMapping(23, 23),
            new KeyMapping(24, 24),
            new KeyMapping(25, 25),
            new KeyMapping(26, 26),
            new KeyMapping(27, 27),
            new KeyMapping(28, 28),
            new KeyMapping(29, 29),
            new KeyMapping(30, 30),
            new KeyMapping(31, 31),
            new KeyMapping(32, 32),
            new KeyMapping(33, 33),
            new KeyMapping(34, 34),
            new KeyMapping(35, 35),
            new KeyMapping(36, 36),
            new KeyMapping(37, 37),
            new KeyMapping(38, 38),
            new KeyMapping(39, 39),
            new KeyMapping(40, 40),
            new KeyMapping(41, 41),

            // Third row
            new KeyMapping(42, 42),
            new KeyMapping(43, 43),
            new KeyMapping(44, 44),
            new KeyMapping(45, 45),
            new KeyMapping(46, 46),
            new KeyMapping(47, 46),
            new KeyMapping(48, 47),
            new KeyMapping(49, 48),
            new KeyMapping(50, 49),
            new KeyMapping(51, 50),
            new KeyMapping(52, 51),
            new KeyMapping(53, 52),
            new KeyMapping(54, 53),
            new KeyMapping(54, 54),
            new KeyMapping(55, 55),
            new KeyMapping(56, 56),
            new KeyMapping(57, 57),
            new KeyMapping(58, 58),
            new KeyMapping(59, 59),
            new KeyMapping(60, 60),
            new KeyMapping(61, 61),
            new KeyMapping(62, 62),

            // Fourth row
            new KeyMapping(63, 63),
            new KeyMapping(64, 64),
            new KeyMapping(65, 65),
            new KeyMapping(66, 65),
            new KeyMapping(67, 66),
            new KeyMapping(68, 67),
            new KeyMapping(69, 68),
            new KeyMapping(70, 69),
            new KeyMapping(71, 70),
            new KeyMapping(72, 71),
            new KeyMapping(73, 72),
            new KeyMapping(74, 73),
            new KeyMapping(75, 74),
            new KeyMapping(76, 75),
            new KeyMapping(76, 76),
            new KeyMapping(78, 77),
            new KeyMapping(79, 78),
            new KeyMapping(79, 79),
            new KeyMapping(80, 80),
            new KeyMapping(81, 81),
            new KeyMapping(82, 82),

            // Fifth row
            new KeyMapping(84, 84),
            new KeyMapping(85, 85),
            new KeyMapping(86, 86),
            new KeyMapping(87, 87),
            new KeyMapping(88, 88),
            new KeyMapping(89, 89),
            new KeyMapping(90, 90),
            new KeyMapping(91, 91),
            new KeyMapping(92, 92),
            new KeyMapping(93, 93),
            new KeyMapping(94, 94),
            new KeyMapping(95, 95),
            new KeyMapping(96, 96),
            new KeyMapping(97, 97),
            new KeyMapping(98, 98),
            new KeyMapping(99, 99),
            new KeyMapping(100, 100),
            new KeyMapping(101, 101),
            new KeyMapping(102, 102),
            new KeyMapping(103, 103),
            new KeyMapping(104, 104),

            // Sixth row
            new KeyMapping(105, 105),
            new KeyMapping(106, 106),
            new KeyMapping(107, 107),
            new KeyMapping(108, 107),
            new KeyMapping(109, 109),
            new KeyMapping(110, 110),
            new KeyMapping(111, 110),
            new KeyMapping(112, 111),
            new KeyMapping(113, 112),
            new KeyMapping(114, 113),
            new KeyMapping(115, 114),
            new KeyMapping(116, 115),
            new KeyMapping(115, 116), // ALTGR
            new KeyMapping(116, 117),
            new KeyMapping(117, 118),
            new KeyMapping(118, 119),
            new KeyMapping(119, 120),
            new KeyMapping(120, 121),
            new KeyMapping(121, 122),
            new KeyMapping(122, 123),
            new KeyMapping(124, 124)
        };

        public static byte[] BitmapToByteArray(Bitmap b, bool remap = true)
        {
            if (b.Width > 21 || b.Height > 6)
                b = ResizeImage(b, 21, 6);

            var rect = new Rectangle(0, 0, b.Width, b.Height);
            var bitmapData = b.LockBits(rect, ImageLockMode.ReadWrite, b.PixelFormat);

            var depth = Image.GetPixelFormatSize(b.PixelFormat);
            var step = depth/8;
            var pixels = new byte[21*6*step];
            var iptr = bitmapData.Scan0;

            // Copy data from pointer to array
            Marshal.Copy(iptr, pixels, 0, pixels.Length);

            if (!remap)
                return pixels;

            var remapped = new byte[pixels.Length];

            // Every  key is 4 bytes
            for (var i = 0; i <= pixels.Length/4; i++)
            {
                var firstSByte = Keymappings[i].Source*4;
                var firstTByte = Keymappings[i].Target*4;

                for (var j = 0; j < 4; j++)
                    remapped[firstTByte + j] = pixels[firstSByte + j];
            }

            return remapped;
        }

        /// <summary>
        ///     Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        public struct KeyMapping
        {
            public KeyMapping(int source, int target)
            {
                Source = source;
                Target = target;
            }

            public int Source { get; set; }
            public int Target { get; set; }
        }
    }
}