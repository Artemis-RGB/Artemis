using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Artemis.Core.Events;
using Artemis.Core.Services.Interfaces;
using Stylet;

namespace Artemis.UI.Screens.Settings.Debug
{
    public class DebugViewModel : Screen, IScreenViewModel
    {
        private readonly ICoreService _coreService;
        private readonly IRgbService _rgbService;

        public DebugViewModel(ICoreService coreService, IRgbService rgbService)
        {
            _coreService = coreService;
            _rgbService = rgbService;
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
            if (e.Bitmap == null)
                return;

            var imageSource = ImageSourceFromBitmap(e.Bitmap);
            imageSource.Freeze();
            Execute.OnUIThread(() => { CurrentFrame = imageSource; });
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

        // This is much quicker than saving the bitmap into a memory stream and converting it
        private static ImageSource ImageSourceFromBitmap(Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(handle);
            }
        }

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);
    }
}