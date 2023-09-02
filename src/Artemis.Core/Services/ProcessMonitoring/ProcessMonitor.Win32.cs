using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Artemis.Core.Services;

// DarthAffe 31.08.2023: Based on how it's done in the framework:
// https://github.com/dotnet/runtime/blob/f0463a98d105f26037f9d3e63213421a3a7d4dff/src/libraries/System.Diagnostics.Process/src/System/Diagnostics/ProcessManager.Win32.cs#L263
public static unsafe partial class ProcessMonitor
{
    #region Native

    [LibraryImport("ntdll.dll", EntryPoint = "NtQuerySystemInformation")]
    private static partial uint NtQuerySystemInformation(int systemInformationClass, void* systemInformation, uint systemInformationLength, uint* returnLength);

    [StructLayout(LayoutKind.Sequential)]
    private struct SystemProcessInformation
    {
        // ReSharper disable MemberCanBePrivate.Local
        public uint NextEntryOffset;
        public uint NumberOfThreads;
        private fixed byte Reserved1[48];
        public UnicodeString ImageName;
        public int BasePriority;
        public nint UniqueProcessId;
        private readonly nuint Reserved2;
        public uint HandleCount;
        public uint SessionId;
        private readonly nuint Reserved3;
        public nuint PeakVirtualSize;  // SIZE_T
        public nuint VirtualSize;
        private readonly uint Reserved4;
        public nuint PeakWorkingSetSize;  // SIZE_T
        public nuint WorkingSetSize;  // SIZE_T
        private readonly nuint Reserved5;
        public nuint QuotaPagedPoolUsage;  // SIZE_T
        private readonly nuint Reserved6;
        public nuint QuotaNonPagedPoolUsage;  // SIZE_T
        public nuint PagefileUsage;  // SIZE_T
        public nuint PeakPagefileUsage;  // SIZE_T
        public nuint PrivatePageCount;  // SIZE_T
        private fixed long Reserved7[6];
        // ReSharper restore MemberCanBePrivate.Local
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct UnicodeString
    {
        // ReSharper disable MemberCanBePrivate.Local
        /// <summary>
        /// Length in bytes, not including the null terminator, if any.
        /// </summary>
        public ushort Length;

        /// <summary>
        /// Max size of the buffer in bytes
        /// </summary>
        public ushort MaximumLength;
        public nint Buffer;
        // ReSharper restore MemberCanBePrivate.Local
    }

    [LibraryImport("kernel32.dll", EntryPoint = "QueryFullProcessImageNameW", StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool QueryFullProcessImageName(nint hProcess, int dwFlags, [Out] char[] lpExeName, ref int lpdwSize);

    [LibraryImport("kernel32.dll")]
    private static partial nint OpenProcess(ProcessAccessFlags processAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int processId);

    [Flags]
    private enum ProcessAccessFlags : uint
    {
        QueryLimitedInformation = 0x00001000
    }

    #endregion

    #region Constants

    private const int SYSTEM_PROCESS_INFORMATION = 5;
    private const uint STATUS_INFO_LENGTH_MISMATCH = 0xC0000004;
    private const int SYSTEM_PROCESS_ID = 4;
    private const int IDLE_PROCESS_ID = 0;
    private const int DEFAULT_BUFFER_SIZE = 1024 * 1024;

    #endregion

    #region Properties & Fields

    private static void* _buffer;
    private static uint _bufferSize;

    #endregion

    #region Methods

    private static void InitializeBuffer()
    {
        _bufferSize = DEFAULT_BUFFER_SIZE;
        _buffer = NativeMemory.Alloc(_bufferSize);
    }

    private static void FreeBuffer()
    {
        NativeMemory.Free(_buffer);
        _bufferSize = 0;
    }

    private static void UpdateProcessInfosWin32(object? o)
    {
        lock (LOCK)
        {
            try
            {
                if (_bufferSize == 0) return;

                while (true)
                {
                    uint actualSize = 0;
                    uint status = NtQuerySystemInformation(SYSTEM_PROCESS_INFORMATION, _buffer, _bufferSize, &actualSize);

                    if (status != STATUS_INFO_LENGTH_MISMATCH)
                    {
                        if ((int)status < 0)
                            throw new InvalidOperationException("Error", new Win32Exception((int)status));

                        UpdateProcessInfosWin32(new ReadOnlySpan<byte>(_buffer, (int)actualSize));
                        return;
                    }

                    ResizeBuffer(GetEstimatedBufferSize(actualSize));
                }
            }
            catch { /* Should we throw here? I guess no ... */ }
        }
    }

    private static void UpdateProcessInfosWin32(ReadOnlySpan<byte> data)
    {
        int processInformationOffset = 0;

        HashSet<int> processIds = new(_processes.Count);

        while (true)
        {
            ref readonly SystemProcessInformation pi = ref MemoryMarshal.AsRef<SystemProcessInformation>(data[processInformationOffset..]);

            int processId = pi.UniqueProcessId.ToInt32();
            processIds.Add(processId);

            if (!_processes.ContainsKey(processId))
            {
                string imageName;
                string processName;
                if (pi.ImageName.Buffer != nint.Zero)
                {
                    imageName = new ReadOnlySpan<char>(pi.ImageName.Buffer.ToPointer(), pi.ImageName.Length / sizeof(char)).ToString();
                    processName = GetProcessShortName(imageName).ToString();
                }
                else
                {
                    imageName = string.Empty;
                    processName = processId switch
                    {
                        SYSTEM_PROCESS_ID => "System",
                        IDLE_PROCESS_ID => "Idle",
                        _ => processId.ToString(CultureInfo.InvariantCulture) // use the process ID for a normal process without a name
                    };
                }

                string executable = GetProcessFilename(processId);
                ProcessInfo processInfo = new(processId, processName, imageName, executable);
                _processes.Add(processId, processInfo);

                OnProcessStarted(processInfo);
            }

            if (pi.NextEntryOffset == 0) break;
            processInformationOffset += (int)pi.NextEntryOffset;
        }

        HandleStoppedProcesses(processIds);

        LastUpdate = DateTime.Now;
    }

    private static string GetProcessFilename(int processId)
    {
        int capacity = byte.MaxValue;
        char[] buffer = new char[capacity];
        nint ptr = OpenProcess(ProcessAccessFlags.QueryLimitedInformation, false, processId);

        return QueryFullProcessImageName(ptr, 0, buffer, ref capacity)
                   ? new string(buffer, 0, capacity)
                   : string.Empty;
    }

    // This function generates the short form of process name.
    //
    // This is from GetProcessShortName in NT code base.
    // Check base\screg\winreg\perfdlls\process\perfsprc.c for details.
    private static ReadOnlySpan<char> GetProcessShortName(ReadOnlySpan<char> name)
    {
        // Trim off everything up to and including the last slash, if there is one.
        // If there isn't, LastIndexOf will return -1 and this will end up as a nop.
        name = name[(name.LastIndexOf('\\') + 1)..];

        // If the name ends with the ".exe" extension, then drop it, otherwise include
        // it in the name.
        if (name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            name = name[..^4];

        return name;
    }

    private static void ResizeBuffer(uint size)
    {
        NativeMemory.Free(_buffer);

        _bufferSize = size;
        _buffer = NativeMemory.Alloc(_bufferSize);
    }

    // allocating a few more kilo bytes just in case there are some new process
    // kicked in since new call to NtQuerySystemInformation
    private static uint GetEstimatedBufferSize(uint actualSize) => actualSize + (1024 * 10);

    #endregion
}