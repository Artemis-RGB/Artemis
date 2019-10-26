using Artemis.UI.ViewModels.Utilities;
using MaterialDesignThemes.Wpf;
using Stylet;

namespace Artemis.UI.ViewModels.Dialogs
{
    public abstract class DialogViewModelBase : ValidatingModelBase
    {
        protected DialogViewModelBase(IModelValidator validator) : base(validator)
        {
        }

        protected DialogViewModelBase()
        {
        }

        public DialogViewModelHost DialogViewModelHost { get; set; }
        public DialogSession Session { get; private set; }

        public void OnDialogOpened(object sender, DialogOpenedEventArgs e)
        {
            Session = e.Session;
        }

        public virtual void OnDialogClosed(object sender, DialogClosingEventArgs e)
        {
        }
    }
}