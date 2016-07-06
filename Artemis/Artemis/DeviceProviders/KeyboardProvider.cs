using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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
                        dialog.SetProgress(0.1*tries);
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