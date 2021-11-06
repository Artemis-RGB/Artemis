using System;
using System.Windows.Input;

namespace Artemis.UI.Avalonia.Shared.Utilities
{
    /// <summary>
    ///     Provides a command that simply calls a delegate when invoked
    /// </summary>
    public class DelegateCommand : ICommand
    {
        private readonly Predicate<object?>? _canExecute;
        private readonly Action<object?> _execute;

        /// <summary>
        ///     Creates a new instance of the <see cref="DelegateCommand" /> class
        /// </summary>
        /// <param name="execute">The delegate to execute</param>
        public DelegateCommand(Action<object?> execute) : this(execute, null)
        {
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="DelegateCommand" /> class with a predicate indicating whether the command
        ///     can be executed
        /// </summary>
        /// <param name="execute">The delegate to execute</param>
        /// <param name="canExecute">The predicate that determines whether the command can execute</param>
        public DelegateCommand(Action<object?> execute, Predicate<object?>? canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        /// <summary>
        ///     Invokes the <see cref="CanExecuteChanged" /> event
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc />
        public event EventHandler? CanExecuteChanged;

        /// <inheritdoc />
        public bool CanExecute(object? parameter)
        {
            if (_canExecute == null)
                return true;

            return _canExecute(parameter);
        }

        /// <inheritdoc />
        public void Execute(object? parameter)
        {
            _execute(parameter);
        }
    }
}