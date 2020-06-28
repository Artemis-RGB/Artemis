using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Artemis.Core.Events;
using Artemis.Core.Services.Interfaces;
using Artemis.Core.Services.Storage.Interfaces;
using SkiaSharp;
using SkiaSharp.Views.WPF;
using Stylet;

namespace Artemis.UI.Screens.Settings.Debug.Tabs
{
    public class RenderDebugViewModel : Screen
    {
        private readonly ICoreService _coreService;
        private readonly IRgbService _rgbService;
        private readonly ISurfaceService _surfaceService;

        public RenderDebugViewModel(ICoreService coreService, IRgbService rgbService, ISurfaceService surfaceService)
        {
            _coreService = coreService;
            _rgbService = rgbService;
            _surfaceService = surfaceService;

            DisplayName = "Rendering";
        }

        public ImageSource CurrentFrame { get; set; }
        public double CurrentFps { get; set; }

        protected override void OnActivate()
        {
            _coreService.FrameRendered += CoreServiceOnFrameRendered;
            _coreService.FrameRendering += CoreServiceOnFrameRendering;
            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            _coreService.FrameRendered -= CoreServiceOnFrameRendered;
            _coreService.FrameRendering -= CoreServiceOnFrameRendering;
            base.OnDeactivate();
        }

        private void CoreServiceOnFrameRendered(object sender, FrameRenderedEventArgs e)
        {
            Execute.PostToUIThread(() =>
            {
                if (e.BitmapBrush?.Bitmap == null)
                    return;

                if (!(CurrentFrame is WriteableBitmap writeableBitmap))
                {
                    CurrentFrame = e.BitmapBrush.Bitmap.ToWriteableBitmap();
                    return;
                }

                try
                {
                    using (var skiaImage = SKImage.FromPixels(e.BitmapBrush.Bitmap.PeekPixels()))
                    {
                        var info = new SKImageInfo(skiaImage.Width, skiaImage.Height);
                        writeableBitmap.Lock();
                        using (var pixmap = new SKPixmap(info, writeableBitmap.BackBuffer, writeableBitmap.BackBufferStride))
                        {
                            skiaImage.ReadPixels(pixmap, 0, 0);
                        }

                        writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, writeableBitmap.PixelWidth, writeableBitmap.PixelHeight));
                        writeableBitmap.Unlock();
                    }
                }
                catch (AccessViolationException)
                {
                    // oops
                }
            });
        }

        private void CoreServiceOnFrameRendering(object sender, FrameRenderingEventArgs e)
        {
            CurrentFps = Math.Round(1.0 / e.DeltaTime, 2);
        }
    }
}