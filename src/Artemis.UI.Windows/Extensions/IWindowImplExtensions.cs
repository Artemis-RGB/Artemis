using System.Linq;
using Artemis.UI.Exceptions;
using Avalonia.Platform;

namespace Artemis.UI.Windows.Extensions;

public static class IWindowImplExtensions
{
    public static IPlatformHandle GetHandle(this IWindowImpl window)
    {
        // This is unfortunate
        IPlatformHandle? handle = (IPlatformHandle?) window.GetType().GetProperties().FirstOrDefault(p => p.Name == "Handle")?.GetValue(window);
        if (handle == null)
            throw new ArtemisUIException("Could not get IWindowImpl internal platform handle, Avalonia API change?");

        return handle;
    }
}