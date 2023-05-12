using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Artemis.UI.Linux.Providers.Input;

public static class LinuxInputDeviceFinder
{
    private const string DEVICES_FILE = "/proc/bus/input/devices";

    public static IEnumerable<LinuxInputDevice> Find()
    {
        IEnumerable<IEnumerable<string>> lineGroups = File.ReadAllLines(DEVICES_FILE).PartitionBy(s => s?.Length == 0); //split on empty lines 

        foreach (IEnumerable<string> lineGroup in lineGroups)
        {
            LinuxInputDevice device;
            
            try
            {
                device = new LinuxInputDevice(lineGroup);
            }
            catch
            {
                continue;
                //some devices don't have all the required data, we can ignore those
            }
            
            if (ShouldReadDevice(device))
            {
                yield return device;
            }
        }
    }

    private static bool ShouldReadDevice(LinuxInputDevice device)
    {
        if (device.DeviceType == LinuxDeviceType.Unknown)
            return false;
        //possibly add more checks here
        
        return true;
    }

    //https://stackoverflow.com/questions/56623354
    private static IEnumerable<IEnumerable<T>> PartitionBy<T>(this IEnumerable<T> a, Func<T, bool> predicate)
    {
        int groupNumber = 0;
        Func<bool, int?> getGroupNumber = skip =>
        {
            if (skip)
            {
                // prepare next group, we don't care if we increment more than once
                // we only want to split groups
                groupNumber++;
                // null, to be able to filter out group separators
                return null;
            }

            return groupNumber;
        };
        return a
            .Select(x => new { Value = x, GroupNumber = getGroupNumber(predicate(x)) })
            .Where(x => x.GroupNumber != null)
            .GroupBy(x => x.GroupNumber)
            .Select(g => g.Select(x => x.Value));
    }
}