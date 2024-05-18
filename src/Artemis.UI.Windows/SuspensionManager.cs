using System;
using System.Threading.Tasks;
using Artemis.Core.Services;
using DryIoc;
using Microsoft.Win32;
using Serilog;

namespace Artemis.UI.Windows;

public class SuspensionManager
{
    private readonly ILogger _logger;
    private readonly IDeviceService _deviceService;

    public SuspensionManager(IContainer container)
    {
        _logger = container.Resolve<ILogger>();
        _deviceService = container.Resolve<IDeviceService>();

        try
        {
            SystemEvents.PowerModeChanged += SystemEventsOnPowerModeChanged;
            SystemEvents.SessionSwitch += SystemEventsOnSessionSwitch;
        }
        catch (Exception e)
        {
            _logger.Warning(e, "Could not subscribe to system events");
        }
    }

    private void SystemEventsOnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
    {
        if (e.Mode == PowerModes.Suspend)
            Task.Run(() => SetDeviceSuspension(true));
        else if (e.Mode == PowerModes.Resume)
            Task.Run(() => SetDeviceSuspension(false));
    }

    private void SystemEventsOnSessionSwitch(object sender, SessionSwitchEventArgs e)
    {
        if (e.Reason is SessionSwitchReason.SessionLock or SessionSwitchReason.SessionLogoff)
            Task.Run(() => SetDeviceSuspension(true));
        else if (e.Reason is SessionSwitchReason.SessionUnlock or SessionSwitchReason.SessionLogon)
            Task.Run(() => SetDeviceSuspension(false));
    }

    private async Task SetDeviceSuspension(bool suspend)
    {
        try
        {
            if (suspend)
            {
                // Suspend instantly, system is going into sleep at any moment
                _deviceService.SuspendDeviceProviders();
            }
            else
            {
                await Task.Delay(TimeSpan.FromSeconds(2));
                _deviceService.ResumeDeviceProviders();
            }
        }
        catch (Exception e)
        {
            _logger.Error(e, "An error occurred while setting provider suspension to {Suspension}", suspend);
        }
    }
}