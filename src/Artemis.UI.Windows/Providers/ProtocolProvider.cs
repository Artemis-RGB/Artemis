using System;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.UI.Shared.Providers;
using Microsoft.Win32;

namespace Artemis.UI.Windows.Providers;

public class ProtocolProvider : IProtocolProvider
{
    /// <inheritdoc />
    public async Task AssociateWithProtocol(string protocol)
    {
        string key = $"HKEY_CURRENT_USER\\Software\\Classes\\{protocol}";
        Registry.SetValue($"{key}", null, "URL:artemis protocol");
        Registry.SetValue($"{key}", "URL Protocol", "");
        Registry.SetValue($"{key}\\DefaultIcon", null, $"\"{Constants.ExecutablePath}\",1");
        Registry.SetValue($"{key}\\shell\\open\\command", null, $"\"{Constants.ExecutablePath}\", \"--route=%1\"");
    }

    /// <inheritdoc />
    public async Task DisassociateWithProtocol(string protocol)
    {
        try
        {
            string key = $"HKEY_CURRENT_USER\\Software\\Classes\\{protocol}";
            Registry.CurrentUser.DeleteSubKeyTree(key);
        }
        catch (ArgumentException)
        {
            // Ignore errors (which means that the protocol wasn't associated before)
        }
    }
}