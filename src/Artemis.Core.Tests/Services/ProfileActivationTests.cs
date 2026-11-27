using Artemis.Core.Modules;
using Artemis.Core.Services;
using Artemis.Core.Tests.Infrastructure.Plugins.DeviceProviders;
using Artemis.Core.Tests.Infrastructure.Plugins.LayerBrushes;
using Artemis.Core.Tests.Infrastructure.Plugins.Modules;
using Artemis.Core.Tests.Infrastructure.Plugins.Nodes;
using Artemis.Core.Tests.Infrastructure.Services;
using Artemis.Core.Tests.Infrastructure.VisualScripting;
using RGB.NET.Core;
using SkiaSharp;

namespace Artemis.Core.Tests.Services;

/// <summary>
///     Activating, deactivating, updating and rendering profiles through the profile service
/// </summary>
public class ProfileActivationTests : ProfileServiceTestBase
{
    [Fact]
    public void ActivateProfile_LoadsTheProfileOntoTheEnabledDevices()
    {
        AddDeviceProvider(TestLayouts.KeyboardAnsi);
        ArtemisDevice keyboard = Resolve<IDeviceService>().Devices.Single();
        TestLayerBrushProvider brushes = EnableFeature(new TestLayerBrushProvider());
        ProfileConfiguration configuration = ProfileService.CreateProfileConfiguration(General, "Test profile", "TestTube");
        Profile profile = ProfileService.ActivateProfile(configuration);
        AddLayer<SolidColorTestBrush>(profile, brushes, [keyboard.GetLed(LedId.Keyboard_A, false)!]);
        ProfileService.SaveProfile(profile, true);
        ProfileService.DeactivateProfile(configuration);
        List<ProfileConfiguration> activated = [];
        ProfileService.ProfileActivated += (_, e) => activated.Add(e.ProfileConfiguration);

        Profile reactivated = ProfileService.ActivateProfile(configuration);

        reactivated.ShouldNotBeSameAs(profile);
        configuration.Profile.ShouldBeSameAs(reactivated);
        activated.ShouldBe([configuration]);
        Layer layer = reactivated.GetAllLayers().Single();
        layer.Leds.Select(l => l.RgbLed.Id).ShouldBe([LedId.Keyboard_A]);
        layer.LayerBrush.ShouldBeOfType<SolidColorTestBrush>();
    }

    [Fact]
    public void ActivateProfile_WhenAlreadyActive_ReturnsTheActiveProfile()
    {
        ProfileConfiguration configuration = ProfileService.CreateProfileConfiguration(General, "Test profile", "TestTube");
        Profile profile = ProfileService.ActivateProfile(configuration);
        int activated = 0;
        ProfileService.ProfileActivated += (_, _) => activated++;

        ProfileService.ActivateProfile(configuration).ShouldBeSameAs(profile);

        activated.ShouldBe(0);
    }

    [Fact]
    public void DeactivateProfile_DisposesTheProfile()
    {
        ProfileConfiguration configuration = ProfileService.CreateProfileConfiguration(General, "Test profile", "TestTube");
        Profile profile = ProfileService.ActivateProfile(configuration);
        List<ProfileConfiguration> deactivated = [];
        ProfileService.ProfileDeactivated += (_, e) => deactivated.Add(e.ProfileConfiguration);

        ProfileService.DeactivateProfile(configuration);
        ProfileService.DeactivateProfile(configuration);

        configuration.Profile.ShouldBeNull();
        profile.Disposed.ShouldBeTrue();
        deactivated.ShouldBe([configuration]);
    }

    [Fact]
    public void DeactivateProfile_OfTheFocusProfile_Throws()
    {
        ProfileConfiguration configuration = ProfileService.CreateProfileConfiguration(General, "Test profile", "TestTube");
        ProfileService.ActivateProfile(configuration);
        ProfileService.FocusProfile = configuration;

        Should.Throw<ArtemisCoreException>(() => ProfileService.DeactivateProfile(configuration));
        configuration.Profile.ShouldNotBeNull();
    }

