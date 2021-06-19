using System;
using System.Windows;
using MaterialDesignThemes.Wpf;
using Stylet;

namespace Artemis.UI.Shared.Screens.Exceptions
{
    internal class ExceptionViewModel : Screen
    {
        public ExceptionViewModel(string message, Exception exception)
        {
            Header = message;
            Exception = exception.ToString();
            MessageQueue = new SnackbarMessageQueue(TimeSpan.FromSeconds(2));
        }

        public string Header { get; }
        public string Exception { get; }
        public SnackbarMessageQueue MessageQueue { get; }

        public void CopyException()
        {
            Clipboard.SetText(Exception);
            MessageQueue.Enqueue("Copied exception to clipboard");
        }

        public void Close()
        {
            RequestClose();
        }
    }
}