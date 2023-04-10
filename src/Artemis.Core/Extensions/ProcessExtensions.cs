using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;

namespace Artemis.Core;

/// <summary>
///     A static class providing <see cref="Process" /> extensions
/// </summary>
[SuppressMessage("Design", "CA1060:Move pinvokes to native methods class", Justification = "I don't care, piss off")]
public static class ProcessExtensions
{
    /// <summary>
    ///     Gets the file name of the given process
    /// </summary>
    /// <param name="p">The process</param>
    /// <returns>The filename of the given process</returns>
    public static string GetProcessFilename(this Process p)
    {
        int capacity = 2000;
        StringBuilder builder = new(capacity);
        nint ptr = OpenProcess(ProcessAccessFlags.QueryLimitedInformation, false, p.Id);
        if (!QueryFullProcessImageName(ptr, 0, builder, ref capacity)) return string.Empty;

        return builder.ToString();
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern bool QueryFullProcessImageName([In] nint hProcess, [In] int dwFlags, [Out] StringBuilder lpExeName, ref int lpdwSize);

    [DllImport("kernel32.dll")]
    private static extern nint OpenProcess(ProcessAccessFlags processAccess, bool bInheritHandle, int processId);

    [Flags]
    private enum ProcessAccessFlags : uint
    {
        QueryLimitedInformation = 0x00001000
    }
}