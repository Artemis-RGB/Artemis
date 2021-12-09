using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Linux.Utilities;

namespace Artemis.UI.Linux.Providers.Input
{
    internal class LinuxInputDeviceReader
    {
        private readonly FileStream _stream;
        private readonly Task _task;
        private readonly CancellationTokenSource _cts;
        private readonly byte[] _buffer;
        internal event EventHandler<LinuxInputEventArgs>? InputEvent;

        public LinuxInputDevice InputDevice { get; }

        public LinuxInputDeviceReader(LinuxInputDevice inputDevice)
        {
            InputDevice = inputDevice;

            _buffer = new byte[Marshal.SizeOf<LinuxInputEventArgs>()];
            _stream = new FileStream(InputDevice.EventPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 2048, FileOptions.Asynchronous);
            _cts = new CancellationTokenSource();
            _task = Task.Run(Read, _cts.Token);
        }

        private async Task Read()
        {
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    int readBytes = await _stream.ReadAsync(_buffer, _cts.Token);
                    if (readBytes == 0)
                        continue;

                    InputEvent?.Invoke(this, MemoryMarshal.Read<LinuxInputEventArgs>(_buffer));
                }
                catch
                {
                    // ignored
                }
            }
        }

        public void Dispose()
        {
            _cts.Cancel();
            
            _stream.Flush();
            _stream.Dispose();

            //_task.Wait(); //TODO: fix this, it hangs
            _cts.Dispose();
        }
    }
}