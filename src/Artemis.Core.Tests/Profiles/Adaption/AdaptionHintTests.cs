using RGB.NET.Core;

namespace Artemis.Core.Tests.Profiles.Adaption;

public class AdaptionHintTests : AdaptionTestBase
{
    #region Device hints

    [Fact]
    public void DeviceHint_AddsEveryLedOfDevicesOfThatType()
    {
        ArtemisDevice keyboard = AddDevice(AdaptionLayouts.KeyboardAnsi);
        ArtemisDevice strip = AddDevice(AdaptionLayouts.LedStrip, 0, 200);
        Layer layer = AddLayer(Harness.CreateProfile());

        new DeviceAdaptionHint {DeviceType = RGBDeviceType.Keyboard}.Apply(layer, Devices);

        LedIdsOn(layer, keyboard).ShouldBe(AllLedIds(keyboard));
        LedIdsOn(layer, strip).ShouldBeEmpty();
    }

    [Fact]
    public void DeviceHint_ForAllTypesAddsEveryLed()
    {
        ArtemisDevice keyboard = AddDevice(AdaptionLayouts.KeyboardAnsi);
        ArtemisDevice strip = AddDevice(AdaptionLayouts.LedStrip, 0, 200);
        Layer layer = AddLayer(Harness.CreateProfile());

        new DeviceAdaptionHint {DeviceType = RGBDeviceType.All}.Apply(layer, Devices);

        layer.Leds.Count.ShouldBe(keyboard.Leds.Count + strip.Leds.Count);
    }

    [Fact]
    public void DeviceHint_WithoutMatchingDevicesAddsNothing()
    {
        AddDevice(AdaptionLayouts.KeyboardAnsi);
        Layer layer = AddLayer(Harness.CreateProfile());

        new DeviceAdaptionHint {DeviceType = RGBDeviceType.Mouse}.Apply(layer, Devices);

        layer.Leds.ShouldBeEmpty();
    }

    [Fact]
    public void DeviceHint_SkipsAndLimitsDevicesFromTopToBottomThenLeftToRight()
    {
        // Added out of order to show the hint orders them by position
        ArtemisDevice bottom = AddDevice(AdaptionLayouts.KeyboardTklAnsi, 0, 200);
        ArtemisDevice topRight = AddDevice(AdaptionLayouts.KeyboardTklAnsi, 500, 0);
        ArtemisDevice topLeft = AddDevice(AdaptionLayouts.KeyboardTklAnsi, 0, 0);
        Profile profile = Harness.CreateProfile();

        DevicesWithLeds(profile, new DeviceAdaptionHint {DeviceType = RGBDeviceType.Keyboard, Skip = 1}).ShouldBe([topRight, bottom]);
        DevicesWithLeds(profile, new DeviceAdaptionHint {DeviceType = RGBDeviceType.Keyboard, LimitAmount = true, Amount = 2}).ShouldBe([topLeft, topRight]);
        DevicesWithLeds(profile, new DeviceAdaptionHint {DeviceType = RGBDeviceType.Keyboard, Skip = 2, LimitAmount = true, Amount = 1}).ShouldBe([bottom]);
        DevicesWithLeds(profile, new DeviceAdaptionHint {DeviceType = RGBDeviceType.Keyboard, Amount = 1}).ShouldBe([topLeft, topRight, bottom], "the amount is only used when limiting");
    }

    [Fact]
    public void DeviceHint_KeepsTheLayersOtherLeds()
    {
        ArtemisDevice keyboard = AddDevice(AdaptionLayouts.KeyboardAnsi);
        ArtemisDevice strip = AddDevice(AdaptionLayouts.LedStrip, 0, 200);
        Layer layer = AddLayer(Harness.CreateProfile(), [Led(strip, LedId.LedStripe1), Led(keyboard, LedId.Keyboard_A)]);

        new DeviceAdaptionHint {DeviceType = RGBDeviceType.Keyboard}.Apply(layer, Devices);

        layer.Leds.Count.ShouldBe(keyboard.Leds.Count + 1);
        layer.Leds.Distinct().Count().ShouldBe(layer.Leds.Count);
    }

    #endregion

    #region Category hints