    [Fact]
    public void UpdateProfiles_ActivatesProfilesThatShouldBeActive()
    {
        ProfileConfiguration configuration = ProfileService.CreateProfileConfiguration(General, "Test profile", "TestTube");

        ProfileService.UpdateProfiles(0.1);

        // Without an activation condition the condition counts as met
        configuration.ActivationConditionMet.ShouldBeTrue();
        configuration.Profile.ShouldNotBeNull();
    }

    [Fact]
    public void UpdateProfiles_DeactivatesSuspendedProfiles()
    {
        ProfileConfiguration configuration = ProfileService.CreateProfileConfiguration(General, "Test profile", "TestTube");
        ProfileService.UpdateProfiles(0.1);

        configuration.IsSuspended = true;
        ProfileService.UpdateProfiles(0.1);

        configuration.Profile.ShouldBeNull();
    }

    [Fact]
    public void UpdateProfiles_DoesNotActivateProfilesInASuspendedCategory()
    {
        ProfileConfiguration configuration = ProfileService.CreateProfileConfiguration(General, "Test profile", "TestTube");
        General.IsSuspended = true;

        ProfileService.UpdateProfiles(0.1);

        configuration.Profile.ShouldBeNull();
    }

    [Fact]
    public void UpdateProfiles_FollowsTheActivationCondition()
    {
        TestNodeProvider nodes = EnableFeature(new TestNodeProvider());
        ProfileConfiguration configuration = ProfileService.CreateProfileConfiguration(General, "Test profile", "TestTube");
        TestBooleanNode condition = UseActivationCondition(configuration, nodes, false);

        ProfileService.UpdateProfiles(0.1);
        configuration.ActivationConditionMet.ShouldBeFalse();
        configuration.Profile.ShouldBeNull();

        condition.Value = true;
        ProfileService.UpdateProfiles(0.1);
        configuration.ActivationConditionMet.ShouldBeTrue();
        configuration.Profile.ShouldNotBeNull();

        condition.Value = false;
        ProfileService.UpdateProfiles(0.1);
        configuration.Profile.ShouldBeNull();
    }

    [Fact]
    public void UpdateProfiles_WithFadeInAndOut_KeepsTheProfileUntilItFadedOut()
    {
        ProfileConfiguration configuration = ProfileService.CreateProfileConfiguration(General, "Test profile", "TestTube");
        configuration.FadeInAndOut = true;
        ProfileService.UpdateProfiles(0.5);
        Profile profile = configuration.Profile!;
        profile.Opacity.ShouldBe(0.5);

        configuration.IsSuspended = true;
        ProfileService.UpdateProfiles(0.25);

        // Fading out at 1 opacity per second
        configuration.Profile.ShouldBeSameAs(profile);
        profile.ShouldDisplay.ShouldBeFalse();
        profile.Opacity.ShouldBe(0.25);

        ProfileService.UpdateProfiles(0.25);
        configuration.Profile.ShouldBeSameAs(profile);
        profile.Opacity.ShouldBe(0);

        ProfileService.UpdateProfiles(0.25);
        configuration.Profile.ShouldBeNull();
    }

    [Fact]
    public void UpdateProfiles_WhileFadingOut_CanFadeBackIn()
    {
        ProfileConfiguration configuration = ProfileService.CreateProfileConfiguration(General, "Test profile", "TestTube");
        configuration.FadeInAndOut = true;
        ProfileService.UpdateProfiles(1);
        Profile profile = configuration.Profile!;
        configuration.IsSuspended = true;
        ProfileService.UpdateProfiles(0.5);

        configuration.IsSuspended = false;
        ProfileService.UpdateProfiles(0.25);

        configuration.Profile.ShouldBeSameAs(profile);
        profile.ShouldDisplay.ShouldBeTrue();
        profile.Opacity.ShouldBe(0.75);
    }

    [Fact]
    public void UpdateProfiles_WithAModule_OnlyActivatesWhileTheModuleIsActivated()
    {
        IModuleService moduleService = ResolveModuleService();
        BooleanActivationRequirement requirement = new();
        TestModule module = EnableFeature(new TestModule {Requirements = [requirement]});
        ProfileConfiguration configuration = ProfileService.CreateProfileConfiguration(General, "Test profile", "TestTube");
        configuration.Module = module;

        moduleService.UpdateModuleActivation();
        ProfileService.UpdateProfiles(0.1);
        configuration.Profile.ShouldBeNull();

        requirement.ActivationMet = true;
        moduleService.UpdateModuleActivation();
        ProfileService.UpdateProfiles(0.1);
        configuration.Profile.ShouldNotBeNull();
    }

