using Artemis.Core.Services;
using Artemis.Core.Tests.Infrastructure.Plugins.DeviceProviders;
using Artemis.Core.Tests.Infrastructure.Services;
using Artemis.Storage.Entities.Surface;
using RGB.NET.Core;
using RGB.NET.Layout;

namespace Artemis.Core.Tests.Services;

public class DeviceServiceTests : ServiceTestBase
{
    private static readonly string KeyboardTkl = Path.Combine(AppContext.BaseDirectory, "DefaultLayouts", "Artemis", "Keyboard", "Artemis TKL keyboard-ANSI.xml");
    private static readonly string Headset = Path.Combine(AppContext.BaseDirectory, "DefaultLayouts", "Artemis", "Headset", "Artemis 4 LEDs headset.xml");

    private IDeviceService Devices => Resolve<IDeviceService>();

    [Fact]
    public void AddDeviceProvider_AddsItsDevices()
    {
        List<ArtemisDevice> added = [];
        List<DeviceProviderEventArgs> providersAdded = [];
        int ledsChanged = 0;
        Devices.DeviceAdded += (_, e) => added.Add(e.Device);
        Devices.DeviceProviderAdded += (_, e) => providersAdded.Add(e);
        Devices.LedsChanged += (_, _) => ledsChanged++;

        TestDeviceProvider provider = AddDeviceProvider(TestLayouts.KeyboardAnsi, TestLayouts.SmallLedStrip);

        Devices.Devices.Count.ShouldBe(2);
        Devices.EnabledDevices.ShouldBe(Devices.Devices, ignoreOrder: true);
        Devices.Devices.ShouldAllBe(d => d.DeviceProvider == provider);
        added.ShouldBe(Devices.Devices, ignoreOrder: true);
        providersAdded.Single().DeviceProvider.ShouldBeSameAs(provider);
        providersAdded.Single().Devices.ShouldBe(Devices.Devices, ignoreOrder: true);
        // Once for the layout of each device and once when all devices are in
        ledsChanged.ShouldBe(3);
    }

    [Fact]
    public void AddDeviceProvider_StoresNewDevices()
    {
        AddDeviceProvider(TestLayouts.KeyboardAnsi);

        using var context = Services.CreateDbContext();
        DeviceEntity stored = context.Devices.Single();
        stored.Id.ShouldBe(Devices.Devices.Single().Identifier);
        stored.IsEnabled.ShouldBeTrue();
    }

    [Fact]
    public void AddDeviceProvider_GivesDevicesTheDefaultLayout()
    {
        AddDeviceProvider(TestLayouts.KeyboardAnsi);

        // The test plugin ships no layouts, so the layout comes from the default layouts in Artemis.Core
        ArtemisDevice keyboard = Devices.Devices.Single();
        keyboard.LayoutSelection.Type.ShouldBe("Default");
        keyboard.Layout.ShouldNotBeNull();
        keyboard.Layout.IsDefaultLayout.ShouldBeTrue();
    }

    [Theory]
    [InlineData("Artemis 1 LED headset.xml", true)]
    [InlineData("Artemis 2 LEDs headset.xml", false)]
    [InlineData("Artemis 4 LEDs headset.xml", false)]
    public void AddDeviceProvider_GivesHeadsetsTheDefaultLayoutForTheirNumberOfLeds(string layoutFile, bool getsLayout)
    {
        AddDeviceProvider(Path.Combine(AppContext.BaseDirectory, "DefaultLayouts", "Artemis", "Headset", layoutFile));

        // Pinned as-is: the default headset layouts are looked up as "Artemis 2 LED headset.xml" and "Artemis 4 LED
        // headset.xml", but the files are named "... LEDs headset.xml", so headsets with 2 or more LEDs get no default
        // layout (the device service logs it as an invalid layout)
        ArtemisDevice headset = Devices.Devices.Single();
        if (getsLayout)
            headset.Layout!.IsDefaultLayout.ShouldBeTrue();
        else
            headset.Layout.ShouldBeNull();
    }

    [Fact]
    public void AddDeviceProvider_WithoutDevices_AddsNothing()
    {
        int providersAdded = 0;
        Devices.DeviceProviderAdded += (_, _) => providersAdded++;

        AddDeviceProvider();

        Devices.Devices.ShouldBeEmpty();
        providersAdded.ShouldBe(0);
    }

