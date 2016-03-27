using System.Collections.Generic;
using System.Drawing;

namespace Artemis.KeyboardProviders
{
    public abstract class KeyboardProvider
    {
        public string Name { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public string CantEnableText { get; set; }

        public List<KeyboardRegion> KeyboardRegions { get; set; }

        public abstract bool CanEnable();
        public abstract void Enable();
        public abstract void Disable();
        public abstract void DrawBitmap(Bitmap bitmap);

        /// <summary>
        ///     Returns a bitmap matching the keyboard's dimensions
        /// </summary>
        /// <returns></returns>
        public Bitmap KeyboardBitmap() => new Bitmap(Width, Height);

        /// <summary>
        ///     Returns a bitmap matching the keyboard's dimensions using the provided scale
        /// </summary>
        /// <returns></returns>
        public Bitmap KeyboardBitmap(int scale) => new Bitmap(Width*scale, Height*scale);
    }
}