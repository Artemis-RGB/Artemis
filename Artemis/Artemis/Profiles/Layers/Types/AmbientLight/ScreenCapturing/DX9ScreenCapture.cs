using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Media;
using Microsoft.Win32;
using SharpDX.Direct3D9;

namespace Artemis.Profiles.Layers.Types.AmbientLight.ScreenCapturing
{
    public class DX9ScreenCapture : IScreenCapture
    {
        #region Constructors

        public DX9ScreenCapture()
        {
            Width = Screen.PrimaryScreen.Bounds.Width;
            Height = Screen.PrimaryScreen.Bounds.Height;

            //DarthAffe 08.04.2017: Fix for systems using windows-scaling. The primary screen size is reported 'wrong'.
            double scaling = GetScaling();
            if (Math.Abs(scaling - 1.0) > 0.01)
            {
                Width = (int)(Width / scaling);
                Height = (int)(Height / scaling);
            }

            var presentParams = new PresentParameters(Width, Height)
            {
                Windowed = true,
                SwapEffect = SwapEffect.Discard
            };

            _device = new Device(new Direct3D(), 0, DeviceType.Hardware, IntPtr.Zero,
                CreateFlags.SoftwareVertexProcessing, presentParams);
            _surface = Surface.CreateOffscreenPlain(_device, Width, Height, Format.A8R8G8B8, Pool.Scratch);
            _buffer = new byte[Width * Height * 4];
        }

        #endregion

        #region Properties & Fields

        private readonly Device _device;
        private readonly Surface _surface;
        private readonly byte[] _buffer;

        public int Width { get; }
        public int Height { get; }
        public PixelFormat PixelFormat => PixelFormats.Bgr24;

        #endregion

        #region Methods

        private double GetScaling()
        {
            try
            {
                int currentDpi = (int)Registry.GetValue("HKEY_CURRENT_USER\\Control Panel\\Desktop", "LogPixels", 96);
                return 96.0 / currentDpi;
            }
            catch
            {
                return 1.0;
            }
        }

        public byte[] CaptureScreen()
        {
            _device.GetFrontBufferData(0, _surface);

            var dr = _surface.LockRectangle(LockFlags.None);
            Marshal.Copy(dr.DataPointer, _buffer, 0, _buffer.Length);
            _surface.UnlockRectangle();

            return _buffer;
        }

        public void Dispose()
        {
            _device?.Dispose();
            _surface?.Dispose();
        }

        #endregion
    }
}