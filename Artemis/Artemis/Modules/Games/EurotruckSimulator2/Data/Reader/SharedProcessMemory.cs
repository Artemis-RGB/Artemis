using System;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace Artemis.Modules.Games.EurotruckSimulator2.Data.Reader
{
    internal class SharedProcessMemory<T> : IDisposable
    {
        private readonly string _mapName;
        private MemoryMappedViewAccessor _memoryMappedAccessor;

        private MemoryMappedFile _memoryMappedFile;

        public SharedProcessMemory(string mapName)
        {
            _mapName = mapName;
            Data = default(T);
        }

        public T Data { get; set; }

        public bool IsConnected
        {
            get
            {
                InitializeViewAccessor();
                return _memoryMappedAccessor != null;
            }
        }

        public void Dispose()
        {
            if (_memoryMappedAccessor != null)
            {
                _memoryMappedAccessor.Dispose();
                _memoryMappedFile.Dispose();
            }
        }

        private void InitializeViewAccessor()
        {
            if (_memoryMappedAccessor == null)
                try
                {
                    _memoryMappedFile = MemoryMappedFile.OpenExisting(_mapName, MemoryMappedFileRights.ReadWrite);
                    _memoryMappedAccessor = _memoryMappedFile.CreateViewAccessor(0, Marshal.SizeOf(typeof(T)),
                        MemoryMappedFileAccess.Read);
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch
                {
                }
        }

        public void Read()
        {
            InitializeViewAccessor();

            if (_memoryMappedAccessor == null)
                return;

            var rawData = new byte[Marshal.SizeOf(typeof(T))];

            _memoryMappedAccessor.ReadArray(0, rawData, 0, rawData.Length);

            T createdObject;

            var reservedMemPtr = IntPtr.Zero;
            try
            {
                reservedMemPtr = Marshal.AllocHGlobal(rawData.Length);
                Marshal.Copy(rawData, 0, reservedMemPtr, rawData.Length);
                createdObject = (T) Marshal.PtrToStructure(reservedMemPtr, typeof(T));
            }
            finally
            {
                if (reservedMemPtr != IntPtr.Zero)
                    Marshal.FreeHGlobal(reservedMemPtr);
            }

            Data = createdObject;
        }
    }
}