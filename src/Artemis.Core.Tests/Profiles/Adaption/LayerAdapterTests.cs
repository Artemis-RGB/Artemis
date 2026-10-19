using Artemis.Storage.Entities.Profile;
using RGB.NET.Core;

namespace Artemis.Core.Tests.Profiles.Adaption;

public class LayerAdapterTests : AdaptionTestBase
{
    #region Adapting without hints

    [Fact]
    public void Adapt_WithoutHintsFindsTheSameKeysOnAnotherKeyboard()
    {
        ArtemisDevice full = AddDevice(AdaptionLayouts.KeyboardAnsi);
        Profile profile = Harness.CreateProfile();
        AddLayer(profile, Leds(full, LedId.Keyboard_W, LedId.Keyboard_A, LedId.Keyboard_S, LedId.Keyboard_D));
        ProfileEntity saved = Harness.Save(profile);

        Harness.RemoveDevice(full);
        ArtemisDevice tenkeyless = AddDevice(AdaptionLayouts.KeyboardTklAnsi);
        Layer layer = Harness.Load(saved).GetAllLayers().Single();
        layer.Leds.ShouldBeEmpty("the stored LEDs belong to a different device");

        layer.Adapter.Adapt(Devices);

        layer.Leds.ShouldBe(Leds(tenkeyless, LedId.Keyboard_W, LedId.Keyboard_A, LedId.Keyboard_S, LedId.Keyboard_D), ignoreOrder: true);
    }

    [Fact]
    public void Adapt_WithoutHintsSkipsKeysTheOtherKeyboardDoesNotHave()
    {
        ArtemisDevice full = AddDevice(AdaptionLayouts.KeyboardAnsi);
        Profile profile = Harness.CreateProfile();
        AddLayer(profile, Leds(full, LedId.Keyboard_Escape, LedId.Keyboard_Num5));
        ProfileEntity saved = Harness.Save(profile);

        Harness.RemoveDevice(full);
        ArtemisDevice tenkeyless = AddDevice(AdaptionLayouts.KeyboardTklAnsi);
        Layer layer = Harness.Load(saved).GetAllLayers().Single();
        layer.Adapter.Adapt(Devices);

        layer.Leds.ShouldBe([Led(tenkeyless, LedId.Keyboard_Escape)]);
    }

    [Fact]
    public void Adapt_WithoutHintsUsesEachKeyOnce()
    {
        ArtemisDevice full = AddDevice(AdaptionLayouts.KeyboardAnsi);
        Profile profile = Harness.CreateProfile();
        AddLayer(profile, Leds(full, LedId.Keyboard_Escape));
        ProfileEntity saved = Harness.Save(profile);

        Harness.RemoveDevice(full);
        ArtemisDevice first = AddDevice(AdaptionLayouts.KeyboardTklAnsi, 0, 200);
        AddDevice(AdaptionLayouts.KeyboardTklAnsi, 0, 0);
        Layer layer = Harness.Load(saved).GetAllLayers().Single();
        layer.Adapter.Adapt(Devices);

        // The first device in the list gets the key, regardless of where the devices are
        layer.Leds.ShouldBe([Led(first, LedId.Keyboard_Escape)]);
    }

    [Fact]
    public void Adapt_WithoutHintsWorksFromTheStoredLeds()
    {
        ArtemisDevice keyboard = AddDevice(AdaptionLayouts.KeyboardAnsi);
        Profile profile = Harness.CreateProfile();
        Layer layer = AddLayer(profile, Leds(keyboard, LedId.Keyboard_Escape));
        Harness.Save(profile);
        layer.AddLed(Led(keyboard, LedId.Keyboard_F1));
        layer.ClearLeds();

        layer.Adapter.Adapt(Devices);

        // The LEDs the layer had when it was last saved, not the ones it has now
        layer.Leds.ShouldBe([Led(keyboard, LedId.Keyboard_Escape)]);
    }

    [Fact]
    public void Adapt_KeepsTheLayersCurrentLeds()
    {
        ArtemisDevice keyboard = AddDevice(AdaptionLayouts.KeyboardAnsi);
        Profile profile = Harness.CreateProfile();
        Layer layer = AddLayer(profile, Leds(keyboard, LedId.Keyboard_Escape));
        layer.Adapter.Add(new SingleLedAdaptionHint {LedId = LedId.Keyboard_F1});

        layer.Adapter.Adapt(Devices);

        // Callers clear the layer first if they want only the adapted LEDs, like the editor's "apply adaption hints" does
        layer.Leds.ShouldBe(Leds(keyboard, LedId.Keyboard_Escape, LedId.Keyboard_F1), ignoreOrder: true);
    }

