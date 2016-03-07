// Original code by Florian Schnell
// http://www.floschnell.de/computer-science/super-fast-screen-capture-with-windows-8.html

using System;
using System.Drawing;
using System.IO;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.DXGI.MapFlags;
using Resource = SharpDX.DXGI.Resource;
using ResultCode = SharpDX.DXGI.ResultCode;

namespace Artemis.Utilities
{
    internal class ScreenCapture : IDisposable
    {
        private readonly Device _device;
        private readonly Factory1 _factory;
        private readonly Texture2D _screenTexture;
        private DataStream _dataStream;
        private OutputDuplication _duplicatedOutput;
        private Resource _screenResource;
        private Surface _screenSurface;

        public ScreenCapture()
        {
            // Create device and factory
            _device = new Device(DriverType.Hardware);
            _factory = new Factory1();

            // Creating CPU-accessible texture resource
            var texdes = new Texture2DDescription
            {
                CpuAccessFlags = CpuAccessFlags.Read,
                BindFlags = BindFlags.None,
                Format = Format.B8G8R8A8_UNorm,
                Height = _factory.Adapters1[0].Outputs[0].Description.DesktopBounds.Bottom,
                Width = _factory.Adapters1[0].Outputs[0].Description.DesktopBounds.Right,
                OptionFlags = ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription =
                {
                    Count = 1,
                    Quality = 0
                },
                Usage = ResourceUsage.Staging
            };
            _screenTexture = new Texture2D(_device, texdes);

            // duplicate output stuff
            var output = new Output1(_factory.Adapters1[0].Outputs[0].NativePointer);
            _duplicatedOutput = output.DuplicateOutput(_device);
            _screenResource = null;
            _dataStream = null;
        }

        public void Dispose()
        {
            _duplicatedOutput.Dispose();
            _screenResource.Dispose();
            _dataStream.Dispose();
            _factory.Dispose();
        }

        public DataStream Capture()
        {
            try
            {
                OutputDuplicateFrameInformation duplicateFrameInformation;
                _duplicatedOutput.AcquireNextFrame(1000, out duplicateFrameInformation, out _screenResource);
            }
            catch (SharpDXException e)
            {
                if (e.ResultCode.Code == ResultCode.WaitTimeout.Result.Code ||
                    e.ResultCode.Code == ResultCode.AccessDenied.Result.Code ||
                    e.ResultCode.Code == ResultCode.AccessLost.Result.Code)
                    return null;
                throw;
            }

            // copy resource into memory that can be accessed by the CPU
            _device.ImmediateContext.CopyResource(_screenResource.QueryInterface<SharpDX.Direct3D11.Resource>(),
                _screenTexture);

            // cast from texture to surface, so we can access its bytes
            _screenSurface = _screenTexture.QueryInterface<Surface>();

            // map the resource to access it
            _screenSurface.Map(MapFlags.Read, out _dataStream);

            // seek within the stream and read one byte
            _dataStream.Position = 4;
            _dataStream.ReadByte();

            // free resources
            _dataStream.Close();
            _screenSurface.Unmap();
            _screenSurface.Dispose();
            _screenResource.Dispose();
            _duplicatedOutput.ReleaseFrame();

            return _dataStream;
        }

        /// <summary>
        ///     Gets a specific pixel out of the data stream.
        /// </summary>
        /// <param name="surfaceDataStream"></param>
        /// <param name="position">Given point on the screen.</param>
        /// <returns></returns>
        public Color GetColor(DataStream surfaceDataStream, Point position)
        {
            var data = new byte[4];
            surfaceDataStream.Seek(
                position.Y*_factory.Adapters1[0].Outputs[0].Description.DesktopBounds.Right*4 + position.X*4,
                SeekOrigin.Begin);
            surfaceDataStream.Read(data, 0, 4);
            return Color.FromArgb(255, data[2], data[1], data[0]);
        }
    }
}