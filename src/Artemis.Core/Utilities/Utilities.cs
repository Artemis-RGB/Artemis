using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using SkiaSharp;

namespace Artemis.Core;

/// <summary>
///     Provides a few general utilities for ease of use
/// </summary>
public static class Utilities
{
    private static bool _shuttingDown;

    /// <summary>
    ///     Call this before even initializing the Core to make sure the folders required for operation are in place
    /// </summary>
    public static void PrepareFirstLaunch()
    {
        CreateAccessibleDirectory(Constants.DataFolder);
        CreateAccessibleDirectory(Constants.PluginsFolder);
        CreateAccessibleDirectory(Constants.LayoutsFolder);
        CreateAccessibleDirectory(Constants.UpdatingFolder);
    }

    /// <summary>
    ///     Attempts to gracefully shut down the application with a delayed kill to ensure the application shut down
    ///     <para>
    ///         This is required because not all SDKs shut down properly, it is too unpredictable to just assume we can
    ///         gracefully shut down
    ///     </para>
    /// </summary>
    public static void Shutdown()
    {
        if (_shuttingDown)
            return;
        
        // Request a graceful shutdown, whatever UI we're running can pick this up
        _shuttingDown = true;
        OnShutdownRequested();
    }

    /// <summary>
    ///     Restarts the application
    /// </summary>
    /// <param name="elevate">Whether the application should be restarted with elevated permissions</param>
    /// <param name="delay">Delay in seconds before killing process and restarting </param>
    /// <param name="extraArgs">A list of extra arguments to pass to Artemis when restarting</param>
    public static void Restart(bool elevate, TimeSpan delay, params string[] extraArgs)
    {
        if (_shuttingDown)
            return;

        if (!OperatingSystem.IsWindows() && elevate)
            throw new ArtemisCoreException("Elevation on non-Windows platforms is not supported.");

        _shuttingDown = true;
        OnRestartRequested(new RestartEventArgs(elevate, delay, extraArgs.ToList()));
    }

    /// <summary>
    ///     Applies a pending update
    /// </summary>
    /// <param name="silent">A boolean indicating whether to silently update or not.</param>
    public static void ApplyUpdate(bool silent)
    {
        OnUpdateRequested(new UpdateEventArgs(silent));
    }

    /// <summary>
    ///     Opens the provided URL in the default web browser
    /// </summary>
    /// <param name="url">The URL to open</param>
    /// <returns>The process created to open the URL</returns>
    public static Process? OpenUrl(string url)
    {
        ProcessStartInfo processInfo = new()
        {
            FileName = url,
            UseShellExecute = true
        };
        return Process.Start(processInfo);
    }

    /// <summary>
    ///     Creates all directories and subdirectories in the specified path unless they already exist with permissions
    ///     allowing access by everyone.
    /// </summary>
    /// <param name="path">The directory to create.</param>
    public static void CreateAccessibleDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            DirectoryInfo dataDirectory = Directory.CreateDirectory(path);

            if (!OperatingSystem.IsWindows())
                return;

            // On Windows, ensure everyone has permission (important when running as admin)
            DirectorySecurity security = dataDirectory.GetAccessControl();
            SecurityIdentifier everyone = new(WellKnownSidType.WorldSid, null);
            security.AddAccessRule(new FileSystemAccessRule(
                everyone,
                FileSystemRights.Modify | FileSystemRights.Synchronize,
                InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                PropagationFlags.None, AccessControlType.Allow)
            );
            dataDirectory.SetAccessControl(security);
        }
    }

    /// <summary>
    ///     Occurs when the core has requested an application shutdown
    /// </summary>
    public static event EventHandler? ShutdownRequested;

    /// <summary>
    ///     Occurs when the core has requested an application restart
    /// </summary>
    public static event EventHandler<RestartEventArgs>? RestartRequested;

    /// <summary>
    ///     Occurs when the core has requested a pending application update to be applied
    /// </summary>
    public static event EventHandler<UpdateEventArgs>? UpdateRequested;

    /// <summary>
    ///     Opens the provided folder in the user's file explorer
    /// </summary>
    /// <param name="path">The full path of the folder to open</param>
    public static void OpenFolder(string path)
    {
        if (OperatingSystem.IsWindows())
            Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", path);
        else if (OperatingSystem.IsMacOS())
            Process.Start("open", path);
        else if (OperatingSystem.IsLinux())
            Process.Start("xdg-open", path);
        else
            throw new PlatformNotSupportedException("Can't open folders on this platform");
    }

    /// <summary>
    ///     Gets the current application location
    /// </summary>
    /// <returns></returns>
    internal static string GetCurrentLocation()
    {
        return Process.GetCurrentProcess().MainModule!.FileName!;
    }

    private static void OnRestartRequested(RestartEventArgs e)
    {
        RestartRequested?.Invoke(null, e);
    }

    private static void OnShutdownRequested()
    {
        ShutdownRequested?.Invoke(null, EventArgs.Empty);
    }

    private static void OnUpdateRequested(UpdateEventArgs e)
    {
        UpdateRequested?.Invoke(null, e);
    }
}