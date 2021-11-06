using System;
using System.Linq;
using System.Threading.Tasks;
using Artemis.UI.Avalonia.Shared.Exceptions;
using Artemis.UI.Avalonia.Shared.Services.Builders;
using Artemis.UI.Avalonia.Shared.Services.Interfaces;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Ninject;

namespace Artemis.UI.Avalonia.Shared.Services
{
    internal class WindowService : IWindowService
    {
        private readonly IKernel _kernel;
        private bool _exceptionDialogOpen;

        public WindowService(IKernel kernel)
        {
            _kernel = kernel;
        }

        public T ShowWindow<T>()
        {
            T viewModel = _kernel.Get<T>()!;
            ShowWindow(viewModel);
            return viewModel;
        }

        public void ShowWindow(object viewModel)
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
        }

        /// <inheritdoc />
        public void ShowExceptionDialog(string title, Exception exception)
        {
            if (_exceptionDialogOpen)
                return;

            try
            {
                _exceptionDialogOpen = true;
                ShowDialogAsync<object>(new ExceptionDialogViewModel(title, exception)).GetAwaiter().GetResult();
            }
            finally
            {
                _exceptionDialogOpen = false;
            }
        }

        public async Task<T> ShowDialogAsync<T>(object viewModel)
        {
            if (Application.Current.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime classic)
                throw new ArtemisSharedUIException($"Can't show a dialog when application lifetime is not IClassicDesktopStyleApplicationLifetime.");

            string name = viewModel.GetType().FullName!.Split('`')[0].Replace("ViewModel", "View");
            Type? type = viewModel.GetType().Assembly.GetType(name);

            if (type == null)
                throw new ArtemisSharedUIException($"Failed to find a window named {name}.");
            if (!type.IsAssignableTo(typeof(Window)))
                throw new ArtemisSharedUIException($"Type {name} is not a window.");

            Window window = (Window) Activator.CreateInstance(type)!;
            window.DataContext = viewModel;
            Window parent = classic.Windows.FirstOrDefault(w => w.IsActive) ?? classic.MainWindow;
            return await window.ShowDialog<T>(parent);
        }

        public ContentDialogBuilder CreateContentDialog()
        {
            return new ContentDialogBuilder(_kernel, GetCurrentWindow());
        }

        public OpenFileDialogBuilder CreateOpenFileDialog()
        {
            return new OpenFileDialogBuilder(GetCurrentWindow());
        }

        public SaveFileDialogBuilder CreateSaveFileDialog()
        {
            return new SaveFileDialogBuilder(GetCurrentWindow());
        }

        public Window GetCurrentWindow()
        {
            if (Application.Current.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime classic)
                throw new ArtemisSharedUIException("Can't show a dialog when application lifetime is not IClassicDesktopStyleApplicationLifetime.");

            Window parent = classic.Windows.FirstOrDefault(w => w.IsActive) ?? classic.MainWindow;
            return parent;
        }
    }
}