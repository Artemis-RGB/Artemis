using System;
using System.Windows;
using Artemis.Core;
using Artemis.UI.DataModelVisualization.Display;
using Artemis.UI.Screens.Settings.Debug;
using Artemis.UI.Services.Interfaces;
using Artemis.UI.Shared.Services;
using MaterialDesignExtensions.Controls;
using Ninject;
using Stylet;

namespace Artemis.UI.Services
{
    public class DebugService : IDebugService
    {
        private readonly IKernel _kernel;
        private readonly IWindowManager _windowManager;
        private DebugViewModel _debugViewModel;

        public DebugService(IKernel kernel, IWindowManager windowManager)
        {
            _kernel = kernel;
            _windowManager = windowManager;
        }

        public void ShowDebugger()
        {
            if (_debugViewModel != null)
                BringDebuggerToForeground();
            else
                CreateDebugger();
        }

        private void CreateDebugger()
        {
            _debugViewModel = _kernel.Get<DebugViewModel>();
            _debugViewModel.Closed += DebugViewModelOnClosed;

            _windowManager.ShowWindow(_debugViewModel);
        }

        private void DebugViewModelOnClosed(object sender, CloseEventArgs e)
        {
            _debugViewModel.Closed -= DebugViewModelOnClosed;
            _debugViewModel = null;
        }

        private void BringDebuggerToForeground()
        {
            var materialWindow = (MaterialWindow) _debugViewModel.View;

            // Not as straightforward as you might think, this ensures the window always shows, even if it's behind another window etc.
            // https://stackoverflow.com/a/4831839/5015269
            if (!materialWindow.IsVisible)
                materialWindow.Show();

            if (materialWindow.WindowState == WindowState.Minimized)
                materialWindow.WindowState = WindowState.Normal;

            materialWindow.Activate();
            materialWindow.Topmost = true; // important
            materialWindow.Topmost = false; // important
            materialWindow.Focus();
        }
    }
}