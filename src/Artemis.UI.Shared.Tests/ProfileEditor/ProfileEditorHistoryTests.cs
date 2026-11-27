using Artemis.UI.Shared.Services.ProfileEditor;
using Artemis.UI.Shared.Services.ProfileEditor.Commands;
using System.Reactive;
using Artemis.UI.Shared.Tests.Infrastructure;
using ReactiveUI;

namespace Artemis.UI.Shared.Tests.ProfileEditor;

/// <summary>
///     Executing commands through the editor, undo and redo, and command scopes
/// </summary>
public class ProfileEditorHistoryTests : ProfileEditorTestBase
{
    private readonly List<string> _log = [];

    [Fact]
    public void ExecuteCommand_WithoutProfile_ShowsAnErrorAndThrows()
    {
        RecordingCommand command = new("A", _log);

        Should.Throw<ArtemisSharedUIException>(() => Service.ExecuteCommand(command));

        _log.ShouldBeEmpty();
        WindowService.ExceptionDialogs.ShouldHaveSingleItem().Title.ShouldBe("Editor command failed");
    }

    [Fact]
    public async Task ExecuteCommand_ExecutesTheCommandAndAllowsUndo()
    {
        await StartEditing();
        List<bool> canUndo = [];
        using IDisposable _ = CurrentHistory.CanUndo.Subscribe(canUndo.Add);

        Service.ExecuteCommand(new RecordingCommand("A", _log));

        _log.ShouldBe(["execute A"]);
        canUndo.ShouldBe([false, true]);
    }

    [Fact]
    public async Task CanUndoAndCanRedo_StartOutFalse()
    {
        await StartEditing();
        List<bool> canUndo = [];
        List<bool> canRedo = [];

        using IDisposable undoSubscription = CurrentHistory.CanUndo.Subscribe(canUndo.Add);
        using IDisposable redoSubscription = CurrentHistory.CanRedo.Subscribe(canRedo.Add);

        canUndo.ShouldBe([false]);
        canRedo.ShouldBe([false]);
    }

    [Fact]
    public async Task UndoAndRedo_WalkThroughTheHistoryInOrder()
    {
        await StartEditing();
        RecordingCommand a = new("A", _log);
        RecordingCommand b = new("B", _log);
        Service.ExecuteCommand(a);
        Service.ExecuteCommand(b);

        Undo().ShouldBe(b);
        Undo().ShouldBe(a);
        Redo().ShouldBe(a);
        Redo().ShouldBe(b);

        _log.ShouldBe(["execute A", "execute B", "undo B", "undo A", "execute A", "execute B"]);
    }

    [Fact]
    public async Task UndoAndRedo_WithNothingToUndoOrRedo_DoNothing()
    {
        await StartEditing();

        Undo().ShouldBeNull();
        Redo().ShouldBeNull();
    }

    [Fact]
    public async Task UndoAndRedo_ReportWhetherMoreCanBeUndoneOrRedone()
    {
        await StartEditing();
        List<bool> canUndo = [];
        List<bool> canRedo = [];
        using IDisposable undoSubscription = CurrentHistory.CanUndo.Subscribe(canUndo.Add);
        using IDisposable redoSubscription = CurrentHistory.CanRedo.Subscribe(canRedo.Add);

        Service.ExecuteCommand(new RecordingCommand("A", _log));
        Undo();
        Redo();

        canUndo.ShouldBe([false, true, false, true]);
        canRedo.ShouldBe([false, true, false]);
    }

    [Fact]
    public async Task ExecuteCommand_AfterUndo_ThrowsAwayAndDisposesWhatCouldBeRedone()
    {
        await StartEditing();
        RecordingCommand a = new("A", _log);
        RecordingCommand b = new("B", _log);
        Service.ExecuteCommand(a);
        Undo();

        Service.ExecuteCommand(b);

        a.Disposed.ShouldBeTrue();
        b.Disposed.ShouldBeFalse();
        Redo().ShouldBeNull();
    }

    [Fact]
    public async Task Clear_DisposesEverythingInTheHistory()
    {
        await StartEditing();
        RecordingCommand a = new("A", _log);
        RecordingCommand b = new("B", _log);
        Service.ExecuteCommand(a);
        Service.ExecuteCommand(b);
        Undo();

        CurrentHistory.Clear();

        a.Disposed.ShouldBeTrue();
        b.Disposed.ShouldBeTrue();
        Undo().ShouldBeNull();
        Redo().ShouldBeNull();
    }