    [Fact]
    public void CategoryHint_AddsEveryLedOfDevicesInThatCategory()
    {
        ArtemisDevice keyboard = AddDevice(AdaptionLayouts.KeyboardAnsi);
        ArtemisDevice headset = AddDevice(AdaptionLayouts.Headset, 500, 0);
        ArtemisDevice strip = AddDevice(AdaptionLayouts.LedStrip, 0, 200);
        foreach (ArtemisDevice device in Devices)
            device.ApplyDefaultCategories();
        Layer peripherals = AddLayer(Harness.CreateProfile());
        Layer pcCase = AddLayer(Harness.CreateProfile());

        new CategoryAdaptionHint {Category = DeviceCategory.Peripherals}.Apply(peripherals, Devices);
        new CategoryAdaptionHint {Category = DeviceCategory.Case}.Apply(pcCase, Devices);

        LedIdsOn(peripherals, keyboard).ShouldBe(AllLedIds(keyboard));
        LedIdsOn(peripherals, headset).ShouldBe(AllLedIds(headset));
        LedIdsOn(peripherals, strip).ShouldBeEmpty();
        pcCase.Leds.ShouldBe(strip.Leds, ignoreOrder: true);
    }

    [Fact]
    public void CategoryHint_UsesTheCategoriesSetOnTheDevice()
    {
        ArtemisDevice keyboard = AddDevice(AdaptionLayouts.KeyboardAnsi);
        ArtemisDevice strip = AddDevice(AdaptionLayouts.LedStrip, 0, 200);
        Layer layer = AddLayer(Harness.CreateProfile());

        // New devices have no categories until they are given their defaults
        new CategoryAdaptionHint {Category = DeviceCategory.Peripherals}.Apply(layer, Devices);
        layer.Leds.ShouldBeEmpty();

        strip.Categories.Add(DeviceCategory.Desk);
        keyboard.Categories.Add(DeviceCategory.Desk);
        new CategoryAdaptionHint {Category = DeviceCategory.Desk}.Apply(layer, Devices);
        layer.Leds.Count.ShouldBe(keyboard.Leds.Count + strip.Leds.Count);
    }

    [Fact]
    public void CategoryHint_SkipsAndLimitsDevicesFromTopToBottomThenLeftToRight()
    {
        ArtemisDevice bottom = AddDevice(AdaptionLayouts.LedStrip, 0, 200);
        ArtemisDevice topRight = AddDevice(AdaptionLayouts.LedStrip, 500, 0);
        ArtemisDevice topLeft = AddDevice(AdaptionLayouts.LedStrip, 0, 0);
        foreach (ArtemisDevice device in Devices)
            device.ApplyDefaultCategories();
        Profile profile = Harness.CreateProfile();

        DevicesWithLeds(profile, new CategoryAdaptionHint {Category = DeviceCategory.Case, Skip = 1}).ShouldBe([topRight, bottom]);
        DevicesWithLeds(profile, new CategoryAdaptionHint {Category = DeviceCategory.Case, LimitAmount = true, Amount = 1}).ShouldBe([topLeft]);
        DevicesWithLeds(profile, new CategoryAdaptionHint {Category = DeviceCategory.Case, Skip = 1, LimitAmount = true, Amount = 5}).ShouldBe([topRight, bottom]);
    }

    #endregion

    #region Keyboard section hints

