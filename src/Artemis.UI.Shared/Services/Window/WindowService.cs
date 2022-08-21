using System;
using System.Linq;
using System.Threading.Tasks;
using Artemis.UI.Shared.Services.Builders;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using Ninject;
using Ninject.Parameters;

namespace Artemis.UI.Shared.Services;

internal class WindowService : IWindowService
{
    private readonly IKernel _kernel;
    private bool _exceptionDialogOpen;

    public WindowService(IKernel kernel)
    {
        _kernel = kernel;
    }

    public T ShowWindow<T>(params (string name, object value)[] parameters)
    {
        IParameter[] paramsArray = parameters.Select(kv => new ConstructorArgument(kv.name, kv.value)).Cast<IParameter>().ToArray();
        T viewModel = _kernel.Get<T>(paramsArray)!;
        ShowWindow(viewModel);
        return viewModel;
    }

    public Window ShowWindow(object viewModel)
    {
        Window? parent = GetCurrentWindow();

        string name = viewModel.GetType().FullName!.Split('`')[0].Replace("ViewModel", "View");
        Type? type = viewModel.GetType().Assembly.GetType(name);

        if (type == null)
            throw new ArtemisSharedUIException($"Failed to find a window named {name}.");

        if (!type.IsAssignableTo(typeof(Window)))
            throw new ArtemisSharedUIException($"Type {name} is not a window.");

        Window window = (Window) Activator.CreateInstance(type)!;
        window.DataContext = viewModel;
        if (parent != null)
            window.Show(parent);
        else
            window.Show();

        return window;
    }

    public async Task<T> ShowDialogAsync<T>(params (string name, object value)[] parameters)
    {
        IParameter[] paramsArray = parameters.Select(kv => new ConstructorArgument(kv.name, kv.value)).Cast<IParameter>().ToArray();
        T viewModel = _kernel.Get<T>(paramsArray)!;
        await ShowDialogAsync(viewModel);
        return viewModel;
    }

    public async Task ShowDialogAsync(object viewModel)
    {
        Window? parent = GetCurrentWindow();

        string name = viewModel.GetType().FullName!.Split('`')[0].Replace("ViewModel", "View");
        Type? type = viewModel.GetType().Assembly.GetType(name);

        if (type == null)
            throw new ArtemisSharedUIException($"Failed to find a window named {name}.");

        if (!type.IsAssignableTo(typeof(Window)))
            throw new ArtemisSharedUIException($"Type {name} is not a window.");

        Window window = (Window) Activator.CreateInstance(type)!;
        window.DataContext = viewModel;
        await window.ShowDialog(parent);
    }

    public async Task<TResult> ShowDialogAsync<TViewModel, TResult>(params (string name, object? value)[] parameters) where TViewModel : DialogViewModelBase<TResult>
    {
        IParameter[] paramsArray = parameters.Select(kv => new ConstructorArgument(kv.name, kv.value)).Cast<IParameter>().ToArray();
        TViewModel viewModel = _kernel.Get<TViewModel>(paramsArray)!;
        return await ShowDialogAsync(viewModel);
    }

    public async Task<bool> ShowConfirmContentDialog(string title, string message, string confirm = "Confirm", string? cancel = "Cancel")
    {
        ContentDialogResult contentDialogResult = await CreateContentDialog()
            .WithTitle(title)
            .WithContent(message)
            .HavingPrimaryButton(b => b.WithText(confirm))
            .WithCloseButtonText(cancel)
            .ShowAsync();

        return contentDialogResult == ContentDialogResult.Primary;
    }

    public async Task<TResult> ShowDialogAsync<TResult>(DialogViewModelBase<TResult> viewModel)
    {
        Window? parent = GetCurrentWindow();

        string name = viewModel.GetType().FullName!.Split('`')[0].Replace("ViewModel", "View");
        Type? type = viewModel.GetType().Assembly.GetType(name);

        if (type == null)
            throw new ArtemisSharedUIException($"Failed to find a window named {name}.");

        if (!type.IsAssignableTo(typeof(Window)))
            throw new ArtemisSharedUIException($"Type {name} is not a window.");

        Window window = (Window) Activator.CreateInstance(type)!;
        window.DataContext = viewModel;
        viewModel.CloseRequested += (_, args) => window.Close(args.Result);

        return await window.ShowDialog<TResult>(parent);
    }

    public void ShowExceptionDialog(string title, Exception exception)
    {
        if (_exceptionDialogOpen)
            return;

        _exceptionDialogOpen = true;
        // Fire and forget the dialog
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            try
            {
                await ShowDialogAsync(new ExceptionDialogViewModel(title, exception, _kernel.Get<INotificationService>()));
            }
            finally
            {
                _exceptionDialogOpen = false;
            }
        });
    }

    public ContentDialogBuilder CreateContentDialog()
    {
        Window? currentWindow = GetCurrentWindow();
        if (currentWindow == null)
            throw new ArtemisSharedUIException("Can't show a content dialog without any windows being shown.");
        return new ContentDialogBuilder(_kernel, currentWindow);
    }

    public OpenFolderDialogBuilder CreateOpenFolderDialog()
    {
        Window? currentWindow = GetCurrentWindow();
        if (currentWindow == null)
            throw new ArtemisSharedUIException("Can't show an open folder dialog without any windows being shown.");
        return new OpenFolderDialogBuilder(currentWindow);
    }

    public OpenFileDialogBuilder CreateOpenFileDialog()
    {
        Window? currentWindow = GetCurrentWindow();
        if (currentWindow == null)
            throw new ArtemisSharedUIException("Can't show an open file dialog without any windows being shown.");
        return new OpenFileDialogBuilder(currentWindow);
    }

    public SaveFileDialogBuilder CreateSaveFileDialog()
    {
        Window? currentWindow = GetCurrentWindow();
        if (currentWindow == null)
            throw new ArtemisSharedUIException("Can't show a save file dialog without any windows being shown.");
        return new SaveFileDialogBuilder(currentWindow);
    }

    public Window? GetCurrentWindow()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime classic)
            throw new ArtemisSharedUIException("Find an open window when application lifetime is not IClassicDesktopStyleApplicationLifetime.");

        Window? parent = classic.Windows.FirstOrDefault(w => w.IsActive && w.ShowInTaskbar) ?? classic.MainWindow;
        return parent;
    }
}