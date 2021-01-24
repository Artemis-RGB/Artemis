using System;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Artemis.Core
{
    /// <summary>
    ///     Provides a few general utilities for ease of use
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        ///     Call this before even initializing the Core to make sure the folders required for operation are in place
        /// </summary>
        public static void PrepareFirstLaunch()
        {
            CreateArtemisFolderIfMissing(Constants.DataFolder);
            CreateArtemisFolderIfMissing(Constants.DataFolder + "plugins");
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
            // Request a graceful shutdown, whatever UI we're running can pick this up
            OnShutdownRequested();
        }

        /// <summary>
        ///     Restarts the application
        /// </summary>
        /// <param name="elevate">Whether the application should be restarted with elevated permissions</param>
        /// <param name="delay">Delay in seconds before killing process and restarting </param>
        public static void Restart(bool elevate, TimeSpan delay)
        {
            OnRestartRequested(new RestartEventArgs(elevate, delay));
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
        ///     Gets the current application location
        /// </summary>
        /// <returns></returns>
        internal static string GetCurrentLocation()
        {
            return Process.GetCurrentProcess().MainModule!.FileName!;
        }

        private static void CreateArtemisFolderIfMissing(string path)
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

        private static void OnRestartRequested(RestartEventArgs e)
        {
            RestartRequested?.Invoke(null, e);
        }

        #region Events

        /// <summary>
        ///     Occurs when the core has requested an application shutdown
        /// </summary>
        public static event EventHandler? ShutdownRequested;

        /// <summary>
        ///     Occurs when the core has requested an application restart
        /// </summary>
        public static event EventHandler<RestartEventArgs>? RestartRequested;

        private static void OnShutdownRequested()
        {
            ShutdownRequested?.Invoke(null, EventArgs.Empty);
        }

        #endregion
    }
}