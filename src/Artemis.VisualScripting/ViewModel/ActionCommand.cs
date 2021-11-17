using System;
using System.Windows.Input;

namespace Artemis.VisualScripting.ViewModel
{
    public class ActionCommand : ICommand
    {
        #region Properties & Fields

        private readonly Func<bool> _canExecute;
        private readonly Action _command;

        #endregion

        #region Events

        public event EventHandler CanExecuteChanged;

        #endregion

        #region Constructors

        public ActionCommand(Action command, Func<bool> canExecute = null)
        {
            this._command = command;
            this._canExecute = canExecute;
        }

        #endregion

        #region Methods

        public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;

        public void Execute(object parameter) => _command?.Invoke();

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, new EventArgs());

        #endregion
    }

    public class ActionCommand<T> : ICommand
    {
        #region Properties & Fields

        private readonly Func<T, bool> _canExecute;
        private readonly Action<T> _command;

        #endregion

        #region Events

        public event EventHandler CanExecuteChanged;

        #endregion

        #region Constructors

        public ActionCommand(Action<T> command, Func<T, bool> canExecute = null)
        {
            this._command = command;
            this._canExecute = canExecute;
        }

        #endregion

        #region Methods

        public bool CanExecute(object parameter) => _canExecute?.Invoke((T)parameter) ?? true;

        public void Execute(object parameter) => _command?.Invoke((T)parameter);

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, new EventArgs());

        #endregion
    }
}
