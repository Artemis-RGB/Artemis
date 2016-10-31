using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Media;

namespace Artemis.Profiles.Layers.Types.AmbientLight.ScreenCapturing
{
    public static class ScreenCaptureManager
    {
        #region Properties & Fields

        private static Thread _worker;
        private static DateTime _lastCaptureAccess;
        private static volatile byte[] _lastScreenCapture;
        private static volatile bool _isRunning = false;

        public static double StandByTime { get; set; } = 3;
        public static double UpdateRate { get; set; } = 1f / 20f; // DarthAffe 29.10.2016: I think 20 FPS should be enough as default

        public static int LastCaptureWidth { get; private set; }
        public static int LastCaptureHeight { get; private set; }
        public static PixelFormat LastCapturePixelFormat { get; private set; }

        #endregion

        #region Methods

        private static IScreenCapture GetScreenCapture()
        {
            return new DX9ScreenCapture();
        }

        private static void Update(IScreenCapture screenCapture)
        {
            try
            {
                while ((DateTime.Now - _lastCaptureAccess).TotalSeconds < StandByTime)
                {
                    DateTime lastCapture = DateTime.Now;
                    try
                    {
                        CaptureScreen(screenCapture);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("[CaptureLoop]: " + ex.Message);
                    }

                    int sleep = (int)((UpdateRate - (DateTime.Now - lastCapture).TotalSeconds) * 1000);
                    if (sleep > 0)
                        Thread.Sleep(sleep);
                }
            }
            finally
            {
                screenCapture.Dispose();
                _isRunning = false;
            }
        }

        private static void CaptureScreen(IScreenCapture screenCapture)
        {
            _lastScreenCapture = screenCapture.CaptureScreen();
            LastCaptureWidth = screenCapture.Width;
            LastCaptureHeight = screenCapture.Height;
            LastCapturePixelFormat = screenCapture.PixelFormat;
        }

        private static void StartLoop()
        {
            IScreenCapture screenCapture = GetScreenCapture();
            if (_isRunning) return;

            // DarthAffe 31.10.2016: _lastScreenCapture should be always initialized!
            CaptureScreen(screenCapture);

            _isRunning = true;
            _worker = new Thread(() => Update(screenCapture));
            _worker.Start();
        }

        public static byte[] GetLastScreenCapture()
        {
            _lastCaptureAccess = DateTime.Now;
            if (!_isRunning)
                StartLoop();
            return _lastScreenCapture;
        }

        #endregion
    }
}
