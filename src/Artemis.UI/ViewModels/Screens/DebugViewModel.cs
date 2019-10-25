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

namespace Artemis.UI.ViewModels.Screens
{
    public class DebugViewModel : Screen
    {
        private readonly ICoreService _coreService;
        private readonly IRgbService _rgbService;

        public DebugViewModel(ICoreService coreService, IRgbService rgbService)
        {
            _coreService = coreService;
            _rgbService = rgbService;

            _coreService.FrameRendered += CoreServiceOnFrameRendered;
        }

        public ImageSource CurrentFrame { get; set; }
        public double CurrentFps { get; set; }

        public void ForceGarbageCollection()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private void CoreServiceOnFrameRendered(object sender, FrameEventArgs e)
        {
            var imageSource = ImageSourceFromBitmap(e.Bitmap);
            imageSource.Freeze();
            CurrentFps = Math.Round(1.0 / e.DeltaTime, 2);
            Execute.OnUIThread(() =>
            {
                CurrentFrame = imageSource;
            });
        }
        
        protected override void OnClose()
        {
            _coreService.FrameRendered -= CoreServiceOnFrameRendered;
            base.OnClose();
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