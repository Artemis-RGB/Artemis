using Artemis.Core.Tests.Infrastructure.Plugins.DeviceProviders;

namespace Artemis.Core.Tests.Profiles.Adaption;

/// <summary>
///     The device layouts used by the adaption tests on top of <see cref="TestLayouts" />
/// </summary>
public static class AdaptionLayouts
{
    private static readonly string Root = Path.Combine(AppContext.BaseDirectory, "DefaultLayouts", "Artemis");

    public static string KeyboardAnsi => TestLayouts.KeyboardAnsi;
    public static string KeyboardTklAnsi => Keyboard("Artemis TKL keyboard-ANSI");
    public static string KeyboardXlAnsi => Keyboard("Artemis XL keyboard-ANSI");
    public static string Headset => Path.Combine(Root, "Headset", "Artemis 2 LEDs headset.xml");
    public static string LedStrip => TestLayouts.SmallLedStrip;

    /// <summary>
    ///     Just the keys around the movement keys of an AZERTY keyboard, A sits above Q
    /// </summary>
    public static string AzertyKeys => Path.Combine(AppContext.BaseDirectory, "TestLayouts", "Test AZERTY keys.xml");

    /// <summary>
    ///     The names of every keyboard layout shipped with Artemis, use <see cref="Keyboard" /> to get its path
    /// </summary>
    public static TheoryData<string> KeyboardNames => new(
        "Artemis L keyboard-ANSI", "Artemis L keyboard-ISO",
        "Artemis TKL keyboard-ANSI", "Artemis TKL keyboard-ISO",
        "Artemis XL keyboard-ANSI", "Artemis XL keyboard-ISO"
    );

    /// <summary>
    ///     Gets the path to the keyboard layout shipped with Artemis by its name
    /// </summary>
    public static string Keyboard(string name)
    {
        return Path.Combine(Root, "Keyboard", name + ".xml");
    }
}
