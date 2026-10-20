using Artemis.UI.Shared.Services.ProfileEditor.Commands;
using Artemis.UI.Shared.Tests.Infrastructure;
using RGB.NET.Core;

namespace Artemis.UI.Shared.Tests.ProfileEditor.Commands;

/// <summary>
///     Adding, removing, moving and renaming folders and layers
/// </summary>
public class ProfileElementCommandTests : ProfileEditorTestBase
{
    private readonly Folder _folder;
    private readonly Layer _first;
    private readonly Layer _second;
    private readonly Layer _third;

    public ProfileElementCommandTests()
    {
        _folder = Harness.AddFolder(Root, "Folder");
        _first = AddLayer("First", _folder, LedId.Keyboard_Q);
        _second = AddLayer("Second", _folder, LedId.Keyboard_W);
        _third = AddLayer("Third", Root, LedId.Keyboard_E);
    }

    [Fact]
    public void AddProfileElement_AddsAndEnablesTheElementAtTheIndex()
    {
        Layer layer = new(_folder, "New layer");
        AddProfileElement command = new(layer, _folder, 1);

        command.Execute();

        command.DisplayName.ShouldBe("Add layer");
        _folder.Children.ShouldBe([_first, layer, _second]);
        layer.Parent.ShouldBe(_folder);
        layer.Enabled.ShouldBeTrue();
    }

    [Fact]
    public void AddProfileElement_Undo_RestoresTheProfile()
    {
        string before = Snapshot();
        Folder folder = new(Root, "New folder");
        AddProfileElement command = new(folder, Root, 0);

        command.Execute();
        command.Undo();

        command.DisplayName.ShouldBe("Add folder");
        Root.Children.ShouldBe([_folder, _third]);
        folder.Parent.ShouldBeNull();
        Snapshot().ShouldBe(before);
    }

    [Fact]
    public void AddProfileElement_Undo_LeavesTheElementEnabled()
    {
        Layer layer = new(_folder, "New layer");
        AddProfileElement command = new(layer, _folder, 0);

        command.Execute();
        command.Undo();

        // Pinned as-is: Execute enables the element but Undo does not disable it again. It is no longer in the profile
        // so it doesn't render, but its brush and effects stay enabled (unlike RemoveProfileElement, which disables)
        layer.Enabled.ShouldBeTrue();
    }

    [Fact]
    public void AddProfileElement_Dispose_DisposesTheElementOnlyWhenNotAdded()
    {
        Layer added = new(_folder, "Added");
        AddProfileElement addedCommand = new(added, _folder, 0);
        addedCommand.Execute();
        Layer undone = new(_folder, "Undone");
        AddProfileElement undoneCommand = new(undone, _folder, 0);
        undoneCommand.Execute();
        undoneCommand.Undo();

        addedCommand.Dispose();
        undoneCommand.Dispose();

        added.Disposed.ShouldBeFalse();
        undone.Disposed.ShouldBeTrue();
    }

    [Fact]
    public void RemoveProfileElement_RemovesAndDisablesTheElement()
    {
        _first.Enable();
        RemoveProfileElement command = new(_first);

        command.Execute();

        command.DisplayName.ShouldBe("Remove layer");
        _folder.Children.ShouldBe([_second]);
        _first.Parent.ShouldBeNull();
        _first.Enabled.ShouldBeFalse();
    }

    [Fact]
    public void RemoveProfileElement_Undo_PutsTheElementBackInPlace()
    {
        _first.Enable();
        string before = Snapshot();
        RemoveProfileElement command = new(_first);

        command.Execute();
        command.Undo();

        _folder.Children.ShouldBe([_first, _second]);
        _first.Parent.ShouldBe(_folder);
        _first.Enabled.ShouldBeTrue();
        Snapshot().ShouldBe(before);
    }

    [Fact]
    public void RemoveProfileElement_Undo_LeavesADisabledElementDisabled()
    {
        _first.Disable();
        RemoveProfileElement command = new(_first);

        command.Execute();
        command.Undo();

        _first.Enabled.ShouldBeFalse();
    }

