using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using DryIoc;
using FluentAvalonia.UI.Controls;
using ReactiveUI;

namespace Artemis.UI.Shared.Services.Builders;

/// <summary>
///     Represents a builder that can be used to create Fluent UI dialogs.
/// </summary>
public class ContentDialogBuilder
{
    private readonly ContentDialog _contentDialog;
    private readonly IContainer _container;
    private readonly Window _parent;
    private ContentDialogViewModelBase? _viewModel;

    internal ContentDialogBuilder(IContainer container, Window parent)
    {
        _container = container;
        _parent = parent;
        _contentDialog = new ContentDialog
        {
            CloseButtonText = "Close"
        };
    }

    /// <summary>
    ///     Changes the title of the dialog.
    /// </summary>
    /// <param name="title">The new title.</param>
    /// <returns>The builder that can be used to further build the dialog.</returns>
    public ContentDialogBuilder WithTitle(string? title)
    {
        _contentDialog.Title = title;
        return this;
    }

    /// <summary>
    ///     Changes the content of the dialog.
    /// </summary>
    /// <param name="content">The new content.</param>
    /// <returns>The builder that can be used to further build the dialog.</returns>
    public ContentDialogBuilder WithContent(string? content)
    {
        _contentDialog.Content = content;
        return this;
    }

    /// <summary>
    ///     Changes the default button of the dialog that is pressed on enter.
    /// </summary>
    /// <param name="defaultButton">The default button.</param>
    /// <returns>The builder that can be used to further build the dialog.</returns>
    public ContentDialogBuilder WithDefaultButton(ContentDialogButton defaultButton)
    {
        _contentDialog.DefaultButton = (FluentAvalonia.UI.Controls.ContentDialogButton) defaultButton;
        return this;
    }

    /// <summary>
    ///     Changes the primary button of the dialog.
    /// </summary>
    /// <param name="configure">An action to configure the button.</param>
    /// <returns>The builder that can be used to further build the dialog.</returns>
    public ContentDialogBuilder HavingPrimaryButton(Action<ContentDialogButtonBuilder> configure)
    {
        ContentDialogButtonBuilder builder = new();
        configure(builder);

        _contentDialog.IsPrimaryButtonEnabled = true;
        _contentDialog.PrimaryButtonText = builder.Text;
        _contentDialog.PrimaryButtonCommand = builder.Command;
        _contentDialog.PrimaryButtonCommandParameter = builder.CommandParameter;

        // I feel like this isn't my responsibility...
        if (builder.Command != null)
        {
            _contentDialog.IsPrimaryButtonEnabled = builder.Command.CanExecute(builder.CommandParameter);
            builder.Command.CanExecuteChanged += (_, _) => _contentDialog.IsPrimaryButtonEnabled = builder.Command.CanExecute(builder.CommandParameter);
        }

        return this;
    }

    /// <summary>
    ///     Changes the secondary button of the dialog.
    /// </summary>
    /// <param name="configure">An action to configure the button.</param>
    /// <returns>The builder that can be used to further build the dialog.</returns>
    public ContentDialogBuilder HavingSecondaryButton(Action<ContentDialogButtonBuilder> configure)
    {
        ContentDialogButtonBuilder builder = new();
        configure(builder);

        _contentDialog.IsSecondaryButtonEnabled = true;
        _contentDialog.SecondaryButtonText = builder.Text;
        _contentDialog.SecondaryButtonCommand = builder.Command;
        _contentDialog.SecondaryButtonCommandParameter = builder.CommandParameter;

        // I feel like this isn't my responsibility...
        if (builder.Command != null)
        {
            _contentDialog.IsSecondaryButtonEnabled = builder.Command.CanExecute(builder.CommandParameter);
            builder.Command.CanExecuteChanged += (_, _) => _contentDialog.IsSecondaryButtonEnabled = builder.Command.CanExecute(builder.CommandParameter);
        }
        
        return this;
    }

    /// <summary>
    ///     Changes the text of the close button of the dialog.
    /// </summary>
    /// <param name="text">The new text.</param>
    /// <returns>The builder that can be used to further build the dialog.</returns>
    public ContentDialogBuilder WithCloseButtonText(string? text)
    {
        _contentDialog.CloseButtonText = text;
        return this;
    }

