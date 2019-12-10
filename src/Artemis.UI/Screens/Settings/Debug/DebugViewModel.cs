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

namespace Artemis.UI.Screens.Settings.Debug
{
    public class DebugViewModel : Screen
    {
        private readonly ICoreService _coreService;
        private readonly IRgbService _rgbService;

        public DebugViewModel(ICoreService coreService, IRgbService rgbService, ISurfaceService surfaceService)
        {
            _coreService = coreService;
            _rgbService = rgbService;

            surfaceService.SurfaceConfigurationUpdated += (sender, args) => Execute.PostToUIThread(() => CurrentFrame = null);
            surfaceService.ActiveSurfaceConfigurationChanged += (sender, args) => Execute.PostToUIThread(() => CurrentFrame = null);
        }

        public ImageSource CurrentFrame { get; set; }
        public double CurrentFps { get; set; }

        public string Title => "Debugger";

        public void ForceGarbageCollection()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private void CoreServiceOnFrameRendered(object sender, FrameRenderedEventArgs e)
        {
            Execute.PostToUIThread(() =>
            {
                if (e.BitmapBrush.Bitmap == null)
                    return;

                if (!(CurrentFrame is WriteableBitmap writeableBitmap))
                {
                    CurrentFrame = e.BitmapBrush.Bitmap.ToWriteableBitmap();
                    return;
                }

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
            });
        }

        private void CoreServiceOnFrameRendering(object sender, FrameRenderingEventArgs e)
        {
            CurrentFps = Math.Round(1.0 / e.DeltaTime, 2);
        }

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
    }
}