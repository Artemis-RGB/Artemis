using System.Collections.Generic;
using System.Linq;

namespace Artemis.UI.Linux.Providers.Input;

/// <summary>
///     Data transfer object representing a device read from /proc/bus/input/devices
/// </summary>
public class LinuxInputDevice
{
    public LinuxInputDevice(IEnumerable<string> lines)
    {
        foreach (string line in lines)
        {
            //get the first character in each line and set the according property with relevant data

            char dataType = line[0];
            string data = line[3..];
            switch (dataType)
            {
                case 'I':
                    InputId = data;
                    break;
                case 'N':
                    Name = data.Replace("\"", "").Replace("Name=", "");
                    break;
                case 'H':
                    Handlers = data.Replace("Handlers=", "").Split(" ");

                    if (Handlers.Any(h => h.Contains("mouse")))
                        DeviceType = LinuxDeviceType.Mouse;
                    else if (Handlers.Any(h => h.Contains("kbd")))
                        DeviceType = LinuxDeviceType.Keyboard;
                    else if (Handlers.Any(h => h.Contains("js")))
                        DeviceType = LinuxDeviceType.Gamepad;
                    else
                        DeviceType = LinuxDeviceType.Unknown;

                    string evt = Handlers.First(h => h.Contains("event"));

                    EventPath = $"/dev/input/{evt}";
                    break;
            }
        }

        if (InputId is null || Name is null || Handlers is null || EventPath is null)
            throw new ArtemisLinuxInputProviderException("Linux device definition did not contain necessary data");
    }

    public string InputId { get; }
    public string Name { get; }
    public string[] Handlers { get; }
    public string EventPath { get; }
    public LinuxDeviceType DeviceType { get; }

    #region Overrides of Object

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{Name} - {EventPath}";
    }

    #endregion
}