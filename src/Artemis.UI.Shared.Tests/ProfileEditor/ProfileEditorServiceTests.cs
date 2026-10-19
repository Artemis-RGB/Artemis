using Artemis.UI.Shared.Services.ProfileEditor;
using Artemis.UI.Shared.Services.ProfileEditor.Commands;
using Artemis.UI.Shared.Tests.Infrastructure;
using RGB.NET.Core;

namespace Artemis.UI.Shared.Tests.ProfileEditor;

/// <summary>
///     Opening and switching profiles, selection, playback and saving
/// </summary>
public class ProfileEditorServiceTests : ProfileEditorTestBase
{
    [Fact]
    public void NewService_HasNothingSelected()
    {
        Current(Service.ProfileConfiguration).ShouldBeNull();
        Current(Service.ProfileElement).ShouldBeNull();
        Current(Service.LayerProperty).ShouldBeNull();
        Current(Service.History).ShouldBeNull();
        Current(Service.Time).ShouldBe(TimeSpan.Zero);
        Current(Service.Playing).ShouldBeFalse();
        Current(Service.SuspendedEditing).ShouldBeFalse();
        Current(Service.PixelsPerSecond).ShouldBe(120);
        Current(Service.FocusMode).ShouldBe(ProfileEditorFocusMode.None);
        Service.SelectedKeyframes.ShouldBeEmpty();
    }

    [Fact]
    public async Task ChangeCurrentProfileConfiguration_ActivatesAndFocusesTheProfile()
    {
        await Service.ChangeCurrentProfileConfiguration(Configuration);

        Current(Service.ProfileConfiguration).ShouldBe(Configuration);
        ProfileService.FocusProfile.ShouldBe(Configuration);
        ProfileService.ActivatedProfiles.ShouldBe([Configuration]);
        // The configuration has no module, so the override is deliberately cleared
        ModuleService.ActivationOverrides.ShouldBe([null]);
        Current(Service.History)!.ProfileConfiguration.ShouldBe(Configuration);
    }

    [Fact]
    public async Task ChangeCurrentProfileConfiguration_OverridesTheActivationOfTheProfilesModule()
    {
        using Core.Tests.Infrastructure.Plugins.TestPlugin plugin = new();
        Core.Tests.Infrastructure.Plugins.Modules.TestModule module = plugin.AddFeature(new Core.Tests.Infrastructure.Plugins.Modules.TestModule());
        Configuration.Module = module;

        await Service.ChangeCurrentProfileConfiguration(Configuration);

        ModuleService.ActivationOverrides.ShouldBe([module]);
    }

    [Fact]
    public async Task ChangeCurrentProfileConfiguration_ToTheSameConfiguration_DoesNothing()
    {
        await Service.ChangeCurrentProfileConfiguration(Configuration);
        await Service.ChangeCurrentProfileConfiguration(Configuration);

        ProfileService.ActivatedProfiles.Count.ShouldBe(1);
        ProfileService.SavedProfiles.ShouldBeEmpty();
    }

    [Fact]
    public async Task ChangeCurrentProfileConfiguration_SavesThePreviousProfileAndStopsPlaying()
    {
        Profile other = Harness.CreateProfile("Other profile");
        await StartEditing();
        Service.Play();

        await Service.ChangeCurrentProfileConfiguration(other.Configuration);

        ProfileService.SavedProfiles.ShouldBe([(Profile, true)]);
        Current(Service.Playing).ShouldBeFalse();
        Current(Service.ProfileConfiguration).ShouldBe(other.Configuration);
    }

    [Fact]
    public async Task ChangeCurrentProfileConfiguration_ClearsTheSelectionAndResetsTime()
    {
        Layer layer = AddLayer("Layer", null, LedId.Keyboard_A);
        Profile other = Harness.CreateProfile("Other profile");
        await StartEditing();
        Service.ChangeCurrentProfileElement(layer);
        Service.ChangeCurrentLayerProperty(layer.Transform.Opacity);
        Service.ChangeTime(TimeSpan.FromSeconds(2));

        await Service.ChangeCurrentProfileConfiguration(other.Configuration);

        Current(Service.ProfileElement).ShouldBeNull();
        Current(Service.LayerProperty).ShouldBeNull();
        Current(Service.Time).ShouldBe(TimeSpan.Zero);
    }

