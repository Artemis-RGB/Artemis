namespace Artemis.Core.Tests.Infrastructure.Plugins.DeviceProviders;

/// <summary>
///     Paths to the device layouts shipped with Artemis.Core, and test layouts shipped with the tests
/// </summary>
public static class TestLayouts
{
    private static readonly string Root = Path.Combine(AppContext.BaseDirectory, "DefaultLayouts", "Artemis");

    public static string KeyboardAnsi => Path.Combine(Root, "Keyboard", "Artemis L keyboard-ANSI.xml");
    public static string KeyboardIso => Path.Combine(Root, "Keyboard", "Artemis L keyboard-ISO.xml");

    /// <summary>
    ///     A strip of 20 small 3x3 LEDs, 4 units apart. Smaller than a canvas pixel at a render scale of 0.25
    /// </summary>
    public static string SmallLedStrip => Path.Combine(AppContext.BaseDirectory, "TestLayouts", "Test LED strip.xml");
}
