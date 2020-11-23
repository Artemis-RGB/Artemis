using MaterialDesignThemes.Wpf;
using Stylet;

namespace Artemis.UI.Shared.Services
{
    /// <summary>
    ///     Represents the base class for a dialog view model
    /// </summary>
    public abstract class DialogViewModelBase : ValidatingModelBase
    {
        private DialogViewModelHost? _dialogViewModelHost;
        private DialogSession? _session;

        /// <summary>
        ///     Creates a new instance of the <see cref="DialogViewModelBase" /> class with a model validator
        /// </summary>
        /// <param name="validator">A validator to apply to the model</param>
        protected DialogViewModelBase(IModelValidator validator) : base(validator)
        {
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="DialogViewModelBase" />
        /// </summary>
        protected DialogViewModelBase()
        {
        }

        /// <summary>
        ///     Gets the dialog session that created this dialog
        ///     <para>Not available until after the dialog has been opened</para>
        /// </summary>
        public DialogSession? Session
        {
            get => _session;
            private set => SetAndNotify(ref _session, value);
        }

        internal DialogViewModelHost? DialogViewModelHost
        {
            get => _dialogViewModelHost;
            set => SetAndNotify(ref _dialogViewModelHost, value);
        }

        /// <summary>
        ///     Called when the dialog has closed
        /// </summary>
        public virtual void OnDialogClosed(object sender, DialogClosingEventArgs e)
        {
        }

        /// <summary>
        ///     If not yet closed, closes the current session passing <see langword="false" />
        /// </summary>
        public virtual void Cancel()
        {
            if (Session != null && !Session.IsEnded)
                Session.Close(false);
        }

        internal void OnDialogOpened(object sender, DialogOpenedEventArgs e)
        {
            Session = e.Session;
        }
    }
}