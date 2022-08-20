using Artemis.Core;
using System.Collections.Generic;
using System.Linq;

namespace Artemis.UI.Linux.Providers.Input
{
    /// <summary>
    /// Data transfer object representing a device read from /proc/bus/input/devices
    /// </summary>
    public class LinuxInputDevice
    {
        public LinuxInputId InputId { get; }
        public string Name { get; }
        public string[] Handlers { get; }
        public string EventPath { get; }
        public LinuxDeviceType DeviceType { get; }

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
                        InputId = new LinuxInputId(data);
                        break;
                    case 'N':
                        Name = data.Replace("\"", "").Replace("Name=", "");
                        break;
                    case 'H':
                        Handlers = data.Replace("Handlers=", "").Split(" ");

                        if (Handlers?.Any(h => h.Contains("mouse")) == true)
                            DeviceType = LinuxDeviceType.Mouse;
                        else if (Handlers?.Any(h => h.Contains("kbd")) == true)
                            DeviceType = LinuxDeviceType.Keyboard;
                        else if (Handlers?.Any(h => h.Contains("js")) == true)
                            DeviceType = LinuxDeviceType.Gamepad;

                        string evt = Handlers!.First(h => h.Contains("event"));

                        EventPath = $"/dev/input/{evt}";
                        break;
                    default:
                        //do we need any more of this data?
                        break;
                }
            }

            if (InputId is null || Name is null || Handlers is null || EventPath is null)
            {
                throw new ArtemisLinuxInputProviderException("Linux device definition did not contain necessary data");
            }
        }

        #region Overrides of Object

        /// <inheritdoc />
        public override string ToString() => $"{Name} - {EventPath}";

        #endregion

        public class LinuxInputId
        {
            public string Bus { get; }
            public string Vendor { get; }
            public string Product { get; }
            public string Version { get; }

            public LinuxInputId(string line)
            {
                Dictionary<string, string> components = line.Split(" ")
                    .Select(c => c.Split('='))
                    .ToDictionary(c => c[0], c => c[1]);

                Bus = components["Bus"];
                Vendor = components["Vendor"];
                Product = components["Product"];
                Version = components["Version"];
            }

            public override string ToString() => $"Bus={Bus} Vendor={Vendor} Product={Product} Version={Version}";
        }
    }
}