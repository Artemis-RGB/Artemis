using System;
using System.Runtime.InteropServices;
using Process.NET.Memory;

namespace Artemis.Utilities.Memory
{
    public static class MemoryHelpers
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer,
            int dwSize, ref int lpNumberOfBytesRead);

        public static System.Diagnostics.Process GetProcessIfRunning(string processName)
        {
            var processes = System.Diagnostics.Process.GetProcessesByName(processName);
            return processes.Length >= 1 ? processes[0] : null;
        }

        public static IntPtr FindAddress(IntPtr pHandle, IntPtr baseAddress, IntPtr staticPointer, int[] offsets)
        {
            // Create a buffer that is 4 bytes on a 32-bit system or 8 bytes on a 64-bit system. 
            var tmp = new byte[IntPtr.Size];
            var address = baseAddress;
            // We must check for 32-bit vs 64-bit. 
            address = IntPtr.Size == 4
                ? new IntPtr(address.ToInt32() + staticPointer.ToInt32())
                : new IntPtr(address.ToInt64() + staticPointer.ToInt64());

            // Loop through each offset to find the address 
            foreach (IntPtr t in offsets)
            {
                var lpNumberOfBytesRead = 0;
                ReadProcessMemory(pHandle, address, tmp, IntPtr.Size, ref lpNumberOfBytesRead);
                address = IntPtr.Size == 4
                    ? new IntPtr(BitConverter.ToInt32(tmp, 0) + t.ToInt32())
                    : new IntPtr(BitConverter.ToInt64(tmp, 0) + t.ToInt64());
            }
            return address;
        }

        public static T ReadMultilevelPointer<T>(this IMemory memory, IntPtr address, params int[] offsets)
            where T : struct
        {
            for (var i = 0; i < offsets.Length - 1; i++)
            {
                address = memory.Read<IntPtr>(address + offsets[i]);
            }
            return memory.Read<T>(address + offsets[offsets.Length - 1]);
        }
    }
}