    [Fact]
    public void Adapt_WithHintsIgnoresTheStoredLeds()
    {
        ArtemisDevice full = AddDevice(AdaptionLayouts.KeyboardAnsi);
        Profile profile = Harness.CreateProfile();
        Layer original = AddLayer(profile, Leds(full, LedId.Keyboard_W, LedId.Keyboard_A, LedId.Keyboard_S, LedId.Keyboard_D));
        original.Adapter.Add(new SingleLedAdaptionHint {LedId = LedId.Keyboard_Escape});
        ProfileEntity saved = Harness.Save(profile);

        Harness.RemoveDevice(full);
        ArtemisDevice tenkeyless = AddDevice(AdaptionLayouts.KeyboardTklAnsi);
        Layer layer = Harness.Load(saved).GetAllLayers().Single();
        layer.Adapter.Adapt(Devices);

        layer.Leds.ShouldBe([Led(tenkeyless, LedId.Keyboard_Escape)]);
    }

    [Fact]
    public void Adapt_AppliesEveryHint()
    {
        ArtemisDevice keyboard = AddDevice(AdaptionLayouts.KeyboardAnsi);
        ArtemisDevice strip = AddDevice(AdaptionLayouts.LedStrip, 0, 200);
        Layer layer = AddLayer(Harness.CreateProfile());
        layer.Adapter.Add(new KeyboardSectionAdaptionHint {Section = KeyboardSection.FunctionKeys});
        layer.Adapter.Add(new DeviceAdaptionHint {DeviceType = RGBDeviceType.LedStripe});

        layer.Adapter.Adapt(Devices);

        LedIdsOn(layer, keyboard).Length.ShouldBe(12);
        LedIdsOn(layer, strip).ShouldBe(AllLedIds(strip));
    }

    #endregion

    #region Determining hints

    [Fact]
    public void DetermineHints_ForALayerCoveringEverythingAddsAHintForAllDevices()
    {
        ArtemisDevice keyboard = AddDevice(AdaptionLayouts.KeyboardAnsi);
        ArtemisDevice strip = AddDevice(AdaptionLayouts.LedStrip, 0, 200);
        Layer layer = AddLayer(Harness.CreateProfile(), [..keyboard.Leds, ..strip.Leds]);

        List<IAdaptionHint> hints = layer.Adapter.DetermineHints(Devices);

        DeviceAdaptionHint hint = hints.Single().ShouldBeOfType<DeviceAdaptionHint>();
        hint.DeviceType.ShouldBe(RGBDeviceType.All);
        layer.Adapter.AdaptionHints.ShouldBe(hints);
    }

    [Fact]
    public void DetermineHints_ForALayerCoveringADeviceAddsAHintForItsTypeAndCategory()
    {
        ArtemisDevice keyboard = AddDevice(AdaptionLayouts.KeyboardAnsi);
        AddDevice(AdaptionLayouts.LedStrip, 0, 200);
        foreach (ArtemisDevice device in Devices)
            device.ApplyDefaultCategories();
        Layer layer = AddLayer(Harness.CreateProfile(), keyboard.Leds);

        List<IAdaptionHint> hints = layer.Adapter.DetermineHints(Devices);

        hints.Count.ShouldBe(2);
        hints[0].ShouldBeOfType<DeviceAdaptionHint>().DeviceType.ShouldBe(RGBDeviceType.Keyboard);
        hints[1].ShouldBeOfType<CategoryAdaptionHint>().Category.ShouldBe(DeviceCategory.Peripherals);
    }

    [Fact]
    public void DetermineHints_OnlyAddsACategoryHintWhenEveryDeviceInItIsCovered()
    {
        ArtemisDevice keyboard = AddDevice(AdaptionLayouts.KeyboardAnsi);
        AddDevice(AdaptionLayouts.Headset, 500, 0);
        foreach (ArtemisDevice device in Devices)
            device.ApplyDefaultCategories();
        Layer layer = AddLayer(Harness.CreateProfile(), keyboard.Leds);

        List<IAdaptionHint> hints = layer.Adapter.DetermineHints(Devices);

        hints.Single().ShouldBeOfType<DeviceAdaptionHint>().DeviceType.ShouldBe(RGBDeviceType.Keyboard);
    }