    /// <summary>
    ///     Changes the view model of the content dialog, hosting it inside the dialog.
    /// </summary>
    /// <typeparam name="T">The type of the view model to host.</typeparam>
    /// <param name="viewModel">The resulting view model.</param>
    /// <param name="parameters">Optional parameters to pass to the constructor of the view model.</param>
    /// <returns>The builder that can be used to further build the dialog.</returns>
    public ContentDialogBuilder WithViewModel<T>(out T viewModel, params object[] parameters) where T : ContentDialogViewModelBase
    {
        viewModel = _container.Resolve<T>(parameters);
        viewModel.ContentDialog = _contentDialog;
        _contentDialog.Content = viewModel;

        _viewModel = viewModel;
        return this;
    }

    /// <summary>
    ///     Changes the dialog to take the full height of the window it's being hosted in.
    /// </summary>
    /// <returns>The builder that can be used to further build the dialog.</returns>
    public ContentDialogBuilder WithFullSize()
    {
        _contentDialog.FullSizeDesired = true;
        return this;
    }
    
    /// <summary>
    ///     Changes the dialog to be full screen.
    /// </summary>
    /// <returns>The builder that can be used to further build the dialog.</returns>
    public ContentDialogBuilder WithFullScreen()
    {
        _contentDialog.Classes.Add("fullscreen");
        return this;
    }

    /// <summary>
    ///     Asynchronously shows the content dialog.
    /// </summary>
    /// <returns>A task containing the result of the content dialog.</returns>
    /// <exception cref="ArtemisSharedUIException">Thrown when the parent window does not contain a panel at its root.</exception>
    public async Task<ContentDialogResult> ShowAsync()
    {
        if (_parent.Content is not Panel panel)
            throw new ArtemisSharedUIException($"The parent window {_parent.GetType().FullName} should contain a panel at its root");

        try
        {
            panel.Children.Add(_contentDialog);
            ContentDialogResult result = await _contentDialog.ShowAsync();

            // Take the dialog away from the VM in case it's going to try to hide it again or whatever...
            if (_viewModel != null)
                _viewModel.ContentDialog = null;

            return result;
        }
        finally
        {
            panel.Children.Remove(_contentDialog);
        }
    }
}

/// <summary>
///     Represents a content dialog button.
/// </summary>
public enum ContentDialogButton
{
    /// <summary>
    ///     No button.
    /// </summary>
    None,

    /// <summary>
    ///     The primary button.
    /// </summary>
    Primary,

    /// <summary>
    ///     The secondary button.
    /// </summary>
    Secondary,

    /// <summary>
    ///     The close button.
    /// </summary>
    Close
}

/// <summary>
///     Represents a builder that can be used to create buttons inside content dialogs.
/// </summary>
public class ContentDialogButtonBuilder
{
    internal ContentDialogButtonBuilder()
    {
    }

    internal string? Text { get; set; }
    internal ICommand? Command { get; set; }
    internal Action? Action { get; set; }
    internal object? CommandParameter { get; set; }

    /// <summary>
    ///     Changes text message of the button.
    /// </summary>
    /// <param name="text">The new text.</param>
    /// <returns>The notification builder that can be used to further build the button.</returns>
    public ContentDialogButtonBuilder WithText(string? text)
    {
        Text = text;
        return this;
    }

    /// <summary>
    ///     Changes action that is called when the button is clicked.
    /// </summary>
    /// <param name="action">The action to call when the button is clicked.</param>
    /// <returns>The builder that can be used to further build the button.</returns>
    public ContentDialogButtonBuilder WithAction(Action action)
    {
        Action = action;
        Command = ReactiveCommand.Create(() => Action());
        return this;
    }

    /// <summary>
    ///     Changes command that is called when the button is clicked.
    /// </summary>
    /// <param name="command">The command to call when the button is clicked.</param>
    /// <returns>The builder that can be used to further build the button.</returns>
    public ContentDialogButtonBuilder WithCommand(ICommand? command)
    {
        Command = command;
        return this;
    }

    /// <summary>
    ///     Changes parameter of the command that is called when the button is clicked.
    /// </summary>
    /// <param name="commandParameter">The parameter of the command to call when the button is clicked.</param>
    /// <returns>The builder that can be used to further build the button.</returns>
    public ContentDialogButtonBuilder WithCommandParameter(object? commandParameter)
    {
        CommandParameter = commandParameter;
        return this;
    }
}