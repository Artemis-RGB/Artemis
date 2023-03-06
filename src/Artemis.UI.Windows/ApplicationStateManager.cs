using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Windows.Utilities;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using DryIoc;

namespace Artemis.UI.Windows;

public class ApplicationStateManager
{
    private const int SM_SHUTTINGDOWN = 0x2000;

    public ApplicationStateManager(IContainer container, string[] startupArguments)
    {
        StartupArguments = startupArguments;
        IsElevated = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

        Core.Utilities.ShutdownRequested += UtilitiesOnShutdownRequested;
        Core.Utilities.RestartRequested += UtilitiesOnRestartRequested;
        Core.Utilities.UpdateRequested += UtilitiesOnUpdateRequested;

        // On Windows shutdown dispose the IOC container just so device providers get a chance to clean up
        if (Application.Current?.ApplicationLifetime is IControlledApplicationLifetime controlledApplicationLifetime)
            controlledApplicationLifetime.Exit += (_, _) =>
            {
                RunForcedShutdownIfEnabled();

                // Dispose plugins before disposing the IOC container because plugins might access services during dispose
                container.Resolve<IPluginManagementService>().Dispose();
                container.Dispose();
            };

        // Inform the Core about elevation status
        container.Resolve<ICoreService>().IsElevated = IsElevated;
    }

    public string[] StartupArguments { get; }
    public bool IsElevated { get; }

    private void UtilitiesOnRestartRequested(object? sender, RestartEventArgs e)
    {
        List<string> argsList = new();
        argsList.AddRange(StartupArguments);
        if (e.ExtraArgs != null)
            argsList.AddRange(e.ExtraArgs.Except(argsList));
        string args = argsList.Any() ? "-ArgumentList " + string.Join(',', argsList) : "";
        string command =
            $"-Command \"& {{Start-Sleep -Milliseconds {(int) e.Delay.TotalMilliseconds}; " +
            "(Get-Process 'Artemis.UI.Windows').kill(); " +
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
        if (Application.Current?.ApplicationLifetime is IControlledApplicationLifetime controlledApplicationLifetime)
            Dispatcher.UIThread.Post(() => controlledApplicationLifetime.Shutdown());
    }

    private void UtilitiesOnUpdateRequested(object? sender, UpdateEventArgs e)
    {
        List<string> argsList = new(StartupArguments);
        if (e.Silent)
            argsList.Add("--autorun");

        // Retain startup arguments after update by providing them to the script
        string script = $"\"{Path.Combine(Constants.UpdatingFolder, "installing", "scripts", "update.ps1")}\"";
        string source = $"-sourceDirectory \"{Path.Combine(Constants.UpdatingFolder, "installing")}\"";
        string destination = $"-destinationDirectory \"{Constants.ApplicationFolder}\"";
        string args = argsList.Any() ? $"-artemisArgs \"{string.Join(',', argsList)}\"" : "";

        // Run the PowerShell script included in the new version, that way any changes made to the script are used
        ProcessStartInfo info = new()
        {
            Arguments = $"-File {script} {source} {destination} {args}",
            FileName = "PowerShell.exe"
        };
        Process.Start(info);

        // Lets try a graceful shutdown, PowerShell will kill if needed
        if (Application.Current?.ApplicationLifetime is IControlledApplicationLifetime controlledApplicationLifetime)
            Dispatcher.UIThread.Post(() => controlledApplicationLifetime.Shutdown());
    }

    private void UtilitiesOnShutdownRequested(object? sender, EventArgs e)
    {
        // Use PowerShell to kill the process after 8 sec just in case
        RunForcedShutdownIfEnabled();

        if (Application.Current?.ApplicationLifetime is IControlledApplicationLifetime controlledApplicationLifetime)
            Dispatcher.UIThread.Post(() => controlledApplicationLifetime.Shutdown());
    }

    private void RunForcedShutdownIfEnabled()
    {
        // Don't run a forced shutdown if Windows itself is shutting down, the new PowerShell process will fail
        if (GetSystemMetrics(SM_SHUTTINGDOWN) != 0 || StartupArguments.Contains("--disable-forced-shutdown"))
            return;

        ProcessStartInfo info = new()
        {
            Arguments = "-Command \"& {Start-Sleep -s 8; (Get-Process -Id " + Process.GetCurrentProcess().Id + ").kill()}",
            WindowStyle = ProcessWindowStyle.Hidden,
            CreateNoWindow = true,
            FileName = "PowerShell.exe"
        };
        Process.Start(info);
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);
}