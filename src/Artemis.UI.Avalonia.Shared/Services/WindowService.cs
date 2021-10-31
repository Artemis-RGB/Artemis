using System;
using System.Linq;
using System.Threading.Tasks;
using Artemis.UI.Avalonia.Shared.Exceptions;
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

        public WindowService(IKernel kernel)
        {
            _kernel = kernel;
        }

        #region Implementation of IWindowService

        /// <inheritdoc />
        public T ShowWindow<T>()
        {
            T viewModel = _kernel.Get<T>()!;
            ShowWindow(viewModel);
            return viewModel;
        }

        /// <inheritdoc />
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
        public async Task<T> ShowDialog<T>(object viewModel)
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

        #endregion
    }
}