    [Fact]
    public void DetermineHints_ForASingleLedAddsAHintForOneSuchLed()
    {
        ArtemisDevice keyboard = AddDevice(AdaptionLayouts.KeyboardAnsi);
        Layer layer = AddLayer(Harness.CreateProfile(), Leds(keyboard, LedId.Keyboard_Escape));

        List<IAdaptionHint> hints = layer.Adapter.DetermineHints(Devices);

        SingleLedAdaptionHint hint = hints.Single().ShouldBeOfType<SingleLedAdaptionHint>();
        hint.LedId.ShouldBe(LedId.Keyboard_Escape);
        hint.LimitAmount.ShouldBeTrue();
        hint.Amount.ShouldBe(1);
        hint.Skip.ShouldBe(0);
    }

    [Fact]
    public void DetermineHints_ForAFewLedsAddsNothing()
    {
        ArtemisDevice keyboard = AddDevice(AdaptionLayouts.KeyboardAnsi);
        Layer layer = AddLayer(Harness.CreateProfile(), Leds(keyboard, LedId.Keyboard_W, LedId.Keyboard_A, LedId.Keyboard_S, LedId.Keyboard_D));

        layer.Adapter.DetermineHints(Devices).ShouldBeEmpty();
        layer.Adapter.AdaptionHints.ShouldBeEmpty();
    }

    [Fact]
    public void DetermineHints_DoesNotRepeatExistingHints()
    {
        ArtemisDevice keyboard = AddDevice(AdaptionLayouts.KeyboardAnsi);
        AddDevice(AdaptionLayouts.LedStrip, 0, 200);
        foreach (ArtemisDevice device in Devices)
            device.ApplyDefaultCategories();
        Layer layer = AddLayer(Harness.CreateProfile(), keyboard.Leds);

        layer.Adapter.DetermineHints(Devices);
        List<IAdaptionHint> second = layer.Adapter.DetermineHints(Devices);

        second.ShouldBeEmpty();
        layer.Adapter.AdaptionHints.Count.ShouldBe(2);
    }

    [Fact]
    public void DetermineHints_WithoutDevicesAddsAHintForAllDevices()
    {
        Layer layer = AddLayer(Harness.CreateProfile());

        // Every one of no devices is covered
        layer.Adapter.DetermineHints([]).Single().ShouldBeOfType<DeviceAdaptionHint>().DeviceType.ShouldBe(RGBDeviceType.All);
    }

    [Fact]
    public void DetermineHints_ThenAdaptingRestoresTheLayer()
    {
        ArtemisDevice keyboard = AddDevice(AdaptionLayouts.KeyboardAnsi);
        ArtemisDevice strip = AddDevice(AdaptionLayouts.LedStrip, 0, 200);
        foreach (ArtemisDevice device in Devices)
            device.ApplyDefaultCategories();
        Profile profile = Harness.CreateProfile();
        Layer wholeKeyboard = AddLayer(profile, keyboard.Leds);
        Layer escape = AddLayer(profile, Leds(keyboard, LedId.Keyboard_Escape));
        Layer wholeStrip = AddLayer(profile, strip.Leds);

        foreach (Layer layer in profile.GetAllLayers())
        {
            List<ArtemisLed> before = layer.Leds.ToList();
            layer.Adapter.DetermineHints(Devices);
            layer.ClearLeds();
            layer.Adapter.Adapt(Devices);
            layer.Leds.ShouldBe(before, ignoreOrder: true, layer.Name);
        }

        wholeKeyboard.Adapter.AdaptionHints.Count.ShouldBe(2);
        escape.Adapter.AdaptionHints.Count.ShouldBe(1);
        wholeStrip.Adapter.AdaptionHints.Count.ShouldBe(2);
    }

    #endregion

    #region Managing hints