    [Fact]
    public void AddDeviceProvider_ASecondTime_ReplacesItsDevices()
    {
        TestDeviceProvider provider = AddDeviceProvider(TestLayouts.KeyboardAnsi);
        ArtemisDevice before = Devices.Devices.Single();
        List<ArtemisDevice> removed = [];
        Devices.DeviceRemoved += (_, e) => removed.Add(e.Device);

        Devices.AddDeviceProvider(provider);

        removed.ShouldBe([before]);
        Devices.Devices.Single().ShouldNotBeSameAs(before);
        Devices.Devices.Single().Identifier.ShouldBe(before.Identifier);
    }

    [Fact]
    public void AddDeviceProvider_RestoresStoredDeviceSettings()
    {
        TestDeviceProvider provider = AddDeviceProvider(TestLayouts.KeyboardAnsi);
        ArtemisDevice keyboard = Devices.Devices.Single();
        keyboard.X = 100;
        keyboard.Y = 50;
        keyboard.Scale = 2;
        keyboard.RedScale = 0.5f;
        Devices.SaveDevice(keyboard);
        Devices.RemoveDeviceProvider(provider);

        Devices.AddDeviceProvider(provider);

        ArtemisDevice restored = Devices.Devices.Single();
        restored.ShouldNotBeSameAs(keyboard);
        restored.X.ShouldBe(100);
        restored.Y.ShouldBe(50);
        restored.Scale.ShouldBe(2);
        restored.RedScale.ShouldBe(0.5f);
    }

    [Fact]
    public void AddDeviceProvider_KeepsDisabledDevicesDisabled()
    {
        TestDeviceProvider provider = AddDeviceProvider(TestLayouts.KeyboardAnsi);
        Devices.DisableDevice(Devices.Devices.Single());
        Devices.RemoveDeviceProvider(provider);

        Devices.AddDeviceProvider(provider);

        Devices.Devices.Single().IsEnabled.ShouldBeFalse();
        Devices.EnabledDevices.ShouldBeEmpty();
    }

    [Fact]
    public void Devices_AreSortedByZIndex()
    {
        TestDeviceProvider provider = AddDeviceProvider(TestLayouts.KeyboardAnsi, TestLayouts.SmallLedStrip);
        ArtemisDevice strip = Devices.Devices.Single(d => d.DeviceType == RGBDeviceType.LedStripe);
        ArtemisDevice keyboard = Devices.Devices.Single(d => d.DeviceType == RGBDeviceType.Keyboard);
        strip.ZIndex = 1;
        keyboard.ZIndex = 2;
        Devices.SaveDevices();

        // Sorting happens when devices are added
        Devices.AddDeviceProvider(provider);

        Devices.Devices.Select(d => d.DeviceType).ShouldBe([RGBDeviceType.LedStripe, RGBDeviceType.Keyboard]);
        Devices.EnabledDevices.Select(d => d.DeviceType).ShouldBe([RGBDeviceType.LedStripe, RGBDeviceType.Keyboard]);
    }

    [Fact]
    public void RemoveDeviceProvider_RemovesItsDevices()
    {
        TestDeviceProvider keyboardProvider = AddDeviceProvider(TestLayouts.KeyboardAnsi);
        AddDeviceProvider(new SuspendableTestDeviceProvider(), TestLayouts.SmallLedStrip);
        ArtemisDevice keyboard = Devices.Devices.Single(d => d.DeviceProvider == keyboardProvider);
        List<ArtemisDevice> removed = [];
        List<DeviceProviderEventArgs> providersRemoved = [];
        int ledsChanged = 0;
        Devices.DeviceRemoved += (_, e) => removed.Add(e.Device);
        Devices.DeviceProviderRemoved += (_, e) => providersRemoved.Add(e);
        Devices.LedsChanged += (_, _) => ledsChanged++;

        Devices.RemoveDeviceProvider(keyboardProvider);

        Devices.Devices.ShouldNotContain(keyboard);
        Devices.EnabledDevices.ShouldNotContain(keyboard);
        Devices.Devices.Count.ShouldBe(1);
        removed.ShouldBe([keyboard]);
        providersRemoved.Single().Devices.ShouldBe([keyboard]);
        ledsChanged.ShouldBe(1);
    }

    [Fact]
    public void DisableAndEnableDevice_UpdateTheEnabledDevicesAndStoreIt()
    {
        AddDeviceProvider(TestLayouts.KeyboardAnsi);
        ArtemisDevice keyboard = Devices.Devices.Single();
        List<string> events = [];
        Devices.DeviceDisabled += (_, _) => events.Add("disabled");
        Devices.DeviceEnabled += (_, _) => events.Add("enabled");
        Devices.LedsChanged += (_, _) => events.Add("leds changed");

        Devices.DisableDevice(keyboard);
        Devices.DisableDevice(keyboard);
        keyboard.IsEnabled.ShouldBeFalse();
        Devices.EnabledDevices.ShouldBeEmpty();
        Devices.Devices.ShouldBe([keyboard]);
        StoredDevice().IsEnabled.ShouldBeFalse();

        Devices.EnableDevice(keyboard);
        Devices.EnableDevice(keyboard);
        keyboard.IsEnabled.ShouldBeTrue();
        Devices.EnabledDevices.ShouldBe([keyboard]);
        StoredDevice().IsEnabled.ShouldBeTrue();

        // Doing it a second time does nothing
        events.ShouldBe(["disabled", "leds changed", "enabled", "leds changed"]);
    }

