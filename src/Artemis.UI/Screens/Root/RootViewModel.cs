using System;
using System.Reactive;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Models;
using Artemis.UI.Screens.Sidebar;
using Artemis.UI.Services.Interfaces;
using Artemis.UI.Services.Updating;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.MainWindow;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using ReactiveUI;

namespace Artemis.UI.Screens.Root;

public class RootViewModel : RoutableScreen<IMainScreenViewModel>, IMainWindowProvider
{
    private readonly ICoreService _coreService;
    private readonly IDebugService _debugService;
    private readonly DefaultTitleBarViewModel _defaultTitleBarViewModel;
    private readonly IClassicDesktopStyleApplicationLifetime _lifeTime;
    private readonly IRouter _router;
    private readonly ISettingsService _settingsService;
    private readonly IUpdateService _updateService;
    private readonly IWindowService _windowService;
    private ViewModelBase? _titleBarViewModel;

    public RootViewModel(IRouter router,
        ICoreService coreService,
        ISettingsService settingsService,
        IRegistrationService registrationService,
        IWindowService windowService,
        IMainWindowService mainWindowService,
        IDebugService debugService,
        IUpdateService updateService,
        SidebarViewModel sidebarViewModel,
        DefaultTitleBarViewModel defaultTitleBarViewModel)
    {
        Shared.UI.SetMicaEnabled(settingsService.GetSetting("UI.EnableMica", true).Value);
        WindowSizeSetting = settingsService.GetSetting<WindowSize?>("WindowSize");
        SidebarViewModel = sidebarViewModel;

        _router = router;
        _coreService = coreService;
        _settingsService = settingsService;
        _windowService = windowService;
        _debugService = debugService;
        _updateService = updateService;
        _defaultTitleBarViewModel = defaultTitleBarViewModel;
        _lifeTime = (IClassicDesktopStyleApplicationLifetime) Application.Current!.ApplicationLifetime!;

        router.SetRoot(this);
        mainWindowService.ConfigureMainWindowProvider(this);

        DisplayAccordingToSettings();
        OpenScreen = ReactiveCommand.Create<string>(ExecuteOpenScreen);
        OpenDebugger = ReactiveCommand.CreateFromTask(ExecuteOpenDebugger);
        Exit = ReactiveCommand.CreateFromTask(ExecuteExit);
        this.WhenAnyValue(vm => vm.Screen).Subscribe(UpdateTitleBarViewModel);

        Task.Run(() =>
        {
            if (_updateService.Initialize())
                return;

            coreService.Initialize();
            registrationService.RegisterBuiltInDataModelDisplays();
            registrationService.RegisterBuiltInDataModelInputs();
            registrationService.RegisterBuiltInPropertyEditors();
        });
    }

    public SidebarViewModel SidebarViewModel { get; }
    public ReactiveCommand<string, Unit> OpenScreen { get; }
    public ReactiveCommand<Unit, Unit> OpenDebugger { get; }
    public ReactiveCommand<Unit, Unit> Exit { get; }

    public ViewModelBase? TitleBarViewModel
    {
        get => _titleBarViewModel;
        set => RaiseAndSetIfChanged(ref _titleBarViewModel, value);
    }

    public static PluginSetting<WindowSize?>? WindowSizeSetting { get; private set; }

    public void GoBack()
    {
        _router.GoBack();
    }

    public void GoForward()
    {
        _router.GoForward();
    }

    private void UpdateTitleBarViewModel(IMainScreenViewModel? viewModel)
    {
        if (viewModel?.TitleBarViewModel != null)
            TitleBarViewModel = viewModel.TitleBarViewModel;
        else
            TitleBarViewModel = _defaultTitleBarViewModel;
    }

    private void CurrentMainWindowOnClosing(object? sender, EventArgs e)
    {
        WindowSizeSetting?.Save();
        _lifeTime.MainWindow = null;
        OnMainWindowClosed();
    }

    private void DisplayAccordingToSettings()
    {
        bool autoRunning = Constants.StartupArguments.Contains("--autorun");
        bool minimized = Constants.StartupArguments.Contains("--minimized");
        bool showOnAutoRun = _settingsService.GetSetting("UI.ShowOnStartup", true).Value;

        if ((autoRunning && !showOnAutoRun) || minimized)
            return;

        ShowSplashScreen();
        _coreService.Initialized += (_, _) => Dispatcher.UIThread.InvokeAsync(OpenMainWindow);
    }

    private void ShowSplashScreen()
    {
        _windowService.ShowWindow<SplashViewModel>();
    }

    #region Tray commands

    private void ExecuteOpenScreen(string path)
    {
        // The window will open on the UI thread at some point, respond to that to select the chosen screen
        MainWindowOpened += OnEventHandler;
        OpenMainWindow();

        void OnEventHandler(object? sender, EventArgs args)
        {
            MainWindowOpened -= OnEventHandler;
            // Avoid threading issues by running this on the UI thread 
            Dispatcher.UIThread.InvokeAsync(async () => await _router.Navigate(path));
        }
    }

    private async Task ExecuteOpenDebugger()
    {
        await Dispatcher.UIThread.InvokeAsync(() => _debugService.ShowDebugger());
    }

    private async Task ExecuteExit()
    {
        // Don't freeze the UI right after clicking
        await Task.Delay(200);
        Utilities.Shutdown();
    }

    #endregion

    #region Implementation of IMainWindowProvider

    /// <inheritdoc />
    public bool IsMainWindowOpen => _lifeTime.MainWindow != null;

    /// <inheritdoc />
    public bool IsMainWindowFocused { get; private set; }

    /// <inheritdoc />
    public void OpenMainWindow()
    {
        if (_lifeTime.MainWindow == null)
        {
            _lifeTime.MainWindow = new MainWindow {DataContext = this};
            _lifeTime.MainWindow.Show();
            _lifeTime.MainWindow.Closing += CurrentMainWindowOnClosing;
        }

        _lifeTime.MainWindow.Activate();
        OnMainWindowOpened();
    }

    /// <inheritdoc />
    public void CloseMainWindow()
    {
        Dispatcher.UIThread.Post(() => { _lifeTime.MainWindow?.Close(); });
    }

    public void Focused()
    {
        IsMainWindowFocused = true;
        OnMainWindowFocused();
    }

    public void Unfocused()
    {
        IsMainWindowFocused = false;
        OnMainWindowUnfocused();
    }

    /// <inheritdoc />
    public event EventHandler? MainWindowOpened;

    /// <inheritdoc />
    public event EventHandler? MainWindowClosed;

    /// <inheritdoc />
    public event EventHandler? MainWindowFocused;

    /// <inheritdoc />
    public event EventHandler? MainWindowUnfocused;

    protected virtual void OnMainWindowOpened()
    {
        MainWindowOpened?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnMainWindowClosed()
    {
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

    #endregion
}