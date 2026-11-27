using Artemis.Core.Tests.Infrastructure;
using Artemis.Core.Tests.Infrastructure.Rendering;
using Artemis.Storage.Entities.Profile;
using RGB.NET.Core;
using SkiaSharp;

namespace Artemis.Core.Tests.Profiles.Adaption;

/// <summary>
///     Profiles made on a full size ANSI keyboard and adapted to each keyboard Artemis ships a layout for, the way
///     Artemis adapts a profile that is imported or shared through the workshop
/// </summary>
public class ProfileAdaptionTests : AdaptionTestBase
{
    [Theory]
    [MemberData(nameof(AdaptionLayouts.KeyboardNames), MemberType = typeof(AdaptionLayouts))]
    public void Profile_AdaptsToEveryKeyboard(string keyboardName)
    {
        ProfileEntity saved = CreateProfileOnAFullSizeKeyboard();

        ArtemisDevice keyboard = AddDevice(AdaptionLayouts.Keyboard(keyboardName));
        Profile profile = Harness.Load(saved);
        AdaptProfile(profile);

        LedIdsOn(Layer(profile, "Everything"), keyboard).ShouldBe(AllLedIds(keyboard));
        LedIdsOn(Layer(profile, "F-keys"), keyboard).ShouldBe(Enumerable.Range(0, 12).Select(i => LedId.Keyboard_F1 + i).ToArray());
        LedIdsOn(Layer(profile, "Escape"), keyboard).ShouldBe([LedId.Keyboard_Escape]);
        LedIdsOn(Layer(profile, "WASD"), keyboard).ShouldBe(new[] {LedId.Keyboard_W, LedId.Keyboard_A, LedId.Keyboard_S, LedId.Keyboard_D}.Order().ToArray());
        LedIdsOn(Layer(profile, "Numpad"), keyboard).Length.ShouldBe(keyboardName.Contains("TKL") ? 0 : 17);
    }

    [Fact]
    public void Profile_AdaptsToTwoKeyboards()
    {
        ProfileEntity saved = CreateProfileOnAFullSizeKeyboard();

        ArtemisDevice tenkeyless = AddDevice(AdaptionLayouts.KeyboardTklAnsi);
        ArtemisDevice extraLarge = AddDevice(AdaptionLayouts.KeyboardXlAnsi, 0, 200);
        Profile profile = Harness.Load(saved);
        AdaptProfile(profile);

        // Hints apply to every keyboard, without hints each LED goes to the first keyboard only
        Layer(profile, "Everything").Leds.Count.ShouldBe(tenkeyless.Leds.Count + extraLarge.Leds.Count);
        LedIdsOn(Layer(profile, "F-keys"), extraLarge).Length.ShouldBe(12);
        LedIdsOn(Layer(profile, "Escape"), tenkeyless).ShouldBe([LedId.Keyboard_Escape]);
        LedIdsOn(Layer(profile, "Escape"), extraLarge).ShouldBeEmpty("the hint is limited to one LED");
        LedIdsOn(Layer(profile, "WASD"), tenkeyless).Length.ShouldBe(4);
        LedIdsOn(Layer(profile, "WASD"), extraLarge).ShouldBeEmpty();
        LedIdsOn(Layer(profile, "Numpad"), extraLarge).Length.ShouldBe(17);
    }

    [Fact]
    public void AdaptedProfile_RendersOnTheNewKeyboard()
    {
        ProfileEntity saved = CreateProfileOnAFullSizeKeyboard(["F-keys"]);

        ArtemisDevice keyboard = AddDevice(AdaptionLayouts.KeyboardXlAnsi);
        Profile profile = Harness.Load(saved);
        AdaptProfile(profile);
        using RenderFrame frame = Harness.Render(profile, TimeSpan.FromSeconds(0.1));

        for (LedId ledId = LedId.Keyboard_F1; ledId <= LedId.Keyboard_F12; ledId++)
            frame.GetColor(keyboard, ledId).ShouldBeColor(SKColors.Red, context: ledId.ToString());
        frame.GetColor(keyboard, LedId.Keyboard_Escape).ShouldBeColor(SKColors.Black);
    }

    /// <summary>
    ///     Creates a profile on a full size ANSI keyboard and returns it as stored, the keyboard is removed again
    /// </summary>
    /// <param name="only">The names of the layers to keep, defaults to all of them</param>
    private ProfileEntity CreateProfileOnAFullSizeKeyboard(string[]? only = null)
    {
        ArtemisDevice keyboard = AddDevice(AdaptionLayouts.KeyboardAnsi);
        Profile profile = Harness.CreateProfile();

        // Gets a hint for all devices
        AddLayer(profile, keyboard.Leds, "Everything").Adapter.DetermineHints(Devices);
        // Gets a single LED hint
        AddLayer(profile, Leds(keyboard, LedId.Keyboard_Escape), "Escape").Adapter.DetermineHints(Devices);
        // Gets a keyboard section hint, picked by hand
        Layer functionKeys = AddLayer(profile, Enumerable.Range(0, 12).Select(i => Led(keyboard, LedId.Keyboard_F1 + i)), "F-keys");
        functionKeys.Adapter.Add(new KeyboardSectionAdaptionHint {Section = KeyboardSection.FunctionKeys});
        Layer numpad = AddLayer(profile, [], "Numpad");
        numpad.Adapter.Add(new KeyboardSectionAdaptionHint {Section = KeyboardSection.NumPad});
        // No hints, adapting finds the same keys
        AddLayer(profile, Leds(keyboard, LedId.Keyboard_W, LedId.Keyboard_A, LedId.Keyboard_S, LedId.Keyboard_D), "WASD");

        if (only != null)
        {
            foreach (Layer layer in profile.GetAllLayers().Where(l => !only.Contains(l.Name)))
                layer.Parent.RemoveChild(layer);
        }

        ProfileEntity saved = Harness.Save(profile);
        Harness.RemoveDevice(keyboard);
        return saved;
    }

    /// <summary>
    ///     Adapts every layer the way ProfileService.AdaptProfile does
    /// </summary>
    private void AdaptProfile(Profile profile)
    {
        foreach (Layer layer in profile.GetAllLayers())
            layer.Adapter.Adapt(Devices);
    }

    private static Layer Layer(Profile profile, string name)
    {
        return profile.GetAllLayers().Single(l => l.Name == name);
    }
}
