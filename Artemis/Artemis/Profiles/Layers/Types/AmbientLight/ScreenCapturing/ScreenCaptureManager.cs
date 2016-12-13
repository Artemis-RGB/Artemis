using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows.Media;
using Artemis.Profiles.Layers.Types.AmbientLight.Model;

namespace Artemis.Profiles.Layers.Types.AmbientLight.ScreenCapturing
{
    public static class ScreenCaptureManager
    {
        #region Properties & Fields

        private static Thread _worker;
        private static DateTime _lastCaptureAccess;
        private static volatile byte[] _lastScreenCapture;
        private static volatile bool _isRunning;

        private static ScreenCaptureMode? _lastScreenCaptureMode;
        private static IScreenCapture _screenCapture;

        public static double StandByTime { get; set; } = 3;
        public static double UpdateRate { get; set; } = 1.0/20.0;
        // DarthAffe 29.10.2016: I think 20 FPS should be enough as default
        public static ScreenCaptureMode ScreenCaptureMode { get; set; } = ScreenCaptureMode.DirectX9;

        public static int LastCaptureWidth { get; private set; }
        public static int LastCaptureHeight { get; private set; }
        public static PixelFormat LastCapturePixelFormat { get; private set; }

        #endregion

        #region Methods

        private static IScreenCapture GetScreenCapture()
        {
            if (_lastScreenCaptureMode == ScreenCaptureMode && _screenCapture != null)
                return _screenCapture;

            DisposeScreenCapture();

            _lastScreenCaptureMode = ScreenCaptureMode;
            switch (ScreenCaptureMode)
            {
                case ScreenCaptureMode.DirectX9:
                    return _screenCapture = new DX9ScreenCapture();
                default:
                    throw new InvalidEnumArgumentException();
            }
        }

        private static void DisposeScreenCapture()
        {
            _screenCapture?.Dispose();
            _screenCapture = null;
            _lastScreenCapture = null;
        }

        private static void Update()
        {
            try
            {
                while ((DateTime.Now - _lastCaptureAccess).TotalSeconds < StandByTime)
                {
                    var lastCapture = DateTime.Now;
                    try
                    {
                        CaptureScreen();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("[CaptureLoop]: " + ex.Message);
                    }

                    var sleep = (int) ((UpdateRate - (DateTime.Now - lastCapture).TotalSeconds)*1000);
                    if (sleep > 0)
                        Thread.Sleep(sleep);
                }
            }
            finally
            {
                DisposeScreenCapture();
                _isRunning = false;
            }
        }

        private static void CaptureScreen()
        {
            var screenCapture = GetScreenCapture();

            _lastScreenCapture = screenCapture.CaptureScreen();
            LastCaptureWidth = screenCapture.Width;
            LastCaptureHeight = screenCapture.Height;
            LastCapturePixelFormat = screenCapture.PixelFormat;
        }

        private static void StartLoop()
        {
            if (_isRunning) return;

            // DarthAffe 31.10.2016: _lastScreenCapture should be always initialized!
            CaptureScreen();

            _isRunning = true;
            _worker = new Thread(Update);
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