    [Fact]
    public void RemoveProfileElement_OfAFolder_TakesItsChildrenAlong()
    {
        string before = Snapshot();
        RemoveProfileElement command = new(_folder);

        command.Execute();
        Root.Children.ShouldBe([_third]);
        _folder.Children.ShouldBe([_first, _second]);

        command.Undo();
        command.DisplayName.ShouldBe("Remove folder");
        Snapshot().ShouldBe(before);
    }

    [Fact]
    public void RemoveProfileElement_WithoutParent_Throws()
    {
        _folder.RemoveChild(_first);

        Should.Throw<ArtemisSharedUIException>(() => new RemoveProfileElement(_first));
    }

    [Fact]
    public void RemoveProfileElement_Dispose_DisposesTheElementOnlyWhenRemoved()
    {
        RemoveProfileElement removed = new(_first);
        removed.Execute();
        RemoveProfileElement undone = new(_second);
        undone.Execute();
        undone.Undo();

        removed.Dispose();
        undone.Dispose();

        _first.Disposed.ShouldBeTrue();
        _second.Disposed.ShouldBeFalse();
    }

    [Fact]
    public void MoveProfileElement_ToAnotherFolder_AndBack()
    {
        string before = Snapshot();
        MoveProfileElement command = new(Root, _first, 1);

        command.Execute();
        Root.Children.ShouldBe([_folder, _first, _third]);
        _folder.Children.ShouldBe([_second]);
        _first.Parent.ShouldBe(Root);

        command.Undo();
        _folder.Children.ShouldBe([_first, _second]);
        _first.Parent.ShouldBe(_folder);
        Snapshot().ShouldBe(before);
    }

    [Fact]
    public void MoveProfileElement_WithinTheSameFolder_AndBack()
    {
        Layer fourth = AddLayer("Fourth", _folder, LedId.Keyboard_R);
        string before = Snapshot();
        MoveProfileElement command = new(_folder, _first, 2);

        command.Execute();
        // The element is taken out first, so the index is counted without it
        _folder.Children.ShouldBe([_second, fourth, _first]);
        _folder.Children.Select(c => c.Order).ShouldBe([1, 2, 3]);

        command.Undo();
        _folder.Children.ShouldBe([_first, _second, fourth]);
        Snapshot().ShouldBe(before);
    }

    [Fact]
    public void MoveProfileElement_IntoAnotherFolder_CanBeRedone()
    {
        MoveProfileElement command = new(Root, _first, 0);

        command.Execute();
        command.Undo();
        command.Execute();

        Root.Children.ShouldBe([_first, _folder, _third]);
    }

    [Fact]
    public void MoveProfileElement_DisplayName_AlwaysSaysLayer()
    {
        Folder other = Harness.AddFolder(Root, "Other folder");

        // Pinned as-is: the constructor sets "Move folder" for folders and then always overwrites it with "Move layer"
        new MoveProfileElement(_folder, other, 0).DisplayName.ShouldBe("Move layer");
        new MoveProfileElement(Root, _first, 0).DisplayName.ShouldBe("Move layer");
    }

    [Fact]
    public void RenameProfileElement_RenamesAndUndoRestoresTheName()
    {
        string before = Snapshot();
        RenameProfileElement layerCommand = new(_first, "Renamed layer");
        RenameProfileElement folderCommand = new(_folder, "Renamed folder");

        layerCommand.Execute();
        folderCommand.Execute();
        _first.Name.ShouldBe("Renamed layer");
        _folder.Name.ShouldBe("Renamed folder");

        folderCommand.Undo();
        layerCommand.Undo();
        layerCommand.DisplayName.ShouldBe("Rename layer");
        folderCommand.DisplayName.ShouldBe("Rename folder");
        Snapshot().ShouldBe(before);
    }

    [Fact]
    public void RenameProfileElement_OfTheProfile_IsNotSupported()
    {
        Should.Throw<ArgumentException>(() => new RenameProfileElement(Profile, "Renamed"));
    }
}
