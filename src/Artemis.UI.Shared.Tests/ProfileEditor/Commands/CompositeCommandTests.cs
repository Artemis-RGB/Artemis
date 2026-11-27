using Artemis.UI.Shared.Services.ProfileEditor;
using Artemis.UI.Shared.Services.ProfileEditor.Commands;
using Artemis.UI.Shared.Tests.Infrastructure;
using RGB.NET.Core;

namespace Artemis.UI.Shared.Tests.ProfileEditor.Commands;

/// <summary>
///     Running several commands as one
/// </summary>
public class CompositeCommandTests : ProfileEditorTestBase
{
    private readonly List<string> _log = [];

    [Fact]
    public void Execute_RunsTheCommandsInOrderAndUndoInReverse()
    {
        CompositeCommand command = new([new RecordingCommand("A", _log), new RecordingCommand("B", _log)], "Both");

        command.Execute();
        command.Undo();

        command.DisplayName.ShouldBe("Both");
        _log.ShouldBe(["execute A", "execute B", "undo B", "undo A"]);
    }

    [Fact]
    public void Execute_CanSkipTheFirstExecute()
    {
        // Used by command scopes, whose commands already ran when they were added
        CompositeCommand command = new([new RecordingCommand("A", _log)], "Scope", true);

        command.Execute();
        command.Undo();
        command.Execute();

        _log.ShouldBe(["undo A", "execute A"]);
    }

    [Fact]
    public void Dispose_DisposesEveryCommand()
    {
        RecordingCommand a = new("A", _log);
        RecordingCommand b = new("B", _log);

        new CompositeCommand([a, b], "Both").Dispose();

        a.Disposed.ShouldBeTrue();
        b.Disposed.ShouldBeTrue();
    }

    [Fact]
    public void Constructor_NeedsCommandsAndAName()
    {
        Should.Throw<ArgumentNullException>(() => new CompositeCommand(null!, "Name"));
        Should.Throw<ArgumentNullException>(() => new CompositeCommand([], null!));
        Should.Throw<ArgumentNullException>(() => new CompositeCommand(null!, "Name", true));
        Should.Throw<ArgumentNullException>(() => new CompositeCommand([], null!, true));
    }

    [Fact]
    public void Undo_RestoresTheProfileAfterSeveralRealCommands()
    {
        Folder folder = Harness.AddFolder(Root, "Folder");
        Layer layer = AddLayer("Layer", folder, LedId.Keyboard_A);
        string before = Snapshot();
        Layer newLayer = new(Root, "New layer");
        CompositeCommand command = new(
        [
            new AddProfileElement(newLayer, Root, 0),
            new MoveProfileElement(Root, layer, 0),
            new RenameProfileElement(folder, "Renamed"),
            new ChangeLayerLeds(layer, [Led(LedId.Keyboard_Z)]),
            new UpdateLayerProperty<float>(layer.Transform.Opacity, 30, null)
        ], "Lots of changes");

        command.Execute();
        Root.Children.ShouldBe([layer, newLayer, folder]);

        command.Undo();
        Root.Children.ShouldBe([folder]);
        Snapshot().ShouldBe(before);
    }
}
