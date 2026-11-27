using Artemis.UI.Shared.Services.MainWindow;

namespace Artemis.UI.Shared.Tests.Infrastructure.Services;

/// <summary>
///     A main window service without a window, <see cref="RaiseMainWindowClosed" /> pretends the window was closed.
/// </summary>
public class TestMainWindowService : IMainWindowService
{
    public bool IsMainWindowOpen => true;
    public bool IsMainWindowFocused => true;

    public void RaiseMainWindowClosed()
    {
        MainWindowClosed?.Invoke(this, EventArgs.Empty);
    }

    public void ConfigureMainWindowProvider(IMainWindowProvider mainWindowProvider) => throw new NotSupportedException();
    public void OpenMainWindow() => throw new NotSupportedException();
    public void CloseMainWindow() => throw new NotSupportedException();

    public event EventHandler? MainWindowClosed;

#pragma warning disable CS0067 // Events are part of the interface but never raised here
    public event EventHandler? MainWindowOpened;
    public event EventHandler? MainWindowFocused;
    public event EventHandler? MainWindowUnfocused;
#pragma warning restore CS0067
}
