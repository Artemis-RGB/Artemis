﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Artemis.Core;
using Artemis.UI.Utilities;
using Ninject;
using Stylet;

namespace Artemis.UI
{
    public class ApplicationStateManager
    {
        // ReSharper disable once NotAccessedField.Local - Kept in scope to ensure it does not get released
        private Mutex _artemisMutex;

        public ApplicationStateManager(IKernel kernel, string[] startupArguments)
        {
            StartupArguments = startupArguments;
            IsElevated = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

            Core.Utilities.ShutdownRequested += UtilitiesOnShutdownRequested;
            Core.Utilities.RestartRequested += UtilitiesOnRestartRequested;

            // On Windows shutdown dispose the kernel just so device providers get a chance to clean up
            Application.Current.SessionEnding += (_, _) => kernel.Dispose();
        }

        public string[] StartupArguments { get; }
        public bool IsElevated { get; }

        public bool FocusExistingInstance()
        {
            _artemisMutex = new Mutex(true, "Artemis-3c24b502-64e6-4587-84bf-9072970e535d", out bool createdNew);
            if (createdNew)
                return false;

            try
            {
                // Blocking is required here otherwise Artemis shuts down before the remote call gets a chance to finish
                RemoteFocus().GetAwaiter().GetResult();
            }
            catch (Exception)
            {
                // Not much could go wrong here but this code runs so early it'll crash if something does go wrong
                return true;
            }

            return true;
        }

        private async Task RemoteFocus()
        {
            // At this point we cannot read the database yet to retrieve the web server port.
            // Instead use the method external applications should use as well.
            if (!File.Exists(Path.Combine(Constants.DataFolder, "webserver.txt")))
                return;

            string url = await File.ReadAllTextAsync(Path.Combine(Constants.DataFolder, "webserver.txt"));
            using HttpClient client = new();
            await client.PostAsync(url + "remote/bring-to-foreground", null!);
        }

        private void UtilitiesOnRestartRequested(object sender, RestartEventArgs e)
        {
            List<string> argsList = new();
            argsList.AddRange(StartupArguments);
            if (e.ExtraArgs != null)
                argsList.AddRange(e.ExtraArgs.Except(argsList));
            string args = argsList.Any() ? "-ArgumentList " + string.Join(',', argsList) : "";
            string command =
                $"-Command \"& {{Start-Sleep -Milliseconds {(int) e.Delay.TotalMilliseconds}; " +
                "(Get-Process 'Artemis.UI').kill(); " +
                $"Start-Process -FilePath '{Constants.ExecutablePath}' -WorkingDirectory '{Constants.ApplicationFolder}' {args}}}\"";
            // Elevated always runs with RunAs
            if (e.Elevate)
            {
                ProcessStartInfo info = new()
                {
                    Arguments = command.Replace("}\"", " -Verb RunAs}\""),
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    FileName = "PowerShell.exe"
                };
                Process.Start(info);
            }
            // Non-elevated runs regularly if currently not elevated
            else if (!IsElevated)
            {
                ProcessStartInfo info = new()
                {
                    Arguments = command,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    FileName = "PowerShell.exe"
                };
                Process.Start(info);
            }
            // Non-elevated runs via a utility method is currently elevated (de-elevating is hacky)
            else
            {
                string powerShell = Path.Combine(Environment.SystemDirectory, "WindowsPowerShell", "v1.0", "powershell.exe");
                ProcessUtilities.RunAsDesktopUser(powerShell, command, true);
            }

            // Lets try a graceful shutdown, PowerShell will kill if needed
            Execute.OnUIThread(() => Application.Current.Shutdown());
        }

        private void UtilitiesOnShutdownRequested(object sender, EventArgs e)
        {
            // Use PowerShell to kill the process after 2 sec just in case
            ProcessStartInfo info = new()
            {
                Arguments = "-Command \"& {Start-Sleep -s 2; (Get-Process 'Artemis.UI').kill()}",
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                FileName = "PowerShell.exe"
            };
            Process.Start(info);

            Execute.OnUIThread(() => Application.Current.Shutdown());
        }
    }
}