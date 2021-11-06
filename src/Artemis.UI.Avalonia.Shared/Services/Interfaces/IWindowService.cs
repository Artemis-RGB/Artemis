using System;
using System.Threading.Tasks;
using Artemis.UI.Avalonia.Shared.Services.Builders;
using Avalonia.Controls;

namespace Artemis.UI.Avalonia.Shared.Services.Interfaces
{
    public interface IWindowService : IArtemisSharedUIService
    {
        /// <summary>
        /// Creates a view model instance of type <typeparamref name="T" /> and shows its corresponding View as a window
        /// </summary>
        /// <typeparam name="T">The type of view model to create</typeparam>
        /// <returns>The created view model</returns>
        T ShowWindow<T>();

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
        /// Given a ViewModel, show its corresponding View as a Dialog
        /// </summary>
        /// <typeparam name="T">The return type</typeparam>
        /// <param name="viewModel">ViewModel to show the View for</param>
        /// <returns>A task containing the return value of type <typeparamref name="T" /></returns>
        Task<T> ShowDialogAsync<T>(object viewModel);

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

        Window GetCurrentWindow();
    }
}