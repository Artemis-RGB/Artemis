using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Models;
using Artemis.UI.Screens.Settings;
using Artemis.UI.Screens.Sidebar;
using Artemis.UI.Services.Interfaces;
using Artemis.UI.Services.Updating;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.MainWindow;
using Artemis.WebClient.Workshop.Services;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using Avalonia.Threading;
using ReactiveUI;
using Serilog;

namespace Artemis.UI.Screens.Root;

public class RootViewModel : RoutableHostScreen<RoutableScreen>, IMainWindowProvider
{
    private readonly ICoreService _coreService;
    private readonly IDebugService _debugService;
    private readonly DefaultTitleBarViewModel _defaultTitleBarViewModel;
    private readonly IClassicDesktopStyleApplicationLifetime _lifeTime;
    private readonly ILogger _logger;
    private readonly IRouter _router;
    private readonly ISettingsService _settingsService;
    private readonly IUpdateService _updateService;
    private readonly IWindowService _windowService;
    private readonly ObservableAsPropertyHelper<ViewModelBase?> _titleBarViewModel;
    private readonly PluginSetting<TrayIconEnum> trayIconSetting;

    public RootViewModel(ILogger logger,
        IRouter router,
        ICoreService coreService,
        ISettingsService settingsService,
        IRegistrationService registrationService,
        IWindowService windowService,
        IMainWindowService mainWindowService,
        IDebugService debugService,
        IUpdateService updateService,
        IWorkshopService workshopService,
        SidebarViewModel sidebarViewModel,
        DefaultTitleBarViewModel defaultTitleBarViewModel)
    {
        Shared.UI.SetMicaEnabled(settingsService.GetSetting("UI.EnableMica", true).Value);
        trayIconSetting = settingsService.GetSetting("UI.TrayIcon", TrayIconEnum.Default);
        WindowSizeSetting = settingsService.GetSetting<WindowSize?>("WindowSize");
        SidebarViewModel = sidebarViewModel;

        _logger = logger;
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
        
        OpenScreen = ReactiveCommand.Create<string?>(ExecuteOpenScreen);
        OpenDebugger = ReactiveCommand.CreateFromTask(ExecuteOpenDebugger);
        Exit = ReactiveCommand.CreateFromTask(ExecuteExit);

        _titleBarViewModel = this.WhenAnyValue(vm => vm.Screen)
            .Select(s => s as IMainScreenViewModel)
            .Select(s => s?.WhenAnyValue(svm => svm.TitleBarViewModel) ?? Observable.Never<ViewModelBase>())
            .Switch()
            .Select(vm => vm ?? _defaultTitleBarViewModel)
            .ToProperty(this, vm => vm.TitleBarViewModel);

        if (ShouldShowUI())
        {
            ShowSplashScreen();
            _coreService.Initialized += (_, _) => Dispatcher.UIThread.InvokeAsync(OpenMainWindow);
        }

        trayIconSetting.SettingChanged += (_, _) => this.RaisePropertyChanged(nameof(TrayIcon));

        Task.Run(() =>
        {
            try
            {
                // Before doing heavy lifting, initialize the update service which may prompt a restart
                // Only initialize with an update check if we're not going to show the UI
                if (_updateService.Initialize(!ShouldShowUI()))
                    return;

                // Workshop service goes first so it has a chance to clean up old workshop entries and introduce new ones
                workshopService.Initialize();
                // Core is initialized now that everything is ready to go
                coreService.Initialize();

                registrationService.RegisterBuiltInDataModelDisplays();
                registrationService.RegisterBuiltInDataModelInputs();
                registrationService.RegisterBuiltInPropertyEditors();
            }
            catch (Exception e)
            {
                _logger.Fatal(e, "Error during initialization");
                _windowService.ShowExceptionDialog("Fatal error occured during initialization", e);
            }
        });
        
        // This isn't pretty, but it's the most straightforward way to make sure the ArtemisLinkCommand has access to the router
        ArtemisLinkCommand.Router = router;
    }

    public SidebarViewModel SidebarViewModel { get; }
    public ReactiveCommand<string?, Unit> OpenScreen { get; }
    public ReactiveCommand<Unit, Unit> OpenDebugger { get; }
    public ReactiveCommand<Unit, Unit> Exit { get; }

    public ViewModelBase? TitleBarViewModel => _titleBarViewModel.Value;
    public static PluginSetting<WindowSize?>? WindowSizeSetting { get; private set; }

    public void GoBack()
    {
        _router.GoBack();
    }

    public void GoForward()
    {
        _router.GoForward();
    }

    private void CurrentMainWindowOnClosing(object? sender, EventArgs e)
    {
        WindowSizeSetting?.Save();
        _lifeTime.MainWindow = null;
        OnMainWindowClosed();
    }

    private bool ShouldShowUI()
    {
        bool autoRunning = Constants.StartupArguments.Contains("--autorun");
        bool minimized = Constants.StartupArguments.Contains("--minimized");
        bool showOnAutoRun = _settingsService.GetSetting("UI.ShowOnStartup", true).Value;

        if (autoRunning)
            return showOnAutoRun;
        return !minimized;
    }

    private void ShowSplashScreen()
    {
        _windowService.ShowWindow(out SplashViewModel _);
    }

    #region Tray commands

    private void ExecuteOpenScreen(string? path)
    {
        if (path != null)
            _router.ClearPreviousWindowRoute();

        // The window will open on the UI thread at some point, respond to that to select the chosen screen
        MainWindowOpened += OnEventHandler;
        OpenMainWindow();

        void OnEventHandler(object? sender, EventArgs args)
        {
            MainWindowOpened -= OnEventHandler;
            // Avoid threading issues by running this on the UI thread 
            if (path != null)
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

    public WindowIcon TrayIcon
    {
        get
        {
            string uri = "avares://Artemis.UI/Assets/Images/Logo/application.ico";
            switch (trayIconSetting.Value)
            {
                case TrayIconEnum.Default:
                    break;
                case TrayIconEnum.Monochrome:
                    uri = "avares://Artemis.UI/Assets/Images/Logo/application-monochrome.ico";
                    break;
                case TrayIconEnum.MonochromeDark:
                    uri = "avares://Artemis.UI/Assets/Images/Logo/application-monochrome-dark.ico";
                    break;
                default:
                    _logger.Error("{icon} is not a valid Icon, fall-backing to default icon", trayIconSetting.Value);
                    break;
            }

            return new WindowIcon(AssetLoader.Open(new Uri(uri)));
        }
    }

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
        if (_lifeTime.MainWindow.WindowState == WindowState.Minimized)
            _lifeTime.MainWindow.WindowState = WindowState.Normal;

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