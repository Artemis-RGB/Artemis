using RGB.NET.Core;

namespace Artemis.Core.Tests.Infrastructure.Plugins.DeviceProviders;

/// <summary>
///     An update queue that discards its data, the trigger is never started so nothing runs in the background
/// </summary>
internal sealed class NoOpUpdateQueue() : UpdateQueue(new DeviceUpdateTrigger())
{
    protected override bool Update(ReadOnlySpan<(object key, Color color)> dataSet)
    {
        return true;
    }
}
