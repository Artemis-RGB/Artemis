using System;
using System.Windows.Input;

namespace Artemis.UI.Shared;

/// <summary>
///     Represents a placeholder command that does nothing and can't be executed.
/// </summary>
public class NullCommand : ICommand
{
    private static readonly Lazy<NullCommand> _instance = new(() => new NullCommand());

    private NullCommand()
    {
    }

    /// <summary>
    ///     Gets the static instance of this command.
    /// </summary>
    public static ICommand Instance => _instance.Value;

    /// <inheritdoc />
    public event EventHandler? CanExecuteChanged;

    /// <inheritdoc />
    public void Execute(object? parameter)
    {
        throw new InvalidOperationException("NullCommand cannot be executed");
    }

    /// <inheritdoc />
    public bool CanExecute(object? parameter)
    {
        return false;
    }
}