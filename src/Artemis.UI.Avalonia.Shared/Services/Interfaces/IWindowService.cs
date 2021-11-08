using System;
using System.Threading.Tasks;
using Artemis.UI.Avalonia.Shared.Services.Builders;
using Avalonia.Controls;

namespace Artemis.UI.Avalonia.Shared.Services.Interfaces
{
    public interface IWindowService : IArtemisSharedUIService
    {
        /// <summary>
        /// Creates a view model instance of type <typeparamref name="TViewModel" /> and shows its corresponding View as a window
        /// </summary>
        /// <typeparam name="TViewModel">The type of view model to create</typeparam>
        /// <returns>The created view model</returns>
        TViewModel ShowWindow<TViewModel>(params (string name, object value)[] parameters);

        /// <summary>
        /// Given a ViewModel, show its corresponding View as a window
        /// </summary>
        /// <param name="viewModel">ViewModel to show the View for</param>
        void ShowWindow(object viewModel);

        /// <summary>
        /// Shows a dialog displaying the given exception
        /// </summary>
        /// <param name="title">The title of the dialog</param>
        /// <param name="exception">The exception to display</param>
        void ShowExceptionDialog(string title, Exception exception);

        /// <summary>
        /// Given an existing ViewModel, show its corresponding View as a Dialog
        /// </summary>
        /// <typeparam name="TResult">The return type</typeparam>
        /// <param name="viewModel">ViewModel to show the View for</param>
        /// <returns>A task containing the return value of type <typeparamref name="TResult" /></returns>
        Task<TResult> ShowDialogAsync<TResult>(DialogViewModelBase<TResult> viewModel);

        /// <summary>
        /// Creates a view model instance of type <typeparamref name="TViewModel"/> and shows its corresponding View as a Dialog
        /// </summary>
        /// <typeparam name="TViewModel">The view model type</typeparam>
        /// <typeparam name="TResult">The return type</typeparam>
        /// <returns>A task containing the return value of type <typeparamref name="TResult" /></returns>
        Task<TResult> ShowDialogAsync<TViewModel, TResult>(params (string name, object value)[] parameters) where TViewModel : DialogViewModelBase<TResult>;

        /// <summary>
        /// Creates an open file dialog, use the fluent API to configure it
        /// </summary>
        /// <returns>The builder that can be used to configure the dialog</returns>
        OpenFileDialogBuilder CreateOpenFileDialog();

        /// <summary>
        /// Creates a save file dialog, use the fluent API to configure it
        /// </summary>
        /// <returns>The builder that can be used to configure the dialog</returns>
        SaveFileDialogBuilder CreateSaveFileDialog();

        ContentDialogBuilder CreateContentDialog();

        ConfirmDialogBuilder CreateConfirmDialog();

        Window GetCurrentWindow();
    }
}