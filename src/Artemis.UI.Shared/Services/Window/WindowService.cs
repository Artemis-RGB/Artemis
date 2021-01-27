using System;

namespace Artemis.UI.Shared.Services
{
    internal class WindowService : IWindowService
    {
        private IMainWindowProvider? _mainWindowManager;

        protected virtual void OnMainWindowOpened()
        {
            MainWindowOpened?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnMainWindowClosed()
        {
            MainWindowClosed?.Invoke(this, EventArgs.Empty);
        }

        private void SyncWithManager()
        {
            if (_mainWindowManager == null)
                return;

            if (IsMainWindowOpen && !_mainWindowManager.IsMainWindowOpen)
            {
                IsMainWindowOpen = false;
                OnMainWindowClosed();
            }

            if (!IsMainWindowOpen && _mainWindowManager.IsMainWindowOpen)
            {
                IsMainWindowOpen = true;
                OnMainWindowOpened();
            }
        }

        private void HandleMainWindowOpened(object? sender, EventArgs e)
        {
            SyncWithManager();
        }

        private void HandleMainWindowClosed(object? sender, EventArgs e)
        {
            SyncWithManager();
        }

        public bool IsMainWindowOpen { get; private set; }
        
        public void ConfigureMainWindowProvider(IMainWindowProvider mainWindowProvider)
        {
            if (mainWindowProvider == null) throw new ArgumentNullException(nameof(mainWindowProvider));

            if (_mainWindowManager != null)
            {
                _mainWindowManager.MainWindowOpened -= HandleMainWindowOpened;
                _mainWindowManager.MainWindowClosed -= HandleMainWindowClosed;
            }

            _mainWindowManager = mainWindowProvider;
            _mainWindowManager.MainWindowOpened += HandleMainWindowOpened;
            _mainWindowManager.MainWindowClosed += HandleMainWindowClosed;

            // Sync up with the new manager's state
            SyncWithManager();
        }

        public void OpenMainWindow()
        {
            _mainWindowManager?.OpenMainWindow();
        }

        public void CloseMainWindow()
        {
            _mainWindowManager?.CloseMainWindow();
        }

        public event EventHandler? MainWindowOpened;
        public event EventHandler? MainWindowClosed;
    }
}