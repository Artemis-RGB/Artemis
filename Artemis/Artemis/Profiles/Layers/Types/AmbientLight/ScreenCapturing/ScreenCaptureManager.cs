using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Media;

namespace Artemis.Profiles.Layers.Types.AmbientLight.ScreenCapturing
{
    public static class ScreenCaptureManager
    {
        #region Properties & Fields

        // ReSharper disable once InconsistentNaming
        private static readonly IScreenCapture _screenCapture;

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

        #region Constructors

        static ScreenCaptureManager()
        {
            _screenCapture = new DX9ScreenCapture();
        }

        #endregion

        #region Methods

        private static void Update()
        {
            while ((DateTime.Now - _lastCaptureAccess).TotalSeconds < StandByTime)
            {
                DateTime lastCapture = DateTime.Now;
                try
                {
                    CaptureScreen();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("[CaptureLoop]: " + ex.Message);
                }

                int sleep = (int)((UpdateRate - (DateTime.Now - lastCapture).TotalSeconds) * 1000);
                if (sleep > 0)
                    Thread.Sleep(sleep);
            }

            _isRunning = false;
        }

        private static void CaptureScreen()
        {
            _lastScreenCapture = _screenCapture.CaptureScreen();
            LastCaptureWidth = _screenCapture.Width;
            LastCaptureHeight = _screenCapture.Height;
            LastCapturePixelFormat = _screenCapture.PixelFormat;
        }

        private static void StartLoop()
        {
            if (_isRunning) return;

            _isRunning = true;
            _worker = new Thread(Update);
            _worker.Start();
        }

        public static byte[] GetLastScreenCapture()
        {
            _lastCaptureAccess = DateTime.Now;
            if (!_isRunning)
            {
                // DarthAffe 29.10.2016: Make sure, that _lastScreenCapture is newer returned without data.
                CaptureScreen();
                StartLoop();
            }
            return _lastScreenCapture;
        }

        #endregion
    }
}
