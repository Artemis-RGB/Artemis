using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;

namespace Artemis.UI.Avalonia.Shared.Services.Builders
{
    public class NotificationBuilder
    {
        private readonly InfoBar _infoBar;
        private readonly Window _parent;
        private TimeSpan _timeout = TimeSpan.FromSeconds(3);

        public NotificationBuilder(Window parent)
        {
            _parent = parent;
            _infoBar = new InfoBar {Classes = Classes.Parse("notification-info-bar")};
        }

        public NotificationBuilder WithTitle(string? title)
        {
            _infoBar.Title = title;
            return this;
        }

        public NotificationBuilder WithMessage(string? content)
        {
            _infoBar.Message = content;
            return this;
        }

        public NotificationBuilder WithTimeout(TimeSpan timeout)
        {
            _timeout = timeout;
            return this;
        }

        public NotificationBuilder WithSeverity(NotificationSeverity severity)
        {
            _infoBar.Severity = (InfoBarSeverity) severity;
            return this;
        }

        public void Show()
        {
            if (_parent.Content is not Panel panel)
                return;

            Dispatcher.UIThread.Post(() =>
            {
                panel.Children.Add(_infoBar);
                _infoBar.Closed += InfoBarOnClosed;
                _infoBar.IsOpen = true;
            });

            Task.Run(async () =>
            {
                await Task.Delay(_timeout);
                Dispatcher.UIThread.Post(() => _infoBar.IsOpen = false);
            });
        }

        private void InfoBarOnClosed(InfoBar sender, InfoBarClosedEventArgs args)
        {
            _infoBar.Closed -= InfoBarOnClosed;
            if (_parent.Content is not Panel panel)
                return;

            panel.Children.Remove(_infoBar);
        }
    }

    public enum NotificationSeverity
    {
        Informational,
        Success,
        Warning,
        Error
    }
}