    public static TheoryData<KeyboardSection, LedId[]> SectionsOnAFullSizeKeyboard => new()
    {
        {KeyboardSection.FunctionKeys, Enumerable.Range(0, 12).Select(i => LedId.Keyboard_F1 + i).ToArray()},
        {
            KeyboardSection.NumberKeys, [
                LedId.Keyboard_1, LedId.Keyboard_2, LedId.Keyboard_3, LedId.Keyboard_4, LedId.Keyboard_5,
                LedId.Keyboard_6, LedId.Keyboard_7, LedId.Keyboard_8, LedId.Keyboard_9, LedId.Keyboard_0
            ]
        },
        {
            KeyboardSection.NumPad, [
                LedId.Keyboard_NumLock, LedId.Keyboard_NumSlash, LedId.Keyboard_NumAsterisk, LedId.Keyboard_NumMinus,
                LedId.Keyboard_Num7, LedId.Keyboard_Num8, LedId.Keyboard_Num9, LedId.Keyboard_NumPlus,
                LedId.Keyboard_Num4, LedId.Keyboard_Num5, LedId.Keyboard_Num6,
                LedId.Keyboard_Num1, LedId.Keyboard_Num2, LedId.Keyboard_Num3, LedId.Keyboard_NumEnter,
                LedId.Keyboard_Num0, LedId.Keyboard_NumPeriodAndDelete
            ]
        },
        {KeyboardSection.ArrowKeys, [LedId.Keyboard_ArrowUp, LedId.Keyboard_ArrowLeft, LedId.Keyboard_ArrowDown, LedId.Keyboard_ArrowRight]},
        {
            KeyboardSection.MediaKeys, [
                LedId.Keyboard_MediaMute, LedId.Keyboard_MediaStop, LedId.Keyboard_MediaPreviousTrack,
                LedId.Keyboard_MediaPlay, LedId.Keyboard_MediaNextTrack
            ]
        },
        {KeyboardSection.LedStrips, Enumerable.Range(0, 46).Select(i => LedId.LedStripe1 + i).ToArray()},
        {KeyboardSection.Movement, [LedId.Keyboard_W, LedId.Keyboard_A, LedId.Keyboard_S, LedId.Keyboard_D]},
        // The full size keyboard has no macro keys or extra LEDs
        {KeyboardSection.MacroKeys, []},
        {KeyboardSection.Extra, []}
    };

    [Theory]
    [MemberData(nameof(SectionsOnAFullSizeKeyboard))]
    public void KeyboardSectionHint_AddsTheKeysOfThatSection(KeyboardSection section, LedId[] expected)
    {
        ArtemisDevice keyboard = AddDevice(AdaptionLayouts.KeyboardAnsi);
        Layer layer = AddLayer(Harness.CreateProfile());

        new KeyboardSectionAdaptionHint {Section = section}.Apply(layer, Devices);

        LedIdsOn(layer, keyboard).ShouldBe(expected.Order().ToArray());
    }

    [Fact]
    public void KeyboardSectionHint_AddsTheMacroKeysOfAnXlKeyboard()
    {
        ArtemisDevice keyboard = AddDevice(AdaptionLayouts.KeyboardXlAnsi);
        Layer layer = AddLayer(Harness.CreateProfile());

        new KeyboardSectionAdaptionHint {Section = KeyboardSection.MacroKeys}.Apply(layer, Devices);

        LedIdsOn(layer, keyboard).ShouldBe(Enumerable.Range(0, 6).Select(i => LedId.Keyboard_Programmable1 + i).ToArray());
    }

    [Fact]
    public void KeyboardSectionHint_OnlyLooksAtKeyboards()
    {
        AddDevice(AdaptionLayouts.LedStrip, 0, 200);
        Layer layer = AddLayer(Harness.CreateProfile());

        new KeyboardSectionAdaptionHint {Section = KeyboardSection.LedStrips}.Apply(layer, Devices);

        layer.Leds.ShouldBeEmpty();
    }

    [Fact]
    public void KeyboardSectionHint_AppliesToEveryKeyboard()
    {
        ArtemisDevice first = AddDevice(AdaptionLayouts.KeyboardAnsi);
        ArtemisDevice second = AddDevice(AdaptionLayouts.KeyboardTklAnsi, 0, 200);
        Layer layer = AddLayer(Harness.CreateProfile());

        new KeyboardSectionAdaptionHint {Section = KeyboardSection.FunctionKeys}.Apply(layer, Devices);

        LedIdsOn(layer, first).Length.ShouldBe(12);
        LedIdsOn(layer, second).Length.ShouldBe(12);
    }

    [Fact]
    public void KeyboardSectionHint_MovementUsesZqsdOnAzertyKeyboards()
    {
        ArtemisDevice keyboard = AddDevice(AdaptionLayouts.AzertyKeys);
        Layer layer = AddLayer(Harness.CreateProfile());

        new KeyboardSectionAdaptionHint {Section = KeyboardSection.Movement}.Apply(layer, Devices);

        LedIdsOn(layer, keyboard).ShouldBe(new[] {LedId.Keyboard_Z, LedId.Keyboard_Q, LedId.Keyboard_S, LedId.Keyboard_D}.Order().ToArray());
    }

    #endregion

    #region Single LED hints

