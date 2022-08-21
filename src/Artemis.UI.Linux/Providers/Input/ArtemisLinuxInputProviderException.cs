using System;

namespace Artemis.UI.Linux.Providers.Input;

public class ArtemisLinuxInputProviderException : Exception
{
    public ArtemisLinuxInputProviderException()
    {
    }

    public ArtemisLinuxInputProviderException(string? message) : base(message)
    {
    }

    public ArtemisLinuxInputProviderException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}