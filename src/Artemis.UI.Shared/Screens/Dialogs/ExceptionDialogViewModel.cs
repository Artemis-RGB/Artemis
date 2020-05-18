using System;
using System.Collections.Generic;
using Artemis.UI.Shared.Services.Dialog;
using ICSharpCode.AvalonEdit.Document;

namespace Artemis.UI.Shared.Screens.Dialogs
{
    public class ExceptionDialogViewModel : DialogViewModelBase
    {
        public ExceptionDialogViewModel(string message, Exception exception)
        {
            Header = message;
            Exceptions = new List<DialogException>();

            var currentException = exception;
            while (currentException != null)
            {
                Exceptions.Add(new DialogException(currentException));
                currentException = currentException.InnerException;
            }
        }

        public string Header { get; }
        public List<DialogException> Exceptions { get; set; }


        public void Close()
        {
            Session.Close();
        }
    }

    public class DialogException
    {
        public Exception Exception { get; }
        public IDocument Document { get; set; }

        public DialogException(Exception exception)
        {
            Exception = exception;
            Document = new TextDocument(new StringTextSource($"{exception.Message}\r\n\r\n{exception.StackTrace}"));
        }
    }
}