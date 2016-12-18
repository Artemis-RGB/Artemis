using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace Artemis.Modules.Games.ProjectCars.Data
{
    public class pCarsAPI_GetData
    {
        private static pCarsAPIStruct structCurrent = new pCarsAPIStruct();
        private static MemoryMappedFile memoryMappedFile;
        private static GCHandle handle;
        private static int sharedmemorysize;
        private static byte[] sharedMemoryReadBuffer;
        private static bool isSharedMemoryInitialised;

        private static bool InitialiseSharedMemory()
        {
            try
            {
                memoryMappedFile = MemoryMappedFile.OpenExisting("$pcars$");
                sharedmemorysize = Marshal.SizeOf(typeof(pCarsAPIStruct));
                sharedMemoryReadBuffer = new byte[sharedmemorysize];
                isSharedMemoryInitialised = true;

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static Tuple<bool, pCarsAPIStruct> ReadSharedMemoryData()
        {
            var _pcarsapistruct = new pCarsAPIStruct();

            try
            {
                if (memoryMappedFile == null)
                    InitialiseSharedMemory();

                using (var sharedMemoryStreamView = memoryMappedFile.CreateViewStream())
                {
                    var _SharedMemoryStream = new BinaryReader(sharedMemoryStreamView);
                    sharedMemoryReadBuffer = _SharedMemoryStream.ReadBytes(sharedmemorysize);
                    handle = GCHandle.Alloc(sharedMemoryReadBuffer, GCHandleType.Pinned);
                    _pcarsapistruct =
                        (pCarsAPIStruct) Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(pCarsAPIStruct));
                    handle.Free();
                }

                return new Tuple<bool, pCarsAPIStruct>(true, _pcarsapistruct);
            }
            catch (Exception ex)
            {
                //return false in the tuple as the read failed
                return new Tuple<bool, pCarsAPIStruct>(false, _pcarsapistruct);
            }
        }
    }
}