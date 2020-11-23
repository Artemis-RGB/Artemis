using Artemis.UI.Shared.Services;

namespace Artemis.UI.Shared.Screens.Dialogs
{
    internal class ConfirmDialogViewModel : DialogViewModelBase
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
            if (Session != null && !Session.IsEnded)
                Session.Close(true);
        }
    }
}