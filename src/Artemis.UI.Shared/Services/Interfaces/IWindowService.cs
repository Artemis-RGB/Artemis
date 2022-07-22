using System;
using System.Threading.Tasks;
using Artemis.UI.Shared.Services.Builders;
using Avalonia.Controls;

namespace Artemis.UI.Shared.Services
{
    /// <summary>
    ///     A service that can be used to show windows and dialogs.
    /// </summary>
    public interface IWindowService : IArtemisSharedUIService
    {
        /// <summary>
        ///     Creates a view model instance of type <typeparamref name="TViewModel" /> and shows its corresponding View as a
        ///     window
        /// </summary>
        /// <typeparam name="TViewModel">The type of view model to create</typeparam>
        /// <returns>The created view model</returns>
        TViewModel ShowWindow<TViewModel>(params (string name, object value)[] parameters);

        /// <summary>
        ///     Given a ViewModel, show its corresponding View as a window
        /// </summary>
        /// <param name="viewModel">ViewModel to show the View for</param>
        Window ShowWindow(object viewModel);

        /// <summary>
        ///     Shows a dialog displaying the given exception
        /// </summary>
        /// <param name="title">The title of the dialog</param>
        /// <param name="exception">The exception to display</param>
        void ShowExceptionDialog(string title, Exception exception);

        /// <summary>
        ///     Given an existing ViewModel, show its corresponding View as a Dialog
        /// </summary>
        /// <typeparam name="TResult">The return type</typeparam>
        /// <param name="viewModel">ViewModel to show the View for</param>
        /// <returns>A task containing the return value of type <typeparamref name="TResult" /></returns>
        Task<TResult> ShowDialogAsync<TResult>(DialogViewModelBase<TResult> viewModel);

        /// <summary>
        ///     Creates a view model instance of type <typeparamref name="TViewModel" /> and shows its corresponding View as a
        ///     Dialog
        /// </summary>
        /// <typeparam name="TViewModel">The view model type</typeparam>
        /// <typeparam name="TResult">The return type</typeparam>
        /// <returns>A task containing the return value of type <typeparamref name="TResult" /></returns>
        Task<TResult> ShowDialogAsync<TViewModel, TResult>(params (string name, object? value)[] parameters) where TViewModel : DialogViewModelBase<TResult>;

        /// <summary>
        ///     Shows a content dialog asking the user to confirm an action
        /// </summary>
        /// <param name="title">The title of the dialog</param>
        /// <param name="message">The message of the dialog</param>
        /// <param name="confirm">The text of the confirm button</param>
        /// <param name="cancel">The text of the cancel button, if <see langword="null" /> the cancel button will not be shown</param>
        /// <returns>
        ///     A task containing the result of the dialog, <see langword="true" /> if confirmed; otherwise
        ///     <see langword="false" />
        /// </returns>
        Task<bool> ShowConfirmContentDialog(string title, string message, string confirm = "Confirm", string? cancel = "Cancel");

        /// <summary>
        ///     Creates an open folder dialog, use the fluent API to configure it
        /// </summary>
        /// <returns>The builder that can be used to configure the dialog</returns>
        OpenFolderDialogBuilder CreateOpenFolderDialog();
        
        /// <summary>
        ///     Creates an open file dialog, use the fluent API to configure it
        /// </summary>
        /// <returns>The builder that can be used to configure the dialog</returns>
        OpenFileDialogBuilder CreateOpenFileDialog();

        /// <summary>
        ///     Creates a save file dialog, use the fluent API to configure it
        /// </summary>
        /// <returns>The builder that can be used to configure the dialog</returns>
        SaveFileDialogBuilder CreateSaveFileDialog();

        /// <summary>
        ///     Creates a content dialog, use the fluent API to configure it
        /// </summary>
        /// <returns>The builder that can be used to configure the dialog</returns>
        ContentDialogBuilder CreateContentDialog();

        /// <summary>
        ///     Gets the current window of the application
        /// </summary>
        /// <returns>The current window of the application</returns>
        Window? GetCurrentWindow();
    }
}