using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Artemis.Core.Services;
using Artemis.UI.Events;
using Artemis.UI.Screens.Splash;
using Artemis.UI.Services;
using Artemis.UI.Shared.Services;
using Hardcodet.Wpf.TaskbarNotification;
using MaterialDesignThemes.Wpf;
using Ninject;
using Stylet;
using Icon = System.Drawing.Icon;

namespace Artemis.UI.Screens
{
    public class TrayViewModel : Screen, IMainWindowProvider, INotificationProvider
    {
        private readonly IDebugService _debugService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IKernel _kernel;
        private readonly IWindowManager _windowManager;
        private RootViewModel _rootViewModel;
        private SplashViewModel _splashViewModel;
        private TaskbarIcon _taskBarIcon;

        public TrayViewModel(IKernel kernel,
            IWindowManager windowManager,
            IWindowService windowService,
            IMessageService messageService,
            IUpdateService updateService,
            IEventAggregator eventAggregator,
            ICoreService coreService,
            IDebugService debugService,
            ISettingsService settingsService)
        {
            _kernel = kernel;
            _windowManager = windowManager;
            _eventAggregator = eventAggregator;
            _debugService = debugService;

            Core.Utilities.ShutdownRequested += UtilitiesOnShutdownRequested;
            Core.Utilities.RestartRequested += UtilitiesOnShutdownRequested;

            windowService.ConfigureMainWindowProvider(this);
            messageService.ConfigureNotificationProvider(this);
            bool autoRunning = Bootstrapper.StartupArguments.Contains("--autorun");
            bool showOnAutoRun = settingsService.GetSetting("UI.ShowOnStartup", true).Value;
            if (!autoRunning || showOnAutoRun)
            {
                ShowSplashScreen();
                coreService.Initialized += (_, _) => TrayBringToForeground();
            }
            else
            {
                coreService.Initialized += (_, _) => updateService.AutoUpdate();
            }
        }

        public void TrayBringToForeground()
        {
            if (IsMainWindowOpen)
            {
                Execute.PostToUIThread(FocusMainWindow);
                return;
            }

            // Initialize the shared UI when first showing the window
            if (!UI.Shared.Bootstrapper.Initialized)
                UI.Shared.Bootstrapper.Initialize(_kernel);

            Execute.OnUIThreadSync(() =>
            {
                _splashViewModel?.RequestClose();
                _splashViewModel = null;
                _rootViewModel = _kernel.Get<RootViewModel>();
                _rootViewModel.Closed += RootViewModelOnClosed;
                _windowManager.ShowWindow(_rootViewModel);
            });

            OnMainWindowOpened();
        }

        public void TrayActivateSidebarItem(string sidebarItem)
        {
            TrayBringToForeground();
            _eventAggregator.Publish(new RequestSelectSidebarItemEvent(sidebarItem));
        }

        public void TrayExit()
        {
            Core.Utilities.Shutdown();
        }

        public void TrayOpenDebugger()
        {
            _debugService.ShowDebugger();
        }

        public void SetTaskbarIcon(UIElement view)
        {
            _taskBarIcon = (TaskbarIcon) ((ContentControl) view).Content;
        }

        public void OnTrayBalloonTipClicked(object sender, EventArgs e)
        {
            if (!IsMainWindowOpen)
                TrayBringToForeground();
            else
                FocusMainWindow();
        }

        private void FocusMainWindow()
        {
            // Wrestle the main window to the front
            Window mainWindow = (Window) _rootViewModel.View;
            if (mainWindow.WindowState == WindowState.Minimized)
                mainWindow.WindowState = WindowState.Normal;
            mainWindow.Activate();
            mainWindow.Topmost = true;
            mainWindow.Topmost = false;
            mainWindow.Focus();
        }

        private void UtilitiesOnShutdownRequested(object sender, EventArgs e)
        {
            Execute.OnUIThread(() => _taskBarIcon?.Dispose());
        }

        private void ShowSplashScreen()
        {
            Execute.OnUIThread(() =>
            {
                _splashViewModel = _kernel.Get<SplashViewModel>();
                _windowManager.ShowWindow(_splashViewModel);
            });
        }

        private void RootViewModelOnClosed(object sender, CloseEventArgs e)
        {
            _rootViewModel.Closed -= RootViewModelOnClosed;
            _rootViewModel = null;
            OnMainWindowClosed();
        }

        #region Implementation of INotificationProvider

        /// <inheritdoc />
        public void ShowNotification(string title, string message, PackIconKind icon)
        {
            Execute.OnUIThread(() =>
            {
                // Convert the PackIcon to an icon by drawing it on a visual
                DrawingVisual drawingVisual = new();
                DrawingContext drawingContext = drawingVisual.RenderOpen();

                PackIcon packIcon = new() {Kind = icon};
                Geometry geometry = Geometry.Parse(packIcon.Data);

                // Scale the icon up to fit a 256x256 image and draw it
                geometry = Geometry.Combine(geometry, Geometry.Empty, GeometryCombineMode.Union, new ScaleTransform(256 / geometry.Bounds.Right, 256 / geometry.Bounds.Bottom));
                drawingContext.DrawGeometry(new SolidColorBrush(Colors.White), null, geometry);
                drawingContext.Close();

                // Render the visual and add it to a PNG encoder (we want opacity in our icon)
                RenderTargetBitmap renderTargetBitmap = new(256, 256, 96, 96, PixelFormats.Pbgra32);
                renderTargetBitmap.Render(drawingVisual);
                PngBitmapEncoder encoder = new();
                encoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

                // Save the PNG and get an icon handle
                using MemoryStream stream = new();
                encoder.Save(stream);
                Icon convertedIcon = Icon.FromHandle(new Bitmap(stream).GetHicon());

                // Show the 'balloon'
                _taskBarIcon.ShowBalloonTip(title, message, convertedIcon, true);
            });
        }

        #endregion

        #region Implementation of IMainWindowProvider

        public bool IsMainWindowOpen { get; private set; }

        public bool OpenMainWindow()
        {
            if (IsMainWindowOpen)
                Execute.OnUIThread(FocusMainWindow);
            else
                TrayBringToForeground();
            return _rootViewModel.ScreenState == ScreenState.Active;
        }

        public bool CloseMainWindow()
        {
            Execute.OnUIThread(() => _rootViewModel.RequestClose());
            return _rootViewModel.ScreenState == ScreenState.Closed;
        }

        public event EventHandler MainWindowOpened;

        public event EventHandler MainWindowClosed;

        protected virtual void OnMainWindowOpened()
        {
            IsMainWindowOpen = true;
            MainWindowOpened?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnMainWindowClosed()
        {
            IsMainWindowOpen = false;
            MainWindowClosed?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}