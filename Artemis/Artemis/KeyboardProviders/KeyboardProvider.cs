using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using Size = System.Windows.Size;

namespace Artemis.KeyboardProviders
{
    public abstract class KeyboardProvider
    {
        public string Name { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public string CantEnableText { get; set; }

        public List<KeyboardRegion> KeyboardRegions { get; set; }

        public PreviewSettings PreviewSettings { get; set; }

        public abstract bool CanEnable();
        public abstract void Enable(); // TODO: This should be done in a background thread with a callback mechanism as it causes UI lag
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

        public Rect KeyboardRectangle(int scale) => new Rect(new Size(Width*scale, Height*scale));
    }

    public struct PreviewSettings
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public Thickness Margin { get; set; }
        public Bitmap Image { get; set; }

        public PreviewSettings(int width, int height, Thickness margin, Bitmap image)
        {
            Width = width;
            Height = height;
            Margin = margin;
            Image = image;
        }
    }
}