    [Fact]
    public void SaveDevice_StoresTheDevice()
    {
        AddDeviceProvider(TestLayouts.KeyboardAnsi);
        ArtemisDevice keyboard = Devices.Devices.Single();
        int ledsChanged = 0;
        Devices.LedsChanged += (_, _) => ledsChanged++;
        keyboard.X = 25;
        keyboard.Rotation = 90;

        Devices.SaveDevice(keyboard);

        StoredDevice().X.ShouldBe(25);
        StoredDevice().Rotation.ShouldBe(90);
        ledsChanged.ShouldBe(1);
    }

    [Fact]
    public void LoadDeviceLayout_WithTheNoneLayoutType_RemovesTheLayout()
    {
        AddDeviceProvider(TestLayouts.KeyboardAnsi);
        ArtemisDevice keyboard = Devices.Devices.Single();

        keyboard.LayoutSelection.Type = "None";
        Devices.LoadDeviceLayout(keyboard);

        keyboard.Layout.ShouldBeNull();
        keyboard.LayoutSelection.ErrorState.ShouldBeNull();
    }

    [Fact]
    public void LoadDeviceLayout_WithAnUnknownLayoutType_RemovesTheLayout()
    {
        AddDeviceProvider(TestLayouts.KeyboardAnsi);
        ArtemisDevice keyboard = Devices.Devices.Single();

        keyboard.LayoutSelection.Type = "Something unknown";
        Devices.LoadDeviceLayout(keyboard);

        keyboard.Layout.ShouldBeNull();
    }

    [Fact]
    public void LoadDeviceLayout_BackToTheDefaultType_RestoresTheLayout()
    {
        AddDeviceProvider(TestLayouts.KeyboardAnsi);
        ArtemisDevice keyboard = Devices.Devices.Single();
        keyboard.LayoutSelection.Type = "None";
        Devices.LoadDeviceLayout(keyboard);

        keyboard.LayoutSelection.Type = "Default";
        Devices.LoadDeviceLayout(keyboard);

        keyboard.Layout.ShouldNotBeNull();
        keyboard.Layout.IsDefaultLayout.ShouldBeTrue();
    }

    [Fact]
    public void ChangingTheRenderScale_RaisesLedsChanged()
    {
        AddDeviceProvider(TestLayouts.KeyboardAnsi);
        int ledsChanged = 0;
        Devices.LedsChanged += (_, _) => ledsChanged++;

        // Keep the current value, other tests rely on it
        RenderScale.SetRenderScaleMultiplier(RenderScale.RenderScaleMultiplier);

        ledsChanged.ShouldBe(1);
    }

    [Fact]
    public void AutoArrangeDevices_PlacesDevicesAroundTheKeyboard()
    {
        AddDeviceProvider(TestLayouts.KeyboardAnsi, TestLayouts.SmallLedStrip, Headset);
        using RGBSurface surface = AttachToSurface();
        ArtemisDevice keyboard = Devices.Devices.Single(d => d.DeviceType == RGBDeviceType.Keyboard);
        ArtemisDevice headset = Devices.Devices.Single(d => d.DeviceType == RGBDeviceType.Headset);
        ArtemisDevice strip = Devices.Devices.Single(d => d.DeviceType == RGBDeviceType.LedStripe);

        Devices.AutoArrangeDevices(false);

        // There's no keypad to place the keyboard next to, so it goes 10 to the right of where arranging started
        (keyboard.X, keyboard.Y).ShouldBe((10f, 0f));
        // The headset is centered 100 below the keyboard (the keyboard's own 10 margin included)
        (headset.X, headset.Y).ShouldBe((10 + 441 / 2f - 242 / 2f, 144 + 10 + 100f));
        // There's no fan to place the LED strip next to, so it goes 10 to the right of everything else
        (strip.X, strip.Y).ShouldBe((10 + 441 + 10f, 0f));

        // Pinned as-is: only devices placed by one of their type's own rules get the z-index of their type. LED strips
        // normally get 2, but placed by the fallback (no fan to place it next to) it keeps the 1 every new device starts with
        strip.ZIndex.ShouldBe(1);
    }