    [Fact]
    public async Task ChangeCurrentProfileConfiguration_ResetsThePreviousProfile()
    {
        Layer layer = AddLayer("Layer", null, LedId.Keyboard_A);
        Profile other = Harness.CreateProfile("Other profile");
        await StartEditing();
        Service.ChangeTime(TimeSpan.FromSeconds(2));
        layer.Timeline.Position.ShouldBe(TimeSpan.FromSeconds(2));

        await Service.ChangeCurrentProfileConfiguration(other.Configuration);

        // Reset puts the timeline back at the start, so the profile plays from the beginning outside the editor
        layer.Timeline.Position.ShouldBe(TimeSpan.Zero);
    }

    [Fact]
    public async Task ChangeCurrentProfileConfiguration_EndsSuspendedEditing()
    {
        Profile other = Harness.CreateProfile("Other profile");
        await StartEditing();
        Service.ChangeSuspendedEditing(true);

        await Service.ChangeCurrentProfileConfiguration(other.Configuration);

        Current(Service.SuspendedEditing).ShouldBeFalse();
    }

    [Fact]
    public async Task ChangeCurrentProfileConfiguration_ToNothing_StopsEditing()
    {
        await StartEditing();

        await Service.ChangeCurrentProfileConfiguration(null);

        Current(Service.ProfileConfiguration).ShouldBeNull();
        Current(Service.History).ShouldBeNull();
        ProfileService.FocusProfile.ShouldBeNull();
        ProfileService.ActivatedProfiles.ShouldBe([Configuration]);
        ModuleService.ActivationOverrides.ShouldBe([null, null]);
        ProfileService.SavedProfiles.ShouldBe([(Profile, true)]);
    }

    [Fact]
    public async Task EachProfileConfiguration_KeepsItsOwnHistory()
    {
        Layer layer = AddLayer("Layer", null, LedId.Keyboard_A);
        Profile other = Harness.CreateProfile("Other profile");
        await StartEditing();
        ProfileEditorHistory firstHistory = CurrentHistory;
        Service.ExecuteCommand(new RenameProfileElement(layer, "Renamed"));

        await Service.ChangeCurrentProfileConfiguration(other.Configuration);
        ProfileEditorHistory otherHistory = CurrentHistory;
        otherHistory.ShouldNotBe(firstHistory);
        Undo().ShouldBeNull();
        layer.Name.ShouldBe("Renamed");

        await StartEditing();
        CurrentHistory.ShouldBe(firstHistory);
        Undo().ShouldBeOfType<RenameProfileElement>();
        layer.Name.ShouldBe("Layer");
    }

    [Fact]
    public async Task History_GivesNewSubscribersTheCurrentHistoryTwice()
    {
        await StartEditing();
        List<ProfileEditorHistory?> received = [];

        using IDisposable _ = Service.History.Subscribe(received.Add);

        // Pinned as-is: History first returns the current history and then subscribes to ProfileConfiguration, which
        // repeats the current configuration. Harmless for the UI, but every subscriber gets the same history twice
        received.Count.ShouldBe(2);
        received.ShouldAllBe(h => h == received[0]);
        received[0]!.ProfileConfiguration.ShouldBe(Configuration);
    }

    [Fact]
    public async Task ChangeCurrentProfileElement_SelectsTheElementAndClearsTheRestOfTheSelection()
    {
        Layer layer = AddLayer("Layer", null, LedId.Keyboard_A);
        Layer otherLayer = AddLayer("Other layer", null, LedId.Keyboard_B);
        await StartEditing();
        layer.Transform.Opacity.KeyframesEnabled = true;
        LayerPropertyKeyframe<float> keyframe = new(50, TimeSpan.Zero, Easings.Functions.Linear, layer.Transform.Opacity);
        layer.Transform.Opacity.AddKeyframe(keyframe);
        Service.ChangeCurrentProfileElement(layer);
        Service.ChangeCurrentLayerProperty(layer.Transform.Opacity);
        Service.SelectKeyframe(keyframe, false, false);

        Service.ChangeCurrentProfileElement(otherLayer);

        Current(Service.ProfileElement).ShouldBe(otherLayer);
        Current(Service.LayerProperty).ShouldBeNull();
        Service.SelectedKeyframes.ShouldBeEmpty();
    }

