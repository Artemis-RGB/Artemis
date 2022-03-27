using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using Humanizer;

namespace Artemis.UI.Linux.Providers.Input
{
    /// <summary>
    /// Data transfer object representing a device read from /proc/bus/input/devices
    /// </summary>
    public class LinuxInputDevice
    {
        public string InputId { get; }
        public string? Bus { get; }
        public string? Vendor { get; }
        public string? Product { get; }
        public string? Version { get; }
        public string? Name { get; }
        public string? Phys { get; }
        public string? Sysfs { get; }
        public string? Uniq { get; }
        public string[]? Handlers { get; }
        public bool IsMouse => Handlers.Any(h => h.Contains("mouse"));
        public bool IsKeyboard => Handlers.Any(h => h.Contains("kbd"));
        public bool IsGamePad => Handlers.Any(h => h.Contains("js"));
        public string EventPath => $"/dev/input/{Handlers.First(h => h.Contains("event"))}";

        public LinuxInputDevice(IEnumerable<string> lines)
        {
            foreach (string line in lines)
            {
                char dataType = line.First();
                string data = line.Substring(3);
                //get the first character in each line and set the according property with relevant data
                switch (dataType)
                {
                    case 'I':
                        InputId = data;
                        foreach (string component in data.Split(" "))
                        {
                            string?[] parts = component.Split('=');
                            switch (parts[0])
                            {
                                case "Bus":
                                    Bus = parts[1];
                                    break;
                                case "Vendor":
                                    Vendor = parts[1];
                                    break;
                                case "Product":
                                    Product = parts[1];
                                    break;
                                case "Version":
                                    Version = parts[1];
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    case 'N':
                        Name = data.Replace("\"", "")
                                   .Replace("Name=", "");
                        break;
                    case 'P':
                        Phys = data.Replace("Phys=", "");
                        break;
                    case 'S':
                        Sysfs = data.Replace("Sysfs=", "");
                        break;
                    case 'H':
                        Handlers = data.Replace("Handlers=", "").Split(" ");
                        break;
                    case 'U':
                        Uniq = data.Replace("Uniq=", "");
                        break;
                    default:
                        //do we need any more of this data?
                        break;
                }
            }
        }
        
        #region Overrides of Object

        /// <inheritdoc />
        public override string ToString() => $"{Name} - {EventPath}";

        #endregion
    }
}