    [Fact]
    public void AProfileOfAModuleThatIsDisabled_IsMissingItsModule()
    {
        IModuleService moduleService = ResolveModuleService();
        TestModule module = EnableFeature(new TestModule());
        moduleService.UpdateModuleActivation();
        ProfileConfiguration configuration = ProfileService.CreateProfileConfiguration(General, "Test profile", "TestTube");
        configuration.Module = module;
        ProfileService.SaveProfileCategory(General);
        ProfileService.UpdateProfiles(0.1);
        configuration.Profile.ShouldNotBeNull();

        DisableFeature(module);
        ProfileService.UpdateProfiles(0.1);

        configuration.Module.ShouldBeNull();
        configuration.IsMissingModule.ShouldBeTrue();
        configuration.Profile.ShouldBeNull();
        // The module is remembered while it is missing
        ProfileService.SaveProfileCategory(General);
        configuration.Entity.ProfileConfiguration.ModuleId.ShouldBe(module.Id);

        PluginManagement.EnablePluginFeature(module, false, false);
        configuration.Module.ShouldBeSameAs(module);
        configuration.IsMissingModule.ShouldBeFalse();
    }

    [Fact]
    public void Restarting_WithoutTheModuleOfAProfile_LeavesItMissing()
    {
        TestModule module = EnableFeature(new TestModule());
        ProfileConfiguration configuration = ProfileService.CreateProfileConfiguration(General, "Test profile", "TestTube");
        configuration.Module = module;
        ProfileService.SaveProfileCategory(General);
        DisableFeature(module);

        ProfileConfiguration reloaded = CreateNewProfileService().ProfileCategories.Single(c => c.Name == "General").ProfileConfigurations.Single();

        reloaded.IsMissingModule.ShouldBeTrue();
        reloaded.ShouldBeActive(false).ShouldBeFalse();
    }

    [Fact]
    public void UpdateProfiles_WithAFocusProfile_OnlyUpdatesTheFocusProfile()
    {
        ProfileConfiguration focus = ProfileService.CreateProfileConfiguration(General, "Focus", "TestTube");
        ProfileConfiguration other = ProfileService.CreateProfileConfiguration(General, "Other", "TestTube");
        focus.FadeInAndOut = true;
        Profile focusProfile = ProfileService.ActivateProfile(focus);
        ProfileService.FocusProfile = focus;

        ProfileService.UpdateProfiles(0.5);
        other.Profile.ShouldBeNull();
        focusProfile.Opacity.ShouldBe(0);

        ProfileService.UpdateFocusProfile = true;
        ProfileService.UpdateProfiles(0.5);
        focusProfile.Opacity.ShouldBe(0.5);
        other.Profile.ShouldBeNull();
    }

    [Fact]
    public void RenderProfiles_DrawsActiveProfiles()
    {
        (ProfileConfiguration configuration, ArtemisLed led) = CreateProfileWithARedLayer();
        ProfileService.UpdateProfiles(0.1);

        ColorAt(led).ShouldBe(SKColors.Red);

        configuration.IsSuspended = true;
        ColorAt(led).Alpha.ShouldBe((byte) 0);
    }

    [Fact]
    public void RenderProfiles_WithAFocusProfile_OnlyDrawsTheFocusProfile()
    {
        (ProfileConfiguration configuration, ArtemisLed led) = CreateProfileWithARedLayer();
        ProfileConfiguration focus = ProfileService.CreateProfileConfiguration(General, "Focus", "TestTube");
        ProfileService.UpdateProfiles(0.1);
        ProfileService.ActivateProfile(focus);

        ProfileService.FocusProfile = focus;
        ColorAt(led).Alpha.ShouldBe((byte) 0);

        ProfileService.FocusProfile = configuration;
        ColorAt(led).ShouldBe(SKColors.Red);
    }

