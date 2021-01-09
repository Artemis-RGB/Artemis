using System;

namespace Artemis.UI.Shared.Services
{
    internal class WindowService : IWindowService
    {
        private IMainWindowManager? _mainWindowManager;

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
        
        public void ConfigureMainWindowManager(IMainWindowManager mainWindowManager)
        {
            if (mainWindowManager == null) throw new ArgumentNullException(nameof(mainWindowManager));

            if (_mainWindowManager != null)
            {
                _mainWindowManager.MainWindowOpened -= HandleMainWindowOpened;
                _mainWindowManager.MainWindowClosed -= HandleMainWindowClosed;
            }

            _mainWindowManager = mainWindowManager;
            _mainWindowManager.MainWindowOpened += HandleMainWindowOpened;
            _mainWindowManager.MainWindowClosed += HandleMainWindowClosed;

            // Sync up with the new manager's state
            SyncWithManager();
        }

        public void OpenMainWindow()
        {
            IsMainWindowOpen = true;
            OnMainWindowOpened();
        }

        public void CloseMainWindow()
        {
            IsMainWindowOpen = false;
            OnMainWindowClosed();
        }

        public event EventHandler? MainWindowOpened;
        public event EventHandler? MainWindowClosed;
    }
}