    [Fact]
    public void Add_AddsTheHintOnceAndRaisesAnEvent()
    {
        AddDevice(AdaptionLayouts.KeyboardAnsi);
        LayerAdapter adapter = AddLayer(Harness.CreateProfile()).Adapter;
        List<IAdaptionHint> added = [];
        adapter.AdapterHintAdded += (_, e) => added.Add(e.AdaptionHint);
        DeviceAdaptionHint hint = new();

        adapter.Add(hint);
        adapter.Add(hint);

        adapter.AdaptionHints.ShouldBe([hint]);
        added.ShouldBe([hint]);
    }

    [Fact]
    public void Remove_RemovesTheHintAndRaisesAnEvent()
    {
        AddDevice(AdaptionLayouts.KeyboardAnsi);
        LayerAdapter adapter = AddLayer(Harness.CreateProfile()).Adapter;
        DeviceAdaptionHint hint = new();
        adapter.Add(hint);
        List<IAdaptionHint> removed = [];
        adapter.AdapterHintRemoved += (_, e) => removed.Add(e.AdaptionHint);

        adapter.Remove(hint);
        adapter.Remove(hint);

        adapter.AdaptionHints.ShouldBeEmpty();
        removed.ShouldBe([hint]);
    }

    [Fact]
    public void Clear_RemovesEveryHintOneByOne()
    {
        AddDevice(AdaptionLayouts.KeyboardAnsi);
        LayerAdapter adapter = AddLayer(Harness.CreateProfile()).Adapter;
        DeviceAdaptionHint first = new();
        CategoryAdaptionHint second = new();
        adapter.Add(first);
        adapter.Add(second);
        List<IAdaptionHint> removed = [];
        adapter.AdapterHintRemoved += (_, e) => removed.Add(e.AdaptionHint);

        adapter.Clear();

        adapter.AdaptionHints.ShouldBeEmpty();
        removed.ShouldBe([first, second]);
    }

    [Fact]
    public void Adapter_BelongsToItsLayer()
    {
        AddDevice(AdaptionLayouts.KeyboardAnsi);
        Layer layer = AddLayer(Harness.CreateProfile());

        layer.Adapter.Layer.ShouldBeSameAs(layer);
    }

    #endregion

    #region Storage

    [Fact]
    public void Hints_AreStoredWithTheProfile()
    {
        AddDevice(AdaptionLayouts.KeyboardAnsi);
        Profile profile = Harness.CreateProfile();
        Layer layer = AddLayer(profile);
        layer.Adapter.Add(new DeviceAdaptionHint {DeviceType = RGBDeviceType.Keyboard, Skip = 1, LimitAmount = true, Amount = 2});
        layer.Adapter.Add(new CategoryAdaptionHint {Category = DeviceCategory.Desk, Skip = 3, LimitAmount = true, Amount = 4});
        layer.Adapter.Add(new KeyboardSectionAdaptionHint {Section = KeyboardSection.NumPad});
        layer.Adapter.Add(new SingleLedAdaptionHint {LedId = LedId.Keyboard_Escape, Skip = 5, LimitAmount = true, Amount = 6});

        Layer reloaded = Harness.SaveAndReload(profile).GetAllLayers().Single();

        reloaded.Adapter.AdaptionHints.Select(h => h.ToString()).ShouldBe(layer.Adapter.AdaptionHints.Select(h => h.ToString()));
        reloaded.Adapter.AdaptionHints.Select(h => h.GetType()).ShouldBe([
            typeof(DeviceAdaptionHint), typeof(CategoryAdaptionHint), typeof(KeyboardSectionAdaptionHint), typeof(SingleLedAdaptionHint)
        ]);
    }

    [Fact]
    public void Load_ReplacesTheHintsWithTheStoredOnes()
    {
        AddDevice(AdaptionLayouts.KeyboardAnsi);
        Profile profile = Harness.CreateProfile();
        Layer layer = AddLayer(profile);
        layer.Adapter.Add(new KeyboardSectionAdaptionHint {Section = KeyboardSection.NumPad});
        layer.Save();
        List<IAdaptionHint> removed = [];
        layer.Adapter.AdapterHintRemoved += (_, e) => removed.Add(e.AdaptionHint);
        layer.Adapter.Add(new DeviceAdaptionHint());

        layer.Adapter.Load();

        layer.Adapter.AdaptionHints.Single().ShouldBeOfType<KeyboardSectionAdaptionHint>().Section.ShouldBe(KeyboardSection.NumPad);
        removed.ShouldBeEmpty("loading drops the current hints without reporting them as removed");
    }

    #endregion
}