    [Fact]
    public void ProfileRenderingDisabled_IsOnlyAFlag()
    {
        (_, ArtemisLed led) = CreateProfileWithARedLayer();
        ProfileService.UpdateProfiles(0.1);

        ProfileService.ProfileRenderingDisabled = true;

        // The renderer checks the flag before calling RenderProfiles, the profile service itself ignores it
        ColorAt(led).ShouldBe(SKColors.Red);
    }

    [Fact]
    public void ChangingTheDevices_UpdatesTheLedsOfActiveProfiles()
    {
        TestDeviceProvider deviceProvider = AddDeviceProvider(TestLayouts.KeyboardAnsi);
        IDeviceService deviceService = Resolve<IDeviceService>();
        TestLayerBrushProvider brushes = EnableFeature(new TestLayerBrushProvider());
        ProfileConfiguration configuration = ProfileService.CreateProfileConfiguration(General, "Test profile", "TestTube");
        Profile profile = ProfileService.ActivateProfile(configuration);
        Layer layer = AddLayer<SolidColorTestBrush>(profile, brushes, deviceService.Devices.Single().Leds.Take(3));
        layer.Save();

        deviceService.RemoveDeviceProvider(deviceProvider);
        layer.Leds.ShouldBeEmpty();

        deviceService.AddDeviceProvider(deviceProvider);
        layer.Leds.Count.ShouldBe(3);
        layer.Leds.First().Device.ShouldBeSameAs(deviceService.Devices.Single());
    }

    [Fact]
    public void SaveProfile_StoresTheProfileAndReloadsOtherActiveInstances()
    {
        TestLayerBrushProvider brushes = EnableFeature(new TestLayerBrushProvider());
        ProfileConfiguration configuration = ProfileService.CreateProfileConfiguration(General, "Test profile", "TestTube");
        Profile active = ProfileService.ActivateProfile(configuration);
        // Like the editor, work on a separate instance of the same profile
        ProfileConfiguration clone = ProfileService.CloneProfileConfiguration(configuration);
        Profile edited = ProfileService.ActivateProfile(clone);
        AddLayer<SolidColorTestBrush>(edited, brushes, [], "Added in the editor");

        ProfileService.SaveProfile(edited, true);

        // The active instance is replaced by a fresh one, loaded from what was just saved
        active.Disposed.ShouldBeTrue();
        configuration.Profile.ShouldNotBeSameAs(active);
        configuration.Profile!.GetAllLayers().Single().Name.ShouldBe("Added in the editor");
        CreateNewProfileService().ProfileCategories.Single(c => c.Name == "General").ProfileConfigurations.Single()
            .Entity.Profile.Layers.Single().Name.ShouldBe("Added in the editor");
    }

    [Fact]
    public void Hotkeys_InToggleMode_SuspendAndResumeTheProfile()
    {
        TestInputProvider input = new();
        Resolve<IInputService>().AddInputProvider(input);
        ProfileConfiguration configuration = ProfileService.CreateProfileConfiguration(General, "Test profile", "TestTube");
        configuration.HotkeyMode = ProfileConfigurationHotkeyMode.Toggle;
        configuration.EnableHotkey = new Hotkey(KeyboardKey.F1, null);

        // Key presses are handled on the next update
        input.Press(KeyboardKey.F1);
        configuration.IsSuspended.ShouldBeFalse();
        ProfileService.UpdateProfiles(0.1);
        configuration.IsSuspended.ShouldBeTrue();
        configuration.Profile.ShouldBeNull();

        input.Press(KeyboardKey.F1);
        ProfileService.UpdateProfiles(0.1);
        configuration.IsSuspended.ShouldBeFalse();
        configuration.Profile.ShouldNotBeNull();
    }

