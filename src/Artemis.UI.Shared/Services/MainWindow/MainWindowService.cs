using System;
using ReactiveUI;

namespace Artemis.UI.Shared.Services.MainWindow;

internal class MainWindowService : IMainWindowService
{
    private IMainWindowProvider? _mainWindowManager;

    /// <inheritdoc />
    public bool IsMainWindowOpen { get; private set; }
    
    protected virtual void OnMainWindowOpened()
    {
        MainWindowOpened?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnMainWindowClosed()
    {
        UI.ClearCache();
        MainWindowClosed?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnMainWindowFocused()
    {
        MainWindowFocused?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnMainWindowUnfocused()
    {
        MainWindowUnfocused?.Invoke(this, EventArgs.Empty);
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

    private void HandleMainWindowFocused(object? sender, EventArgs e)
    {
        OnMainWindowFocused();
    }

    private void HandleMainWindowUnfocused(object? sender, EventArgs e)
    {
        OnMainWindowUnfocused();
    }

    public void ConfigureMainWindowProvider(IMainWindowProvider mainWindowProvider)
    {
        if (mainWindowProvider == null) throw new ArgumentNullException(nameof(mainWindowProvider));

        if (_mainWindowManager != null)
        {
            _mainWindowManager.MainWindowOpened -= HandleMainWindowOpened;
            _mainWindowManager.MainWindowClosed -= HandleMainWindowClosed;
            _mainWindowManager.MainWindowFocused -= HandleMainWindowFocused;
            _mainWindowManager.MainWindowUnfocused -= HandleMainWindowUnfocused;
        }

        _mainWindowManager = mainWindowProvider;
        _mainWindowManager.MainWindowOpened += HandleMainWindowOpened;
        _mainWindowManager.MainWindowClosed += HandleMainWindowClosed;
        _mainWindowManager.MainWindowFocused += HandleMainWindowFocused;
        _mainWindowManager.MainWindowUnfocused += HandleMainWindowUnfocused;

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

    public event EventHandler? MainWindowFocused;
    public event EventHandler? MainWindowUnfocused;
}