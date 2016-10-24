using System.Collections.ObjectModel;
using System.Windows.Controls;
using Artemis.Utilities;
using NLog;

namespace Artemis.Controls.Log
{
    /// <summary>
    ///     Interaction logic for LoggingControl.xaml
    /// </summary>
    public partial class LoggingControl : UserControl
    {
        public LoggingControl()
        {
            LogCollection = new ObservableCollection<LogEventInfo>();

            InitializeComponent();

            // init memory queue
            Logging.ClearLoggingEvent();
            Logging.MemoryEvent += EventReceived;
        }

        public static ObservableCollection<LogEventInfo> LogCollection { get; set; }

        private async void EventReceived(LogEventInfo message)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                if (LogCollection.Count >= 50)
                    LogCollection.RemoveAt(LogCollection.Count - 1);

                LogCollection.Add(message);
            });
        }
    }
}