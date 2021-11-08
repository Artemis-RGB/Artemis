using System;
using System.Threading.Tasks;
using Artemis.UI.Avalonia.Shared.Utilities;
using Avalonia.Controls;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using Button = Avalonia.Controls.Button;

namespace Artemis.UI.Avalonia.Shared.Services.Builders
{
    public class NotificationBuilder
    {
        private readonly InfoBar _infoBar;
        private readonly Window _parent;
        private TimeSpan _timeout = TimeSpan.FromSeconds(5);

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


        /// <summary>
        ///     Add a filter to the dialog
        /// </summary>
        public NotificationBuilder HavingButton(Action<NotificationButtonBuilder> configure)
        {
            NotificationButtonBuilder builder = new();
            configure(builder);
            _infoBar.ActionButton = builder.Build();

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

    public class NotificationButtonBuilder
    {
        private string _text = "Text";
        private Action? _action;

        public NotificationButtonBuilder WithText(string text)
        {
            _text = text;
            return this;
        }

        public NotificationButtonBuilder WithAction(Action action)
        {
            _action = action;
            return this;
        }

        public IControl Build()
        {
            return _action != null 
                ? new Button {Content = _text, Command = ReactiveCommand.Create(() => _action)} 
                : new Button {Content = _text};
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