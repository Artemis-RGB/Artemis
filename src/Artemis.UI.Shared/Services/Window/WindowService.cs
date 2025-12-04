using System;
using System.Linq;
using System.Threading.Tasks;
using Artemis.UI.Shared.Services.Builders;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using DryIoc;
using FluentAvalonia.UI.Controls;
using ContentDialogButton = Artemis.UI.Shared.Services.Builders.ContentDialogButton;

namespace Artemis.UI.Shared.Services;

internal class WindowService : IWindowService
{
    private readonly IContainer _container;
    private bool _exceptionDialogOpen;

    public WindowService(IContainer container)
    {
        _container = container;
    }

    public Window ShowWindow<T>(out T viewModel, params object[] parameters)
    {
        viewModel = _container.Resolve<T>(parameters);
        if (viewModel == null)
            throw new ArtemisSharedUIException($"Failed to show window for VM of type {typeof(T).Name}, could not create instance.");
        
        return ShowWindow(viewModel);
    }

    public Window ShowWindow(object viewModel)
    {
        string name = viewModel.GetType().FullName!.Split('`')[0].Replace("ViewModel", "View");
        Type? type = viewModel.GetType().Assembly.GetType(name);

        if (type == null)
            throw new ArtemisSharedUIException($"Failed to find a window named {name}.");

        if (!type.IsAssignableTo(typeof(Window)))
            throw new ArtemisSharedUIException($"Type {name} is not a window.");

        Window window = (Window) Activator.CreateInstance(type)!;
        window.DataContext = viewModel;
        window.Show();

        return window;
    }

    public async Task<T> ShowDialogAsync<T>(params object[] parameters)
    {
        T viewModel = _container.Resolve<T>(parameters);
        if (viewModel == null)
            throw new ArtemisSharedUIException($"Failed to show window for VM of type {typeof(T).Name}, could not create instance.");
        
        await ShowDialogAsync(viewModel);
        return viewModel;
    }

    public async Task ShowDialogAsync(object viewModel)
    {
        Window? parent = GetCurrentWindow();
        
        if (parent == null)
            throw new ArtemisSharedUIException("Failed to get the current window.");
        
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

    public async Task<TResult> ShowDialogAsync<TViewModel, TResult>(params object[] parameters) where TViewModel : DialogViewModelBase<TResult>
    {
        TViewModel viewModel = _container.Resolve<TViewModel>(parameters);
        if (viewModel == null)
            throw new ArtemisSharedUIException($"Failed to show window for VM of type {typeof(TViewModel).Name}, could not create instance.");
        
        return await ShowDialogAsync(viewModel);
    }

    public async Task<bool> ShowConfirmContentDialog(string title, string message, string confirm = "Confirm", string? cancel = "Cancel")
    {
        ContentDialogResult contentDialogResult = await CreateContentDialog()
            .WithTitle(title)
            .WithContent(message)
            .HavingPrimaryButton(b => b.WithText(confirm))
            .WithDefaultButton(ContentDialogButton.Primary)
            .WithCloseButtonText(cancel)
            .ShowAsync();

        return contentDialogResult == ContentDialogResult.Primary;
    }

    public async Task<TResult> ShowDialogAsync<TResult>(DialogViewModelBase<TResult> viewModel)
    {
        Window? parent = GetCurrentWindow();
        
        if (parent == null)
            throw new ArtemisSharedUIException("Failed to get the current window.");

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
                await ShowDialogAsync(new ExceptionDialogViewModel(title, exception, _container.Resolve<INotificationService>()));
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
        return new ContentDialogBuilder(_container, currentWindow);
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