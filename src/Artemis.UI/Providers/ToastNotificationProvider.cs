using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Windows.UI.Notifications;
using Artemis.UI.Shared.Services;
using Artemis.UI.Utilities;
using MaterialDesignThemes.Wpf;
using Microsoft.Toolkit.Uwp.Notifications;
using Stylet;

namespace Artemis.UI.Providers
{
    public class ToastNotificationProvider : INotificationProvider
    {
        private ThemeWatcher _themeWatcher;

        public ToastNotificationProvider()
        {
            _themeWatcher = new ThemeWatcher();
        }

        public static PngBitmapEncoder GetEncoderForIcon(PackIconKind icon, Color color)
        {
            // Convert the PackIcon to an icon by drawing it on a visual
            DrawingVisual drawingVisual = new();
            DrawingContext drawingContext = drawingVisual.RenderOpen();

            PackIcon packIcon = new() {Kind = icon};
            Geometry geometry = Geometry.Parse(packIcon.Data);

            // Scale the icon up to fit a 256x256 image and draw it
            geometry = Geometry.Combine(geometry, Geometry.Empty, GeometryCombineMode.Union, new ScaleTransform(256 / geometry.Bounds.Right, 256 / geometry.Bounds.Bottom));

            drawingContext.DrawGeometry(new SolidColorBrush(color), null, geometry);
            drawingContext.Close();

            // Render the visual and add it to a PNG encoder (we want opacity in our icon)
            RenderTargetBitmap renderTargetBitmap = new(256, 256, 96, 96, PixelFormats.Pbgra32);
            renderTargetBitmap.Render(drawingVisual);
            PngBitmapEncoder encoder = new();
            encoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

            return encoder;
        }

        private void ToastDismissed(string imagePath, Action dismissedCallback)
        {
            if (File.Exists(imagePath))
                File.Delete(imagePath);

            dismissedCallback?.Invoke();
        }

        private void ToastActivated(string imagePath, Action activatedCallback)
        {
            if (File.Exists(imagePath))
                File.Delete(imagePath);

            activatedCallback?.Invoke();
        }

        #region Implementation of INotificationProvider

        /// <inheritdoc />
        public void ShowNotification(string title, string message, PackIconKind icon, Action activatedCallback, Action dismissedCallback)
        {
            string imagePath = Path.GetTempFileName().Replace(".tmp", "png");

            Execute.OnUIThreadSync(() =>
            {
                using FileStream stream = File.OpenWrite(imagePath);
                GetEncoderForIcon(icon, _themeWatcher.GetSystemTheme() == ThemeWatcher.WindowsTheme.Dark ? Colors.White : Colors.Black).Save(stream);
            });

            new ToastContentBuilder()
                .AddAppLogoOverride(new Uri(imagePath))
                .AddText(title, AdaptiveTextStyle.Header)
                .AddText(message)
                .Show(t =>
                {
                    t.Dismissed += (_, _) => ToastDismissed(imagePath, dismissedCallback);
                    t.Activated += (_, _) => ToastActivated(imagePath, activatedCallback);
                    t.Data = new NotificationData(new List<KeyValuePair<string, string>> {new("image", imagePath)});
                });
        }

        #endregion

        #region IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            ToastNotificationManagerCompat.Uninstall();
        }

        #endregion
    }
}