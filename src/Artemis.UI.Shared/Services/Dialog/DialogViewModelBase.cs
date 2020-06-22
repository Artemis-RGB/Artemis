using MaterialDesignThemes.Wpf;
using Stylet;

namespace Artemis.UI.Shared.Services.Dialog
{
    public abstract class DialogViewModelBase : ValidatingModelBase
    {
        private DialogViewModelHost _dialogViewModelHost;
        private DialogSession _session;

        protected DialogViewModelBase(IModelValidator validator) : base(validator)
        {
        }

        protected DialogViewModelBase()
        {
        }

        public DialogViewModelHost DialogViewModelHost
        {
            get => _dialogViewModelHost;
            set => SetAndNotify(ref _dialogViewModelHost, value);
        }

        public DialogSession Session
        {
            get => _session;
            private set => SetAndNotify(ref _session, value);
        }

        public void OnDialogOpened(object sender, DialogOpenedEventArgs e)
        {
            Session = e.Session;
        }

        public virtual void OnDialogClosed(object sender, DialogClosingEventArgs e)
        {
        }
    }
}