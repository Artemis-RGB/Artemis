using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Media;
using SharpDX;
using SharpDX.Direct3D9;

namespace Artemis.Profiles.Layers.Types.AmbientLight.ScreenCapturing
{
    public class DX9ScreenCapture : IScreenCapture
    {
        #region Properties & Fields

        private Device _device;

        public int Width { get; }
        public int Height { get; }
        public PixelFormat PixelFormat => PixelFormats.Bgr24;

        #endregion

        #region Constructors

        public DX9ScreenCapture()
        {
            Width = Screen.PrimaryScreen.Bounds.Width;
            Height = Screen.PrimaryScreen.Bounds.Height;

            PresentParameters presentParams = new PresentParameters(Width, Height)
            {
                Windowed = true,
                SwapEffect = SwapEffect.Discard
            };

            _device = new Device(new Direct3D(), 0, DeviceType.Hardware, IntPtr.Zero, CreateFlags.SoftwareVertexProcessing, presentParams);
        }

        #endregion

        #region Methods

        public byte[] CaptureScreen()
        {
            using (Surface s = Surface.CreateOffscreenPlain(_device, Width, Height, Format.A8R8G8B8, Pool.Scratch))
            {
                _device.GetFrontBufferData(0, s);
                DataRectangle dr = s.LockRectangle(LockFlags.None);

                byte[] buffer = new byte[Width * Height * 4];
                Marshal.Copy(dr.DataPointer, buffer, 0, buffer.Length);

                s.UnlockRectangle();
                return buffer;
            }
        }

        public void Dispose()
        {
            _device?.Dispose();
        }

        #endregion
    }
}
