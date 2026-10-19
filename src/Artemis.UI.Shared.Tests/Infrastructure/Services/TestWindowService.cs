using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.Builders;
using Avalonia.Controls;

namespace Artemis.UI.Shared.Tests.Infrastructure.Services;

/// <summary>
///     A window service that opens nothing, it only records the exception dialogs it was asked to show.
/// </summary>
public class TestWindowService : IWindowService
{
    /// <summary>
    ///     Gets every exception dialog that would have been shown, in order. Dialogs may be requested from another thread.
    /// </summary>
    public List<(string Title, Exception Exception)> ExceptionDialogs { get; } = [];

    public void ShowExceptionDialog(string title, Exception exception, string? customMessage = null)
    {
        lock (ExceptionDialogs)
            ExceptionDialogs.Add((title, exception));
    }

    public Window ShowWindow<TViewModel>(out TViewModel viewModel, params object[] parameters) => throw new NotSupportedException();
    public Window ShowWindow(object viewModel) => throw new NotSupportedException();
    public Task<TViewModel> ShowDialogAsync<TViewModel>(params object[] parameters) => throw new NotSupportedException();
    public Task ShowDialogAsync(object viewModel) => throw new NotSupportedException();
    public Task<TResult> ShowDialogAsync<TResult>(DialogViewModelBase<TResult> viewModel) => throw new NotSupportedException();
    public Task<TResult> ShowDialogAsync<TViewModel, TResult>(params object[] parameters) where TViewModel : DialogViewModelBase<TResult> => throw new NotSupportedException();
    public Task<bool> ShowConfirmContentDialog(string title, string message, string confirm = "Confirm", string? cancel = "Cancel") => throw new NotSupportedException();
    public OpenFolderDialogBuilder CreateOpenFolderDialog() => throw new NotSupportedException();
    public OpenFileDialogBuilder CreateOpenFileDialog() => throw new NotSupportedException();
    public SaveFileDialogBuilder CreateSaveFileDialog() => throw new NotSupportedException();
    public ContentDialogBuilder CreateContentDialog() => throw new NotSupportedException();
    public Window? GetCurrentWindow() => throw new NotSupportedException();
}