    [Fact]
    public void Hotkeys_InEnableDisableMode_UseSeparateKeys()
    {
        TestInputProvider input = new();
        Resolve<IInputService>().AddInputProvider(input);
        ProfileConfiguration configuration = ProfileService.CreateProfileConfiguration(General, "Test profile", "TestTube");
        configuration.HotkeyMode = ProfileConfigurationHotkeyMode.EnableDisable;
        configuration.EnableHotkey = new Hotkey(KeyboardKey.F1, null);
        configuration.DisableHotkey = new Hotkey(KeyboardKey.F2, null);

        input.Press(KeyboardKey.F1);
        ProfileService.UpdateProfiles(0.1);
        configuration.IsSuspended.ShouldBeFalse();

        input.Press(KeyboardKey.F2);
        ProfileService.UpdateProfiles(0.1);
        configuration.IsSuspended.ShouldBeTrue();

        input.Press(KeyboardKey.F2);
        ProfileService.UpdateProfiles(0.1);
        configuration.IsSuspended.ShouldBeTrue();

        input.Press(KeyboardKey.F1);
        ProfileService.UpdateProfiles(0.1);
        configuration.IsSuspended.ShouldBeFalse();
    }

    [Fact]
    public void Hotkeys_AreIgnoredWithoutAHotkeyMode()
    {
        TestInputProvider input = new();
        Resolve<IInputService>().AddInputProvider(input);
        ProfileConfiguration configuration = ProfileService.CreateProfileConfiguration(General, "Test profile", "TestTube");
        configuration.EnableHotkey = new Hotkey(KeyboardKey.F1, null);

        input.Press(KeyboardKey.F1);
        ProfileService.UpdateProfiles(0.1);

        configuration.IsSuspended.ShouldBeFalse();
    }

    [Fact]
    public void Hotkeys_WithModifiers_OnlyMatchWithThoseModifiers()
    {
        TestInputProvider input = new();
        Resolve<IInputService>().AddInputProvider(input);
        ProfileConfiguration configuration = ProfileService.CreateProfileConfiguration(General, "Test profile", "TestTube");
        configuration.HotkeyMode = ProfileConfigurationHotkeyMode.Toggle;
        configuration.EnableHotkey = new Hotkey(KeyboardKey.F1, KeyboardModifierKey.Shift);

        input.Press(KeyboardKey.F1);
        ProfileService.UpdateProfiles(0.1);

        configuration.IsSuspended.ShouldBeFalse();
    }

    [Fact]
    public void Hotkeys_StoreTheSuspension()
    {
        TestInputProvider input = new();
        Resolve<IInputService>().AddInputProvider(input);
        ProfileConfiguration configuration = ProfileService.CreateProfileConfiguration(General, "Test profile", "TestTube");
        configuration.HotkeyMode = ProfileConfigurationHotkeyMode.Toggle;
        configuration.EnableHotkey = new Hotkey(KeyboardKey.F1, null);

        input.Press(KeyboardKey.F1);
        ProfileService.UpdateProfiles(0.1);

        CreateNewProfileService().ProfileCategories.Single(c => c.Name == "General").ProfileConfigurations.Single().IsSuspended.ShouldBeTrue();
    }

    private (ProfileConfiguration Configuration, ArtemisLed Led) CreateProfileWithARedLayer()
    {
        AddDeviceProvider(TestLayouts.KeyboardAnsi);
        ArtemisLed led = Resolve<IDeviceService>().Devices.Single().GetLed(LedId.Keyboard_A, false)!;
        TestLayerBrushProvider brushes = EnableFeature(new TestLayerBrushProvider());
        ProfileConfiguration configuration = ProfileService.CreateProfileConfiguration(General, "Red", "TestTube");
        Profile profile = ProfileService.ActivateProfile(configuration);
        // The solid test brush is red by default
        AddLayer<SolidColorTestBrush>(profile, brushes, [led]);
        profile.Reset();
        return (configuration, led);
    }

    /// <summary>
    ///     Renders the profiles onto a canvas covering the whole surface and returns the color in the middle of the LED
    /// </summary>
    private SKColor ColorAt(ArtemisLed led)
    {
        using SKBitmap bitmap = new(2000, 1000);
        using (SKCanvas canvas = new(bitmap))
        {
            canvas.Clear(SKColors.Transparent);
            ProfileService.RenderProfiles(canvas);
        }

        SKRect rectangle = led.AbsoluteRectangle;
        return bitmap.GetPixel((int) rectangle.MidX, (int) rectangle.MidY);
    }
}
