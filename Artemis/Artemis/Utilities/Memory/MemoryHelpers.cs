using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using Artemis.Models;
using Artemis.Settings;
using Newtonsoft.Json;

namespace Artemis.Utilities.Memory
{
    public static class MemoryHelpers
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer,
            int dwSize, ref int lpNumberOfBytesRead);

        public static Process GetProcessIfRunning(string processName)
        {
            var processes = Process.GetProcessesByName(processName);
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

        public static void GetPointers()
        {
            if (!General.Default.EnablePointersUpdate)
                return;

            try
            {
                var jsonClient = new WebClient();
                // Random number to get around cache issues
                var rand = new Random(DateTime.Now.Millisecond);
                var json =
                    jsonClient.DownloadString(
                        "https://raw.githubusercontent.com/SpoinkyNL/Artemis/master/pointers.json?random=" + rand.Next());

                // Get a list of pointers
                var pointers = JsonConvert.DeserializeObject<List<GamePointersCollectionModel>>(json);
                // Assign each pointer to the settings file
                var rlPointers = JsonConvert.SerializeObject(pointers.FirstOrDefault(p => p.Game == "RocketLeague"));
                if (rlPointers != null)
                {
                    Offsets.Default.RocketLeague = rlPointers;
                    Offsets.Default.Save();
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}