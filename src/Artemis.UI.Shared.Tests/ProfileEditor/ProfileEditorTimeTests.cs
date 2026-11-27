using Artemis.UI.Shared.Services.ProfileEditor.Commands;
using Artemis.UI.Shared.Tests.Infrastructure;
using RGB.NET.Core;

namespace Artemis.UI.Shared.Tests.ProfileEditor;

/// <summary>
///     Moving through time in the editor, which overrides the timelines of the profile being edited, and suspending editing
/// </summary>
public class ProfileEditorTimeTests : ProfileEditorTestBase
{
    [Fact]
    public async Task ChangeTime_MovesTheTimelineOfEveryElement()
    {
        Folder folder = Harness.AddFolder(Root, "Folder");
        Layer layer = AddLayer("Layer", folder, LedId.Keyboard_A);
        await StartEditing();

        Service.ChangeTime(TimeSpan.FromSeconds(1.5));

        Current(Service.Time).ShouldBe(TimeSpan.FromSeconds(1.5));
        foreach (RenderProfileElement element in new RenderProfileElement[] {Root, folder, layer})
        {
            element.Timeline.Position.ShouldBe(TimeSpan.FromSeconds(1.5));
            element.Timeline.IsOverridden.ShouldBeTrue();
        }
    }

    [Fact]
    public async Task ChangeTime_EnablesElements()
    {
        Folder folder = Harness.AddFolder(Root, "Folder");
        Layer layer = AddLayer("Layer", folder, LedId.Keyboard_A);
        folder.Disable();
        layer.Disable();
        await StartEditing();

        Service.ChangeTime(TimeSpan.FromSeconds(1));

        folder.Enabled.ShouldBeTrue();
        layer.Enabled.ShouldBeTrue();
    }

    [Fact]
    public async Task ChangeTime_AppliesKeyframesAtTheNewTime()
    {
        Layer layer = AddLayer("Layer", null, LedId.Keyboard_A);
        FloatLayerProperty opacity = layer.Transform.Opacity;
        opacity.KeyframesEnabled = true;
        opacity.AddKeyframe(new LayerPropertyKeyframe<float>(0, TimeSpan.Zero, Easings.Functions.Linear, opacity));
        opacity.AddKeyframe(new LayerPropertyKeyframe<float>(100, TimeSpan.FromSeconds(2), Easings.Functions.Linear, opacity));
        await StartEditing();

        Service.ChangeTime(TimeSpan.FromSeconds(1));
        opacity.CurrentValue.ShouldBe(50, 0.01f);

        Service.ChangeTime(TimeSpan.FromSeconds(2));
        opacity.CurrentValue.ShouldBe(100, 0.01f);
    }

    [Fact]
    public async Task ChangeTime_DisablesSuspendedElementsAndSkipsTheirChildren()
    {
        Folder folder = Harness.AddFolder(Root, "Folder");
        Layer layer = AddLayer("Layer", folder, LedId.Keyboard_A);
        await StartEditing();
        Service.ChangeTime(TimeSpan.FromSeconds(1));

        folder.Suspended = true;
        Service.ChangeTime(TimeSpan.FromSeconds(2));

        // Disabling a folder disables its children, their timelines are left where they were
        folder.Enabled.ShouldBeFalse();
        layer.Enabled.ShouldBeFalse();
        layer.Timeline.Position.ShouldBe(TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ChangeTime_WithoutProfile_OnlyChangesTheTime()
    {
        Layer layer = AddLayer("Layer", null, LedId.Keyboard_A);

        Service.ChangeTime(TimeSpan.FromSeconds(1));

        Current(Service.Time).ShouldBe(TimeSpan.FromSeconds(1));
        layer.Timeline.IsOverridden.ShouldBeFalse();
    }

    [Fact]
    public async Task ChangeTime_WhileEditingIsSuspended_LeavesTheProfileAlone()
    {
        Layer layer = AddLayer("Layer", null, LedId.Keyboard_A);
        await StartEditing();
        Service.ChangeSuspendedEditing(true);

        Service.ChangeTime(TimeSpan.FromSeconds(1));

        Current(Service.Time).ShouldBe(TimeSpan.FromSeconds(1));
        layer.Timeline.Position.ShouldNotBe(TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task ExecuteCommand_AppliesTheCurrentTimeToTheChangedProfile()
    {
        await StartEditing();
        Service.ChangeTime(TimeSpan.FromSeconds(1));
        Layer layer = new(Root, "New layer");

        Service.ExecuteCommand(new AddProfileElement(layer, Root, 0));

        layer.Timeline.Position.ShouldBe(TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task SuspendingEditing_StopsPlayingAndHandsTheProfileBackToNormalPlayback()
    {
        Layer layer = AddLayer("Layer", null, LedId.Keyboard_A);
        await StartEditing();
        Service.ChangeTime(TimeSpan.FromSeconds(2));
        Service.Play();

        Service.ChangeSuspendedEditing(true);

        Current(Service.SuspendedEditing).ShouldBeTrue();
        Current(Service.Playing).ShouldBeFalse();
        ProfileService.UpdateFocusProfile.ShouldBeTrue();
        // The profile is reset so it plays from the start
        layer.Timeline.Position.ShouldBe(TimeSpan.Zero);
        // The editor keeps its own time
        Current(Service.Time).ShouldBe(TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task ResumingEditing_TakesBackControlAndAppliesTheTime()
    {
        Layer layer = AddLayer("Layer", null, LedId.Keyboard_A);
        await StartEditing();
        Service.ChangeTime(TimeSpan.FromSeconds(2));
        Service.ChangeSuspendedEditing(true);

        Service.ChangeSuspendedEditing(false);

        Current(Service.SuspendedEditing).ShouldBeFalse();
        ProfileService.UpdateFocusProfile.ShouldBeFalse();
        Current(Service.Time).ShouldBe(TimeSpan.FromSeconds(2));
        layer.Timeline.Position.ShouldBe(TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void ResumingEditing_WithoutProfile_LeavesTheProfileServiceAlone()
    {
        Service.ChangeSuspendedEditing(true);
        ProfileService.UpdateFocusProfile.ShouldBeTrue();

        Service.ChangeSuspendedEditing(false);

        ProfileService.UpdateFocusProfile.ShouldBeTrue();
    }

    [Fact]
    public async Task ChangeSuspendedEditing_ToTheCurrentValue_DoesNothing()
    {
        await StartEditing();
        List<bool> received = [];
        using IDisposable _ = Service.SuspendedEditing.Subscribe(received.Add);

        Service.ChangeSuspendedEditing(false);

        received.ShouldBe([false]);
        ProfileService.UpdateFocusProfile.ShouldBeFalse();
    }
}
