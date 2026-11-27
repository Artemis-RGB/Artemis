using Artemis.Core.Tests.Infrastructure.Plugins.LayerBrushes;
using Artemis.Core.Tests.Infrastructure.Plugins.LayerEffects;
using Artemis.Core.Tests.Infrastructure.Rendering;
using RGB.NET.Core;
using SkiaSharp;

namespace Artemis.Core.Tests.Profiles;

/// <summary>
///     A folder with a variety of children, so folder behaviour can't pass by accident on a single solid layer:
///     overlapping layers, a gradient, a per-LED brush and a nested folder with a half transparent layer and a layer that
///     has an effect of its own. Next to the folder the profile contains a layer that isn't part of it.
/// </summary>
public sealed class BusyFolder
{
    private BusyFolder(Folder folder, Folder nested, Layer outside, ArtemisLed[] leds, ArtemisLed[] nestedLeds)
    {
        Folder = folder;
        Nested = nested;
        Outside = outside;
        Leds = leds;
        NestedLeds = nestedLeds;
    }

    /// <summary>
    ///     Gets the busy folder
    /// </summary>
    public Folder Folder { get; }

    /// <summary>
    ///     Gets the folder nested inside the busy folder
    /// </summary>
    public Folder Nested { get; }

    /// <summary>
    ///     Gets the layer next to the busy folder, it does not overlap any of the folder's LEDs
    /// </summary>
    public Layer Outside { get; }

    /// <summary>
    ///     Gets all LEDs covered by the busy folder, including those of the nested folder
    /// </summary>
    public ArtemisLed[] Leds { get; }

    /// <summary>
    ///     Gets the LEDs covered by the nested folder
    /// </summary>
    public ArtemisLed[] NestedLeds { get; }

    /// <summary>
    ///     Adds a busy folder to the root of the profile, with a layer below it on LEDs the folder doesn't cover.
    /// </summary>
    public static BusyFolder Create(RenderHarness harness, ArtemisDevice keyboard, Profile profile)
    {
        ArtemisLed[] Leds(params LedId[] ids) => ids.Select(id => keyboard.GetLed(id, false)!).ToArray();

        Folder folder = harness.AddFolder(profile.GetRootFolder(), "Busy folder");

        // Solid red on top, partially covering a blue layer below it
        (_, SolidColorTestBrush solid) = harness.AddLayer<SolidColorTestBrush>(folder, Leds(LedId.Keyboard_Q, LedId.Keyboard_W, LedId.Keyboard_E, LedId.Keyboard_R), "Solid");
        solid.Properties.Color.SetCurrentValue(SKColors.Red);
        (_, SolidColorTestBrush covered) = harness.AddLayer<SolidColorTestBrush>(folder, Leds(LedId.Keyboard_R, LedId.Keyboard_T, LedId.Keyboard_Y), "Partially covered");
        covered.Properties.Color.SetCurrentValue(SKColors.Blue);

        // A gradient, giving a range of colors
        (_, GradientTestBrush gradient) = harness.AddLayer<GradientTestBrush>(folder, Leds(LedId.Keyboard_U, LedId.Keyboard_I, LedId.Keyboard_O, LedId.Keyboard_P), "Gradient");
        gradient.Properties.Left.SetCurrentValue(SKColors.Lime);
        gradient.Properties.Right.SetCurrentValue(new SKColor(255, 0, 255));

        // A per-LED brush, which renders through a different code path
        (_, PerLedTestBrush perLed) = harness.AddLayer<PerLedTestBrush>(folder, Leds(LedId.Keyboard_A, LedId.Keyboard_S, LedId.Keyboard_D, LedId.Keyboard_F), "Per LED");
        perLed.ColorSelector = (led, _) => led.RgbLed.Id switch
        {
            LedId.Keyboard_A => SKColors.Red,
            LedId.Keyboard_S => SKColors.Lime,
            LedId.Keyboard_D => SKColors.Blue,
            _ => SKColors.Yellow
        };

        // A nested folder with a half transparent layer and a layer with an effect of its own
        Folder nested = harness.AddFolder(folder, "Nested folder");
        (Layer halfTransparent, SolidColorTestBrush cyan) = harness.AddLayer<SolidColorTestBrush>(nested, Leds(LedId.Keyboard_Z, LedId.Keyboard_X, LedId.Keyboard_C), "Half transparent");
        cyan.Properties.Color.SetCurrentValue(SKColors.Cyan);
        halfTransparent.Transform.Opacity.SetCurrentValue(50);
        (Layer tinted, SolidColorTestBrush white) = harness.AddLayer<SolidColorTestBrush>(nested, Leds(LedId.Keyboard_V, LedId.Keyboard_B, LedId.Keyboard_N), "Tinted");
        white.Properties.Color.SetCurrentValue(SKColors.White);
        harness.AddEffect<TintTestEffect>(tinted).Properties.Color.SetCurrentValue(new SKColor(255, 128, 0));

        // Below the folder, on LEDs the folder doesn't cover
        (Layer outside, SolidColorTestBrush orange) = harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), Leds(
            LedId.Keyboard_G, LedId.Keyboard_H, LedId.Keyboard_J, LedId.Keyboard_1, LedId.Keyboard_2, LedId.Keyboard_3
        ), "Outside");
        orange.Properties.Color.SetCurrentValue(new SKColor(255, 160, 32));

        ArtemisLed[] nestedLeds = nested.GetAllLayers().SelectMany(l => l.Leds).Distinct().ToArray();
        ArtemisLed[] leds = folder.GetAllLayers().SelectMany(l => l.Leds).Distinct().ToArray();
        return new BusyFolder(folder, nested, outside, leds, nestedLeds);
    }
}
