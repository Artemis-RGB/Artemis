using Artemis.Core.Tests.Infrastructure.Plugins.DeviceProviders;

namespace Artemis.Core.Tests.Infrastructure.Services;

/// <summary>
///     A test device provider that supports being suspended, like providers that need to let go of the hardware while the
///     computer sleeps
/// </summary>
public class SuspendableTestDeviceProvider : TestDeviceProvider
{
    public SuspendableTestDeviceProvider()
    {
        SuspendSupported = true;
    }

    /// <summary>
    ///     Gets the number of times the provider was suspended
    /// </summary>
    public int Suspended { get; private set; }

    public override void Suspend()
    {
        Suspended++;
    }
}
