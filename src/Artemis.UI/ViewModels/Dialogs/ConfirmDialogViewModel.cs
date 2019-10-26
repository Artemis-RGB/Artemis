namespace Artemis.UI.ViewModels.Dialogs
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
            Session.Close(true);
        }

        public void Cancel()
        {
            Session.Close(false);
        }
    }
}