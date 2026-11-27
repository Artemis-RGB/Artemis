using Artemis.Core.Services;

namespace Artemis.Core.Tests.Infrastructure.Services;

/// <summary>
///     An input provider the test presses keys on, the real providers hook into the operating system
/// </summary>
public class TestInputProvider : InputProvider
{
    /// <summary>
    ///     Presses and releases the key
    /// </summary>
    public void Press(KeyboardKey key)
    {
        OnKeyboardDataReceived(null, key, true);
        OnKeyboardDataReceived(null, key, false);
    }
}
