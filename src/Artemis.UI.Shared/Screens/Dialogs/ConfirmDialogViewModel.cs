using Artemis.UI.Shared.Services.Dialog;

namespace Artemis.UI.Shared.Screens.Dialogs
{
    public class ConfirmDialogViewModel : DialogViewModelBase
    {
        public ConfirmDialogViewModel(string header, string text, string confirmText, string cancelText)
        {
            Header = header;
            Text = text;
            ConfirmText = confirmText;
            CancelText = cancelText;
        }

        public string Header { get; }
        public string Text { get; }
        public string ConfirmText { get; }
        public string CancelText { get; }

        public void Confirm()
        {
            if (!Session.IsEnded)
                Session.Close(true);
        }

        public void Cancel()
        {
            if (!Session.IsEnded)
                Session.Close(false);
        }
    }
}