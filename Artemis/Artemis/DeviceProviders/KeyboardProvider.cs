using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using Artemis.Utilities;
using MahApps.Metro.Controls.Dialogs;
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
        ///     Returns a bitmap matching the keyboard's dimensions using the provided scale
        /// </summary>
        /// <returns></returns>
        public Bitmap KeyboardBitmap(int scale = 4) => new Bitmap(Width * scale, Height * scale);

        public Rect KeyboardRectangle(int scale = 4) => new Rect(new Size(Width * scale, Height * scale));

        /// <summary>
        ///     Runs CanEnable asynchronously multiple times until successful, cancelled or max tries reached
        /// </summary>
        /// <param name="dialog"></param>
        /// <returns></returns>
        public Task<bool> CanEnableAsync(ProgressDialogController dialog)
        {
            return Task.Run(() =>
            {
                for (var tries = 1; tries <= 10; tries++)
                {
                    // Dialog interaction
                    if (dialog != null)
                    {
                        // Stop if cancelled by user
                        if (dialog.IsCanceled)
                        {
                            dialog.SetIndeterminate();
                            return false;
                        }
                        // Updated progress to indicate how much tries are left
                        dialog.SetProgress(0.1 * tries);
                    }

                    if (CanEnable())
                    {
                        dialog?.SetIndeterminate();
                        return true;
                    }
                    Thread.Sleep(2000);
                }
                dialog?.SetIndeterminate();
                return false;
            });
        }

        /// <summary>
        ///     Runs CanEnable asynchronously
        /// </summary>
        /// <param name="dialog"></param>
        /// <returns></returns>
        public Task EnableAsync(ProgressDialogController dialog)
        {
            return Task.Run(() => Enable());
        }

        public override void UpdateDevice(Bitmap bitmap)
        {
            throw new NotSupportedException("KeyboardProvider doesn't implement UpdateDevice, use DrawBitmap instead.");
        }

        public override bool TryEnable()
        {
            throw new NotSupportedException("KeyboardProvider doesn't implement TryEnable, use CanEnableAsync instead.");
        }

        /// <summary>
        ///     Returns the real life X and Y coordinates of the given key
        /// </summary>
        /// <param name="keyCode"></param>
        /// <returns></returns>
        public abstract KeyMatch? GetKeyPosition(Keys keyCode);
    }

    public struct KeyMatch
    {
        public KeyMatch(Keys keyCode, int x, int y)
        {
            KeyCode = keyCode;
            X = x;
            Y = y;
        }

        public Keys KeyCode { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }

    public struct PreviewSettings
    {
        public Rect OverlayRectangle { get; set; }
        public Rect BackgroundRectangle { get; set; }
        public BitmapImage Image { get; set; }

        public PreviewSettings(Rect overlayRectangle, Bitmap bitmap)
        {
            OverlayRectangle = overlayRectangle;
            BackgroundRectangle = new Rect(0, 0, bitmap.Width, bitmap.Height);
            Image = ImageUtilities.BitmapToBitmapImage(bitmap);
            Image.Freeze();
        }
    }
}
