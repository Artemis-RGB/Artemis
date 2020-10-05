using System;
using System.Collections.Generic;
using ICSharpCode.AvalonEdit.Document;
using Stylet;

namespace Artemis.UI.Shared.Screens.Exceptions
{
    internal class ExceptionViewModel : Screen
    {
        private List<DialogException> _exceptions;

        public ExceptionViewModel(string message, Exception exception)
        {
            Header = message;
            Exceptions = new List<DialogException>();

            Exception currentException = exception;
            while (currentException != null)
            {
                Exceptions.Add(new DialogException(currentException));
                currentException = currentException.InnerException;
            }
        }

        public string Header { get; }

        public List<DialogException> Exceptions
        {
            get => _exceptions;
            set => SetAndNotify(ref _exceptions, value);
        }
    }

    internal class DialogException
    {
        public DialogException(Exception exception)
        {
            Exception = exception;
            Document = new TextDocument(new StringTextSource($"{exception.Message}\r\n\r\n{exception.StackTrace}"));
        }

        public Exception Exception { get; }
        public IDocument Document { get; set; }
    }
}