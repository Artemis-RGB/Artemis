using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using Button = Avalonia.Controls.Button;

namespace Artemis.UI.Shared.Services.Builders;

/// <summary>
///     Represents a builder that can be used to create notifications.
/// </summary>
public class NotificationBuilder
{
    private readonly InfoBar _infoBar;
    private readonly Window _parent;
    private TimeSpan _timeout = TimeSpan.FromSeconds(5);

    /// <summary>
    ///     Creates a new instance of the <see cref="NotificationBuilder" /> class.
    /// </summary>
    /// <param name="parent">The parent window that will host the notification.</param>
    public NotificationBuilder(Window parent)
    {
        _parent = parent;
        _infoBar = new InfoBar {Classes = Classes.Parse("notification-info-bar")};
    }

    /// <summary>
    ///     Changes the title of the notification.
    /// </summary>
    /// <param name="title">The new title.</param>
    /// <returns>The notification builder that can be used to further build the notification.</returns>
    public NotificationBuilder WithTitle(string? title)
    {
        _infoBar.Title = title;
        return this;
    }

    /// <summary>
    ///     Changes the message of the notification.
    /// </summary>
    /// <param name="content">The new message.</param>
    /// <returns>The notification builder that can be used to further build the notification.</returns>
    public NotificationBuilder WithMessage(string? content)
    {
        _infoBar.Message = content;
        return this;
    }

    /// <summary>
    ///     Changes the timeout of the notification after which it disappears automatically.
    /// </summary>
    /// <param name="timeout">The timeout of the notification after which it disappears automatically.</param>
    /// <returns>The notification builder that can be used to further build the notification.</returns>
    public NotificationBuilder WithTimeout(TimeSpan timeout)
    {
        _timeout = timeout;
        return this;
    }

    /// <summary>
    ///     Changes the vertical position of the notification inside the parent window.
    /// </summary>
    /// <param name="position">The vertical position of the notification inside the parent window.</param>
    /// <returns>The notification builder that can be used to further build the notification.</returns>
    public NotificationBuilder WithVerticalPosition(VerticalAlignment position)
    {
        _infoBar.VerticalAlignment = position;
        return this;
    }

    /// <summary>
    ///     Changes the horizontal position of the notification inside the parent window.
    /// </summary>
    /// <param name="position">The horizontal position of the notification inside the parent window.</param>
    /// <returns>The notification builder that can be used to further build the notification.</returns>
    public NotificationBuilder WithHorizontalPosition(HorizontalAlignment position)
    {
        _infoBar.HorizontalAlignment = position;
        return this;
    }

    /// <summary>
    ///     Changes the severity (color) of the notification.
    /// </summary>
    /// <param name="severity">The severity (color) of the notification.</param>
    /// <returns>The notification builder that can be used to further build the notification.</returns>
    public NotificationBuilder WithSeverity(NotificationSeverity severity)
    {
        _infoBar.Severity = (InfoBarSeverity) severity;
        return this;
    }

    /// <summary>
    ///     Changes the action button of the notification.
    /// </summary>
    /// <param name="configure">An action to configure the button.</param>
    /// <returns>The notification builder that can be used to further build the notification.</returns>
    public NotificationBuilder HavingButton(Action<NotificationButtonBuilder> configure)
    {
        NotificationButtonBuilder builder = new();
        configure(builder);
        _infoBar.ActionButton = builder.Build();

        return this;
    }

    /// <summary>
    ///     Shows the notification.
    /// </summary>
    /// <returns>An action that can be called to hide the notification.</returns>
    /// <exception cref="ArtemisSharedUIException" />
    public Action Show()
    {
        IPanel? panel = _parent.Find<IPanel>("NotificationContainer");
        if (panel == null)
            throw new ArtemisSharedUIException("Can't display a notification on a window without a NotificationContainer.");

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

        return () => Dispatcher.UIThread.Post(() => _infoBar.IsOpen = false);
    }

    private void InfoBarOnClosed(InfoBar sender, InfoBarClosedEventArgs args)
    {
        _infoBar.Closed -= InfoBarOnClosed;
        if (_parent.Content is not Panel panel)
            return;

        panel.Children.Remove(_infoBar);
    }
}

/// <summary>
///     Represents a builder that can be used to create buttons inside notifications.
/// </summary>
public class NotificationButtonBuilder
{
    private Action? _action;
    private ICommand? _command;
    private object? _commandParameter;
    private string _text = "Text";

    /// <summary>
    ///     Changes text message of the button.
    /// </summary>
    /// <param name="text">The new text.</param>
    /// <returns>The notification builder that can be used to further build the button.</returns>
    public NotificationButtonBuilder WithText(string text)
    {
        _text = text;
        return this;
    }

    /// <summary>
    ///     Changes action that is called when the button is clicked.
    /// </summary>
    /// <param name="action">The action to call when the button is clicked.</param>
    /// <returns>The builder that can be used to further build the button.</returns>
    public NotificationButtonBuilder WithAction(Action action)
    {
        _command = null;
        _action = action;
        return this;
    }

    /// <summary>
    ///     Changes command that is called when the button is clicked.
    /// </summary>
    /// <param name="command">The command to call when the button is clicked.</param>
    /// <returns>The builder that can be used to further build the button.</returns>
    public NotificationButtonBuilder WithCommand(ICommand command)
    {
        _action = null;
        _command = command;
        return this;
    }

    /// <summary>
    ///     Changes parameter of the command that is called when the button is clicked.
    /// </summary>
    /// <param name="commandParameter">The parameter of the command to call when the button is clicked.</param>
    /// <returns>The builder that can be used to further build the button.</returns>
    public NotificationButtonBuilder WithCommandParameter(object? commandParameter)
    {
        _commandParameter = commandParameter;
        return this;
    }

    internal IControl Build()
    {
        if (_action != null)
            return new Button {Content = _text, Command = ReactiveCommand.Create(() => _action()), Classes = new Classes("AppBarButton")};
        if (_command != null)
            return new Button {Content = _text, Command = _command, CommandParameter = _commandParameter, Classes = new Classes("AppBarButton")};
        return new Button {Content = _text, Classes = new Classes("AppBarButton")};
    }
}

/// <summary>
///     Represents a severity of a notification.
/// </summary>
public enum NotificationSeverity
{
    /// <summary>
    ///     A severity for informational messages.
    /// </summary>
    Informational,

    /// <summary>
    ///     A severity for success messages.
    /// </summary>
    Success,

    /// <summary>
    ///     A severity for warning messages.
    /// </summary>
    Warning,

    /// <summary>
    ///     A severity for error messages.
    /// </summary>
    Error
}