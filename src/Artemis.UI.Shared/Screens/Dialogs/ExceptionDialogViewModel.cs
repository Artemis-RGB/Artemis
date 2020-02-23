using System;
using Artemis.UI.Shared.Services.Dialog;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;

namespace Artemis.UI.Shared.Screens.Dialogs
{
    public class ExceptionDialogViewModel : DialogViewModelBase
    {
        public ExceptionDialogViewModel(string message, Exception exception)
        {
            Header = message;
            Exception = exception;
            Document = new TextDocument(new StringTextSource(exception.StackTrace));
        }

        public string Header { get; }
        public Exception Exception { get; }

        public IDocument Document { get; set; }

        public void Close()
        {
            Session.Close();
        }
    }
}