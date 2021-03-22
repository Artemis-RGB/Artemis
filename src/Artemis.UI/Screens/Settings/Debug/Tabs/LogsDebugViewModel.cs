using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Artemis.Core;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Serilog.Events;
using Serilog.Formatting.Display;
using Stylet;

namespace Artemis.UI.Screens.Settings.Debug.Tabs
{
    public class LogsDebugViewModel : Screen
    {
        private readonly IDialogService _dialogService;
        private readonly MessageTemplateTextFormatter _formatter;
        private ScrollViewer _scrollViewer;

        public LogsDebugViewModel(IDialogService dialogService)
        {
            DisplayName = "LOGS";
            _dialogService = dialogService;
            _formatter = new MessageTemplateTextFormatter(
                "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}"
            );
        }

        public FlowDocument LogsDocument { get; } = new();

        public void ShowLogsFolder()
        {
            Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", Path.Combine(Constants.DataFolder, "Logs"));
        }

        public async Task UploadLogs()
        {
            bool confirmed = await _dialogService.ShowConfirmDialogAt(
                "DebuggerDialog",
                "Upload logs",
                "Automatically uploading logs is not yet implemented.\r\n\r\n" +
                "To manually upload a log simply drag the log file from the logs folder\r\n" +
                "into Discord or the GitHub issue textbox, depending on what you're using.",
                "OPEN LOGS FOLDER",
                "CANCEL");

            if (confirmed)
                ShowLogsFolder();
        }

        private Paragraph CreateLogEventParagraph(LogEvent logEvent)
        {
            Paragraph paragraph = new(new Run(RenderLogEvent(logEvent)))
            {
                // But mah MVVM
                Foreground = logEvent.Level switch
                {
                    LogEventLevel.Verbose => new SolidColorBrush(Colors.White),
                    LogEventLevel.Debug => new SolidColorBrush(Color.FromRgb(216, 216, 216)),
                    LogEventLevel.Information => new SolidColorBrush(Color.FromRgb(93, 201, 255)),
                    LogEventLevel.Warning => new SolidColorBrush(Color.FromRgb(255, 177, 53)),
                    LogEventLevel.Error => new SolidColorBrush(Color.FromRgb(255, 63, 63)),
                    LogEventLevel.Fatal => new SolidColorBrush(Colors.Red),
                    _ => throw new ArgumentOutOfRangeException()
                }
            };

            return paragraph;
        }

        private string RenderLogEvent(LogEvent logEvent)
        {
            using StringWriter writer = new();
            _formatter.Format(logEvent, writer);
            return writer.ToString().Trim();
        }

        #region Overrides of Screen

        /// <inheritdoc />
        protected override void OnActivate()
        {
            LogsDocument.Blocks.AddRange(LogStore.Events.Select(e => CreateLogEventParagraph(e)));
            LogStore.EventAdded += LogStoreOnEventAdded;

            base.OnActivate();
        }

        private void LogStoreOnEventAdded(object sender, LogEventEventArgs e)
        {
            Execute.PostToUIThread(() =>
            {
                LogsDocument.Blocks.Add(CreateLogEventParagraph(e.LogEvent));
                while (LogsDocument.Blocks.Count > 500)
                    LogsDocument.Blocks.Remove(LogsDocument.Blocks.FirstBlock);

                if (_scrollViewer != null && Math.Abs(_scrollViewer.VerticalOffset - _scrollViewer.ScrollableHeight) < 10)
                    _scrollViewer.ScrollToBottom();
            });
        }

        /// <inheritdoc />
        protected override void OnDeactivate()
        {
            LogStore.EventAdded -= LogStoreOnEventAdded;
            LogsDocument.Blocks.Clear();

            base.OnDeactivate();
        }

        /// <inheritdoc />
        protected override void OnViewLoaded()
        {
            ScrollViewer scrollViewer = VisualTreeUtilities.FindChild<ScrollViewer>(View, null);
            _scrollViewer = scrollViewer;
            _scrollViewer?.ScrollToBottom();

            base.OnViewLoaded();
        }

        #endregion
    }
}