    [Fact]
    public void AutoArrangeDevices_StacksWideDevicesOfTheSameTypeVertically()
    {
        AddDeviceProvider(TestLayouts.KeyboardAnsi, KeyboardTkl);
        using RGBSurface surface = AttachToSurface();

        Devices.AutoArrangeDevices(false);

        List<ArtemisDevice> keyboards = Devices.Devices.OrderBy(d => d.Y).ToList();
        keyboards.ShouldAllBe(d => d.X == 10);
        keyboards[0].Y.ShouldBe(0);
        // Half the margin of 10 between them
        keyboards[1].Y.ShouldBe(keyboards[0].RgbDevice.Size.Height + 5);
    }

    [Fact]
    public void AutoArrangeDevices_LeftHanded_PlacesTheHeadsetTheSameWay()
    {
        AddDeviceProvider(TestLayouts.KeyboardAnsi, Headset);
        using RGBSurface surface = AttachToSurface();
        ArtemisDevice headset = Devices.Devices.Single(d => d.DeviceType == RGBDeviceType.Headset);

        Devices.AutoArrangeDevices(true);

        // Without a mouse or mousepad the left-handed arrangement places the keyboard and headset like the right-handed one
        (headset.X, headset.Y).ShouldBe((10 + 441 / 2f - 242 / 2f, 144 + 10 + 100f));
    }

    [Fact]
    public void AutoArrangeDevices_AppliesDefaultCategoriesAndStoresTheDevices()
    {
        AddDeviceProvider(TestLayouts.KeyboardAnsi, TestLayouts.SmallLedStrip);
        using RGBSurface surface = AttachToSurface();

        Devices.AutoArrangeDevices(false);

        Devices.Devices.Single(d => d.DeviceType == RGBDeviceType.Keyboard).Categories.ShouldBe([DeviceCategory.Peripherals]);
        Devices.Devices.Single(d => d.DeviceType == RGBDeviceType.LedStripe).Categories.ShouldBe([DeviceCategory.Case]);
        string stripId = Devices.Devices.Single(d => d.DeviceType == RGBDeviceType.LedStripe).Identifier;
        using var context = Services.CreateDbContext();
        context.Devices.Single(d => d.Id == stripId).X.ShouldBe(461);
    }

    [Fact]
    public void AutoArrangeDevices_WithoutASurface_UsesTheOldPositions()
    {
        AddDeviceProvider(TestLayouts.KeyboardAnsi, TestLayouts.SmallLedStrip);
        ArtemisDevice strip = Devices.Devices.Single(d => d.DeviceType == RGBDeviceType.LedStripe);

        Devices.AutoArrangeDevices(false);

        // Arranging measures where devices are on the RGB.NET surface, which only follows the Artemis position while the
        // device is attached to one. Artemis attaches devices when it starts rendering, here the keyboard is still at 0
        // on the surface, so the strip ends up 10 closer than with a surface (461)
        strip.X.ShouldBe(451);
    }

    [Fact]
    public void SuspendAndResumeDeviceProviders_OnlyAffectProvidersThatSupportIt()
    {
        // The render service, which the device service pauses while suspending, needs the module service
        ResolveModuleService();
        SuspendableTestDeviceProvider suspendable = EnableFeature(new SuspendableTestDeviceProvider());
        TestDeviceProvider regular = AddDeviceProvider(TestLayouts.KeyboardAnsi);

        Devices.SuspendDeviceProviders();

        Devices.SuspendedDeviceProviders.ShouldBe([suspendable]);
        suspendable.Suspended.ShouldBe(1);
        suspendable.IsEnabled.ShouldBeFalse();
        regular.IsEnabled.ShouldBeTrue();

        // Suspending again doesn't suspend it twice
        Devices.SuspendDeviceProviders();
        suspendable.Suspended.ShouldBe(1);

        Devices.ResumeDeviceProviders();

        Devices.SuspendedDeviceProviders.ShouldBeEmpty();
        suspendable.IsEnabled.ShouldBeTrue();
        Resolve<IRenderService>().IsPaused.ShouldBeTrue();
    }

    private DeviceEntity StoredDevice()
    {
        using var context = Services.CreateDbContext();
        return context.Devices.Single();
    }

    /// <summary>
    ///     Attaches the devices to an RGB.NET surface, like the render service does when Artemis starts rendering
    /// </summary>
    private RGBSurface AttachToSurface()
    {
        RGBSurface surface = new();
        surface.Attach(Devices.Devices.Select(d => d.RgbDevice));
        return surface;
    }
}
