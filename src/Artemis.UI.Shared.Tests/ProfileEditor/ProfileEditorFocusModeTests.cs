using Artemis.UI.Shared.Services.ProfileEditor;
using Artemis.UI.Shared.Tests.Infrastructure;
using RGB.NET.Core;

namespace Artemis.UI.Shared.Tests.ProfileEditor;

/// <summary>
///     Focus modes decide which part of the profile the profile service keeps showing while editing
/// </summary>
public class ProfileEditorFocusModeTests : ProfileEditorTestBase
{
    private readonly Folder _folder;
    private readonly Layer _layer;

    public ProfileEditorFocusModeTests()
    {
        _folder = Harness.AddFolder(Root, "Folder");
        _layer = AddLayer("Layer", _folder, LedId.Keyboard_A);
    }

    [Fact]
    public async Task NoFocus_FocusesNothing()
    {
        await StartEditing();
        Service.ChangeCurrentProfileElement(_layer);

        ProfileService.FocusProfileElement.ShouldBeNull();
    }

    [Fact]
    public async Task FolderFocus_FocusesTheFolderOfTheSelectedElement()
    {
        await StartEditing();
        Service.ChangeCurrentProfileElement(_layer);

        Service.ChangeFocusMode(ProfileEditorFocusMode.Folder);

        Current(Service.FocusMode).ShouldBe(ProfileEditorFocusMode.Folder);
        ProfileService.FocusProfileElement.ShouldBe(_folder);
    }

    [Fact]
    public async Task SelectionFocus_FocusesTheSelectedElement()
    {
        await StartEditing();
        Service.ChangeCurrentProfileElement(_layer);

        Service.ChangeFocusMode(ProfileEditorFocusMode.Selection);

        ProfileService.FocusProfileElement.ShouldBe(_layer);
    }

    [Fact]
    public async Task ChangingTheSelection_MovesTheFocus()
    {
        await StartEditing();
        Service.ChangeFocusMode(ProfileEditorFocusMode.Selection);

        Service.ChangeCurrentProfileElement(_folder);
        ProfileService.FocusProfileElement.ShouldBe(_folder);

        Service.ChangeCurrentProfileElement(null);
        ProfileService.FocusProfileElement.ShouldBeNull();
    }

    [Fact]
    public async Task BackToNoFocus_ClearsTheFocus()
    {
        await StartEditing();
        Service.ChangeCurrentProfileElement(_layer);
        Service.ChangeFocusMode(ProfileEditorFocusMode.Selection);

        Service.ChangeFocusMode(ProfileEditorFocusMode.None);

        ProfileService.FocusProfileElement.ShouldBeNull();
    }

    [Fact]
    public async Task ChangeFocusMode_ToTheCurrentMode_DoesNothing()
    {
        await StartEditing();
        Service.ChangeCurrentProfileElement(_layer);
        Service.ChangeFocusMode(ProfileEditorFocusMode.Selection);
        int changes = ProfileService.FocusProfileElementChanges.Count;

        Service.ChangeFocusMode(ProfileEditorFocusMode.Selection);

        ProfileService.FocusProfileElementChanges.Count.ShouldBe(changes);
    }

    [Fact]
    public async Task SuspendedEditing_StillFocusesTheSelection()
    {
        await StartEditing();
        Service.ChangeCurrentProfileElement(_layer);
        Service.ChangeFocusMode(ProfileEditorFocusMode.Selection);
        ProfileService.FocusProfileElementChanges.Clear();

        Service.ChangeSuspendedEditing(true);

        // Pinned as-is: while editing is suspended ApplyFocusMode clears the focus, but then immediately sets it again
        // from the focus mode, so the clear has no effect. It looks like the intention was to not focus anything while
        // the profile plays normally.
        ProfileService.FocusProfileElementChanges.ShouldBe([null, _layer]);
        ProfileService.FocusProfileElement.ShouldBe(_layer);
    }
}
