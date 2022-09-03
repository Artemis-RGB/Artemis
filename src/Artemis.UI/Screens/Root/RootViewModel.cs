using System;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.Sidebar;
using Artemis.UI.Services.Interfaces;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.MainWindow;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using Avalonia.Threading;
using ReactiveUI;

namespace Artemis.UI.Screens.Root;

public class RootViewModel : ActivatableViewModelBase, IScreen, IMainWindowProvider
{
    private readonly IAssetLoader _assetLoader;
    private readonly ICoreService _coreService;
    private readonly IDebugService _debugService;
    private readonly DefaultTitleBarViewModel _defaultTitleBarViewModel;
    private readonly IClassicDesktopStyleApplicationLifetime _lifeTime;
    private readonly ISettingsService _settingsService;
    private readonly ISidebarVmFactory _sidebarVmFactory;
    private readonly IUpdateService _updateService;
    private readonly IWindowService _windowService;
    private SidebarViewModel? _sidebarViewModel;
    private ViewModelBase? _titleBarViewModel;

    public RootViewModel(ICoreService coreService,
        ISettingsService settingsService,
        IRegistrationService registrationService,
        IWindowService windowService,
        IMainWindowService mainWindowService,
        IDebugService debugService,
        IUpdateService updateService,
        IAssetLoader assetLoader,
        DefaultTitleBarViewModel defaultTitleBarViewModel,
        ISidebarVmFactory sidebarVmFactory)
    {
        Router = new RoutingState();

        _coreService = coreService;
        _settingsService = settingsService;
        _windowService = windowService;
        _debugService = debugService;
        _updateService = updateService;
        _assetLoader = assetLoader;
        _defaultTitleBarViewModel = defaultTitleBarViewModel;
        _sidebarVmFactory = sidebarVmFactory;
        _lifeTime = (IClassicDesktopStyleApplicationLifetime) Application.Current!.ApplicationLifetime!;

        mainWindowService.ConfigureMainWindowProvider(this);

        DisplayAccordingToSettings();
        Router.CurrentViewModel.Subscribe(UpdateTitleBarViewModel);
        Task.Run(() =>
        {
            coreService.Initialize();
            registrationService.RegisterBuiltInDataModelDisplays();
            registrationService.RegisterBuiltInDataModelInputs();
            registrationService.RegisterBuiltInPropertyEditors();
        });
    }

    public SidebarViewModel? SidebarViewModel
    {
        get => _sidebarViewModel;
        set => RaiseAndSetIfChanged(ref _sidebarViewModel, value);
    }

    public ViewModelBase? TitleBarViewModel
    {
        get => _titleBarViewModel;
        set => RaiseAndSetIfChanged(ref _titleBarViewModel, value);
    }

    private void UpdateTitleBarViewModel(IRoutableViewModel? viewModel)
    {
        if (viewModel is MainScreenViewModel mainScreenViewModel && mainScreenViewModel.TitleBarViewModel != null)
            TitleBarViewModel = mainScreenViewModel.TitleBarViewModel;
        else
            TitleBarViewModel = _defaultTitleBarViewModel;
    }

    private void CurrentMainWindowOnClosing(object? sender, EventArgs e)
    {
        _lifeTime.MainWindow = null;
        SidebarViewModel = null;
        Router.NavigateAndReset.Execute(new EmptyViewModel(this, "blank")).Subscribe();
        OnMainWindowClosed();
    }

    private void DisplayAccordingToSettings()
    {
        bool autoRunning = Constants.StartupArguments.Contains("--autorun");
        bool minimized = Constants.StartupArguments.Contains("--minimized");
        bool showOnAutoRun = _settingsService.GetSetting("UI.ShowOnStartup", true).Value;

        if ((autoRunning && !showOnAutoRun) || minimized)
        {
            // TODO: Auto-update
        }
        else
        {
            ShowSplashScreen();
            _coreService.Initialized += (_, _) => Dispatcher.UIThread.InvokeAsync(OpenMainWindow);
        }
    }

    private void ShowSplashScreen()
    {
        _windowService.ShowWindow<SplashViewModel>();
    }

    /// <inheritdoc />
    public RoutingState Router { get; }

    #region Tray commands

    public void OpenScreen(string displayName)
    {
        // The window will open on the UI thread at some point, respond to that to select the chosen screen
        MainWindowOpened += OnEventHandler;
        OpenMainWindow();

        void OnEventHandler(object? sender, EventArgs args)
        {
            if (SidebarViewModel != null)
                SidebarViewModel.SelectedSidebarScreen = SidebarViewModel.SidebarScreens.FirstOrDefault(s => s.DisplayName == displayName);
            MainWindowOpened -= OnEventHandler;
        }
    }

    public async Task OpenDebugger()
    {
        await Dispatcher.UIThread.InvokeAsync(() => _debugService.ShowDebugger());
    }

    public async Task Exit()
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
            SidebarViewModel = _sidebarVmFactory.SidebarViewModel(this);
            _lifeTime.MainWindow = new MainWindow {DataContext = this};
            _lifeTime.MainWindow.Show();
            _lifeTime.MainWindow.Closing += CurrentMainWindowOnClosing;
        }

        _lifeTime.MainWindow.WindowState = WindowState.Normal;
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

internal class EmptyViewModel : MainScreenViewModel
{
    /// <inheritdoc />
    public EmptyViewModel(IScreen hostScreen, string urlPathSegment) : base(hostScreen, urlPathSegment)
    {
    }
}