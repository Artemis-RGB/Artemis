using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls.Dialogs;
using Brush = System.Windows.Media.Brush;
using Size = System.Windows.Size;

namespace Artemis.DeviceProviders
{
    public abstract class KeyboardProvider : DeviceProvider
    {
        protected KeyboardProvider()
        {
            Type = DeviceType.Keyboard;
        }

        public string Name { get; set; }
        public string Slug { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public string CantEnableText { get; set; }
        public PreviewSettings PreviewSettings { get; set; }

        public abstract bool CanEnable();
        public abstract void Enable();
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

        public virtual Task<bool> CanEnableAsync(ProgressDialogController dialog)
        {
            return Task.Run(() => CanEnable());
        }

        public virtual Task EnableAsync(ProgressDialogController dialog)
        {
            return Task.Run(() => Enable());
        }

        public override void UpdateDevice(Brush brush)
        {
            throw new NotImplementedException("KeyboardProvider doesn't implement UpdateDevice, use DrawBitmap instead.");
        }

        public override bool TryEnable()
        {
            throw new NotImplementedException(
                "KeyboardProvider doesn't implement TryEnable, use CanEnableAsync instead.");
        }
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