    [Fact]
    public void ChangeCurrentLayerProperty_SelectsTheProperty()
    {
        Layer layer = AddLayer("Layer", null, LedId.Keyboard_A);

        Service.ChangeCurrentLayerProperty(layer.Transform.Opacity);

        Current(Service.LayerProperty).ShouldBe(layer.Transform.Opacity);
    }

    [Fact]
    public void ChangePixelsPerSecond_ChangesTheZoomLevel()
    {
        Service.ChangePixelsPerSecond(300);

        Current(Service.PixelsPerSecond).ShouldBe(300);
    }

    [Fact]
    public void PlayAndPause_OnlyReportChanges()
    {
        List<bool> received = [];
        using IDisposable _ = Service.Playing.Subscribe(received.Add);

        Service.Pause();
        Service.Play();
        Service.Play();
        Service.Pause();
        Service.Pause();

        received.ShouldBe([false, true, false]);
    }

    [Fact]
    public async Task SaveProfileAsync_SavesTheProfileIncludingItsChildren()
    {
        await StartEditing();

        await Service.SaveProfileAsync();

        ProfileService.SavedProfiles.ShouldBe([(Profile, true)]);
    }

    [Fact]
    public async Task SaveProfileAsync_WithoutProfile_SavesNothing()
    {
        await Service.SaveProfileAsync();

        ProfileService.SavedProfiles.ShouldBeEmpty();
    }

    [Fact]
    public async Task Commands_SaveTheProfileOnceASecondAfterTheLastChange()
    {
        Layer layer = AddLayer("Layer", null, LedId.Keyboard_A);
        await StartEditing();

        Service.ExecuteCommand(new RenameProfileElement(layer, "First"));
        Service.ExecuteCommand(new RenameProfileElement(layer, "Second"));
        Undo();
        ProfileService.SavedProfiles.ShouldBeEmpty();

        await WaitUntil(() => ProfileService.SavedProfiles.Count > 0);
        await Task.Delay(1200, TestContext.Current.CancellationToken);

        lock (ProfileService.SavedProfiles)
            ProfileService.SavedProfiles.ShouldBe([(Profile, true)]);
    }

    [Fact]
    public async Task AutoSave_AfterAFailedSave_StopsSaving()
    {
        Layer layer = AddLayer("Layer", null, LedId.Keyboard_A);
        await StartEditing();
        ProfileService.SaveException = new InvalidOperationException("Disk full");

        Service.ExecuteCommand(new RenameProfileElement(layer, "First"));
        await WaitUntil(() => WindowService.ExceptionDialogs.Count > 0);
        WindowService.ExceptionDialogs.ShouldHaveSingleItem().Title.ShouldBe("Failed to auto-save profile");

        ProfileService.SaveException = null;
        Service.ExecuteCommand(new RenameProfileElement(layer, "Second"));
        await Task.Delay(2000, TestContext.Current.CancellationToken);

        // Pinned as-is: the auto-save rethrows the exception after showing the dialog, which ends the observable that
        // drives auto-saving. Nothing is auto-saved anymore for as long as the app runs, without telling the user
        lock (ProfileService.SavedProfiles)
            ProfileService.SavedProfiles.Count.ShouldBe(1);
    }

    [Fact]
    public async Task ClosingTheMainWindow_StopsEditing()
    {
        await StartEditing();

        MainWindowService.RaiseMainWindowClosed();

        await WaitUntil(() => Current(Service.ProfileConfiguration) == null);
        ProfileService.FocusProfile.ShouldBeNull();
    }
}
