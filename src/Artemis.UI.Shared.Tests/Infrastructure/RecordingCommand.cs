using Artemis.UI.Shared.Services.ProfileEditor;

namespace Artemis.UI.Shared.Tests.Infrastructure;

/// <summary>
///     A command that only writes down what happened to it, in a log that can be shared between commands to check the
///     order they ran in.
/// </summary>
public class RecordingCommand(string name, List<string>? log = null) : IProfileEditorCommand, IDisposable
{
    public List<string> Log { get; } = log ?? [];
    public bool Disposed { get; private set; }

    /// <summary>
    ///     Gets or sets an exception <see cref="Execute" /> should throw
    /// </summary>
    public Exception? ExecuteException { get; set; }

    public string DisplayName => name;

    public void Execute()
    {
        if (ExecuteException != null)
            throw ExecuteException;
        Log.Add($"execute {name}");
    }

    public void Undo()
    {
        Log.Add($"undo {name}");
    }

    public void Dispose()
    {
        Disposed = true;
        Log.Add($"dispose {name}");
    }
}
