using System;
using System.Diagnostics;

namespace Artemis.Core
{
    /// <summary>
    ///     Provides a few general utilities for ease of use
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        ///     Attempts to gracefully shut down the application with a delayed kill to ensure the application shut down
        ///     <para>
        ///         This is required because not all SDKs shut down properly, it is too unpredictable to just assume we can
        ///         gracefully shut down
        ///     </para>
        /// </summary>
        /// <param name="delay">The delay in seconds after which to kill the application (ignored when a debugger is attached)</param>
        /// <param name="restart">Whether or not to restart the application after shutdown (ignored when a debugger is attached)</param>
        public static void Shutdown(int delay, bool restart)
        {
            // Always kill the process after the delay has passed, with all the plugins a graceful shutdown cannot be guaranteed
            string arguments = "-Command \"& {Start-Sleep -s " + delay + "; (Get-Process 'Artemis.UI').kill()}";
            // If restart is required, start the executable again after the process was killed 
            if (restart)
                arguments = "-Command \"& {Start-Sleep -s " + delay + "; (Get-Process 'Artemis.UI').kill(); Start-Process -FilePath '" + Process.GetCurrentProcess().MainModule!.FileName + "'}\"";

            ProcessStartInfo info = new ProcessStartInfo
            {
                Arguments = arguments,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                FileName = "PowerShell.exe"
            };

            if (!Debugger.IsAttached)
                Process.Start(info);

            // Request a graceful shutdown, whatever UI we're running can pick this up
            OnShutdownRequested();
        }

        /// <summary>
        ///     Opens the provided URL in the default web browser
        /// </summary>
        /// <param name="url">The URL to open</param>
        /// <returns>The process created to open the URL</returns>
        public static Process? OpenUrl(string url)
        {
            ProcessStartInfo processInfo = new ProcessStartInfo
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

        #region Events

        /// <summary>
        ///     Occurs when the core has requested an application shutdown
        /// </summary>
        public static event EventHandler? ShutdownRequested;

        private static void OnShutdownRequested()
        {
            ShutdownRequested?.Invoke(null, EventArgs.Empty);
        }

        #endregion
    }
}