    [Fact]
    public async Task ExecuteCommand_ThatFails_DoesNotEndUpInTheHistory()
    {
        RecordingCommand failing = new("A", _log) {ExecuteException = new InvalidOperationException("Oops")};

        // ReactiveCommand also reports the exception to ReactiveUI's default exception handler, which throws it again on
        // the main thread and would end the test run (and the app). Catch it here instead, the history picks up the
        // handler when it is created, which happens when editing starts
        List<Exception> unhandled = [];
        IObserver<Exception> defaultHandler = RxApp.DefaultExceptionHandler;
        RxApp.DefaultExceptionHandler = Observer.Create<Exception>(unhandled.Add);
        try
        {
            await StartEditing();
            Should.Throw<InvalidOperationException>(() => Service.ExecuteCommand(failing)).Message.ShouldBe("Oops");
        }
        finally
        {
            RxApp.DefaultExceptionHandler = defaultHandler;
        }

        WindowService.ExceptionDialogs.ShouldHaveSingleItem().Title.ShouldBe("Editor command failed");
        // The dialog is enough, ReactiveUI doesn't also treat the failure as unhandled (a crash in the app)
        unhandled.ShouldBeEmpty();
        Undo().ShouldBeNull();
    }

    [Fact]
    public async Task CommandScope_ExecutesCommandsRightAwayAndAddsThemToTheHistoryAsOne()
    {
        await StartEditing();
        RecordingCommand a = new("A", _log);
        RecordingCommand b = new("B", _log);

        using (ProfileEditorCommandScope scope = Service.CreateCommandScope("Scope"))
        {
            Service.ExecuteCommand(a);
            Service.ExecuteCommand(b);

            _log.ShouldBe(["execute A", "execute B"]);
            scope.ProfileEditorCommands.ShouldBe([a, b]);
            Undo().ShouldBeNull();
        }

        // Adding the scope to the history does not execute its commands again
        _log.ShouldBe(["execute A", "execute B"]);

        IProfileEditorCommand? undone = Undo();
        undone.ShouldBeOfType<CompositeCommand>().DisplayName.ShouldBe("Scope");
        Redo();
        _log.ShouldBe(["execute A", "execute B", "undo B", "undo A", "execute A", "execute B"]);
    }

    [Fact]
    public async Task CommandScope_CantBeNested()
    {
        await StartEditing();
        using ProfileEditorCommandScope _ = Service.CreateCommandScope("Outer");

        Should.Throw<ArtemisSharedUIException>(() => Service.CreateCommandScope("Inner")).Message.ShouldContain("Outer");
    }

    [Fact]
    public async Task CommandScope_NeedsAName()
    {
        await StartEditing();

        Should.Throw<ArgumentNullException>(() => Service.CreateCommandScope(null!));
    }

    [Fact]
    public async Task CommandScope_DisposedTwice_AddsItselfToTheHistoryOnce()
    {
        await StartEditing();
        ProfileEditorCommandScope scope = Service.CreateCommandScope("Scope");
        Service.ExecuteCommand(new RecordingCommand("A", _log));

        scope.Dispose();
        scope.Dispose();

        Undo().ShouldBeOfType<CompositeCommand>();
        Undo().ShouldBeNull();
    }

    [Fact]
    public async Task CommandScope_WithoutCommands_DoesNotAddAnEntryToTheHistory()
    {
        await StartEditing();

        Service.CreateCommandScope("Empty").Dispose();

        Undo().ShouldBeNull();
    }

    [Fact]
    public async Task CommandScope_LeftOpenWhenSwitchingProfiles_EndsUpInTheHistoryOfThePreviousProfile()
    {
        Profile other = Harness.CreateProfile("Other profile");
        await StartEditing();
        ProfileEditorHistory history = CurrentHistory;
        Service.CreateCommandScope("Scope");
        Service.ExecuteCommand(new RecordingCommand("A", _log));

        await Service.ChangeCurrentProfileConfiguration(other.Configuration);

        Undo().ShouldBeNull();
        await StartEditing();
        CurrentHistory.ShouldBe(history);
        Undo().ShouldBeOfType<CompositeCommand>().DisplayName.ShouldBe("Scope");
        // A new scope can be created again
        Service.CreateCommandScope("Next").Dispose();
    }

    [Fact]
    public void ProfileEditorHistory_CanBeUsedOnItsOwn()
    {
        ProfileEditorHistory history = new(Configuration);
        RecordingCommand a = new("A", _log);

        history.ExecuteEditorCommand(a);
        history.Undo.Execute().Subscribe();
        history.Redo.Execute().Subscribe();

        history.ProfileConfiguration.ShouldBe(Configuration);
        _log.ShouldBe(["execute A", "undo A", "execute A"]);
    }
}