    [Fact]
    public void SingleLedHint_AddsThatLedOfEveryDeviceThatHasIt()
    {
        ArtemisDevice first = AddDevice(AdaptionLayouts.KeyboardAnsi);
        ArtemisDevice second = AddDevice(AdaptionLayouts.KeyboardTklAnsi, 0, 200);
        ArtemisDevice strip = AddDevice(AdaptionLayouts.LedStrip, 0, 400);
        Layer layer = AddLayer(Harness.CreateProfile());

        new SingleLedAdaptionHint {LedId = LedId.Keyboard_Escape}.Apply(layer, Devices);

        layer.Leds.ShouldBe([Led(first, LedId.Keyboard_Escape), Led(second, LedId.Keyboard_Escape)], ignoreOrder: true);
        LedIdsOn(layer, strip).ShouldBeEmpty();
    }

    [Fact]
    public void SingleLedHint_SkipsAndLimitsDevicesFromTopToBottomThenLeftToRight()
    {
        ArtemisDevice bottom = AddDevice(AdaptionLayouts.KeyboardTklAnsi, 0, 200);
        ArtemisDevice topRight = AddDevice(AdaptionLayouts.KeyboardTklAnsi, 500, 0);
        ArtemisDevice topLeft = AddDevice(AdaptionLayouts.KeyboardTklAnsi, 0, 0);
        Profile profile = Harness.CreateProfile();

        DevicesWithLeds(profile, new SingleLedAdaptionHint {LedId = LedId.Keyboard_Escape, LimitAmount = true, Amount = 1}).ShouldBe([topLeft]);
        DevicesWithLeds(profile, new SingleLedAdaptionHint {LedId = LedId.Keyboard_Escape, Skip = 1}).ShouldBe([topRight, bottom]);
        DevicesWithLeds(profile, new SingleLedAdaptionHint {LedId = LedId.Keyboard_Escape, Skip = 1, LimitAmount = true, Amount = 1}).ShouldBe([topRight]);
    }

    [Fact]
    public void SingleLedHint_ForAnLedNoDeviceHasAddsNothing()
    {
        AddDevice(AdaptionLayouts.KeyboardAnsi);
        Layer layer = AddLayer(Harness.CreateProfile());

        new SingleLedAdaptionHint {LedId = LedId.Mouse1}.Apply(layer, Devices);

        layer.Leds.ShouldBeEmpty();
    }

    #endregion

    #region Descriptions and storage

    [Fact]
    public void ToString_DescribesTheHint()
    {
        new CategoryAdaptionHint {Category = DeviceCategory.Desk, Skip = 1, LimitAmount = true, Amount = 2}.ToString()
            .ShouldBe("Category adaption - Category: Desk, Skip: 1, LimitAmount: True, Amount: 2");
        new DeviceAdaptionHint {DeviceType = RGBDeviceType.Mouse, Skip = 3}.ToString()
            .ShouldBe("Device adaption - DeviceType: Mouse, Skip: 3, LimitAmount: False, Amount: 0");
        new KeyboardSectionAdaptionHint {Section = KeyboardSection.NumPad}.ToString()
            .ShouldBe("Keyboard section adaption - Section: NumPad");
        new SingleLedAdaptionHint {LedId = LedId.Keyboard_Escape, Amount = 1, LimitAmount = true}.ToString()
            .ShouldBe("Single LED adaption - LedId: Keyboard_Escape, Skip: 0, LimitAmount: True, Amount: 1");
    }

    [Fact]
    public void Properties_RaisePropertyChanged()
    {
        CategoryAdaptionHint hint = new();
        List<string?> changed = [];
        hint.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        hint.Category = DeviceCategory.Room;
        hint.Skip = 1;
        hint.LimitAmount = true;
        hint.Amount = 2;

        changed.ShouldBe([nameof(CategoryAdaptionHint.Category), nameof(CategoryAdaptionHint.Skip), nameof(CategoryAdaptionHint.LimitAmount), nameof(CategoryAdaptionHint.Amount)]);
    }

    #endregion

    /// <summary>
    ///     Applies the hint to a new layer and returns the devices that got LEDs, in the order they were added
    /// </summary>
    private List<ArtemisDevice> DevicesWithLeds(Profile profile, IAdaptionHint hint)
    {
        Layer layer = AddLayer(profile);
        hint.Apply(layer, Devices);
        return layer.Leds.Select(l => l.Device).Distinct().ToList();
    }
}
