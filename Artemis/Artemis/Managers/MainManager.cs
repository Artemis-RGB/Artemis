using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;
using Artemis.Events;
using Artemis.Utilities;
using Artemis.Utilities.DataReaders;
using Artemis.Utilities.GameState;
using Artemis.ViewModels;
using Microsoft.Win32;
using Ninject;
using Ninject.Extensions.Logging;

namespace Artemis.Managers
{
    /// <summary>
    ///     Contains all the other managers and non-loop related components
    /// </summary>
    public class MainManager : IDisposable
    {
        private readonly Timer _processTimer;

        public MainManager(ILogger logger, LoopManager loopManager, DeviceManager deviceManager,
            ModuleManager moduleManager, PreviewManager previewManager, PipeServer pipeServer,
            GameStateWebServer gameStateWebServer)
        {
            Logger = logger;
            LoopManager = loopManager;
            DeviceManager = deviceManager;
            ModuleManager = moduleManager;
            PreviewManager = previewManager;
            PipeServer = pipeServer;

            _processTimer = new Timer(1000);
            _processTimer.Elapsed += ScanProcesses;
            _processTimer.Start();

            ProgramEnabled = false;
            Running = false;

            // Create and start the web server
            GameStateWebServer = gameStateWebServer;
            GameStateWebServer.Start();

            // Start the named pipe
            PipeServer.Start("artemis");

            // Start the update task
            var updateTask = new Task(Updater.UpdateApp);
            updateTask.Start();

            // Listen for power mode changes
            SystemEvents.PowerModeChanged += OnPowerChange;

            Logger.Info("Intialized MainManager");
            Logger.Info($"Artemis version {Assembly.GetExecutingAssembly().GetName().Version} is ready!");
        }

        [Inject]
        public Lazy<ShellViewModel> ShellViewModel { get; set; }

        public ILogger Logger { get; set; }
        public LoopManager LoopManager { get; }
        public DeviceManager DeviceManager { get; set; }
        public ModuleManager ModuleManager { get; set; }
        public PreviewManager PreviewManager { get; set; }

        public PipeServer PipeServer { get; set; }
        public GameStateWebServer GameStateWebServer { get; set; }
        public bool ProgramEnabled { get; private set; }
        public bool Running { get; private set; }

        public void Dispose()
        {
            Logger.Debug("Shutting down MainManager");

            _processTimer?.Stop();
            _processTimer?.Dispose();
            LoopManager?.Stop();
            ModuleManager?.ActiveModule?.Dispose();
            GameStateWebServer?.Stop();
            PipeServer?.Stop();
        }

        public event EventHandler<EnabledChangedEventArgs> EnabledChanged;

        /// <summary>
        ///     Restarts the loop manager when the system resumes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnPowerChange(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode != PowerModes.Resume)
                return;

            Logger.Debug("Restarting for OnPowerChange");
            DisableProgram();
            // Wait an extra while for device providers to be fully ready
            await Task.Delay(2000);
            EnableProgram();
        }

        /// <summary>
        ///     Loads the last active effect and starts the program
        /// </summary>
        public void EnableProgram()
        {
            Logger.Debug("Enabling program");
            ProgramEnabled = true;
            LoopManager.StartAsync();
            foreach (var overlayModule in ModuleManager.OverlayModules)
            {
                if (overlayModule.Settings.IsEnabled)
                    overlayModule.Enable();
            }
            RaiseEnabledChangedEvent(new EnabledChangedEventArgs(ProgramEnabled));
        }

        /// <summary>
        ///     Stops the program
        /// </summary>
        public void DisableProgram()
        {
            Logger.Debug("Disabling program");
            foreach (var overlayModule in ModuleManager.OverlayModules)
            {
                if (overlayModule.Settings.IsEnabled)
                    overlayModule.Dispose();
            }
            LoopManager.Stop();
            ProgramEnabled = false;
            RaiseEnabledChangedEvent(new EnabledChangedEventArgs(ProgramEnabled));
        }

        /// <summary>
        ///     Manages active process bound modules by keeping an eye on their processes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScanProcesses(object sender, ElapsedEventArgs e)
        {
            if (!ProgramEnabled)
                return;

            var processes = System.Diagnostics.Process.GetProcesses();
            var module = ModuleManager.ActiveModule;

            // If the current active module is in preview-mode, leave it alone
            if (module?.PreviewLayers != null)
                return;

            // If the active module is a process bound module, make sure it should still be enabled
            if (module != null && module.IsBoundToProcess)
            {
                if (!module.Settings.IsEnabled)
                    ModuleManager.DisableProcessBoundModule();

                // If the currently active effect is a no longer running game, get rid of it.
                if (!processes.Any(p => p.ProcessName == module.ProcessName && p.HasExited == false))
                {
                    Logger.Info("Disabling process bound module because process stopped: {0}", module.Name);
                    ModuleManager.DisableProcessBoundModule();
                }
            }

            // Look for running games, stopping on the first one that's found.
            var newModule = ModuleManager.ProcessModules.Where(g => g.Settings.IsEnabled && g.Settings.IsEnabled)
                .FirstOrDefault(g => processes.Any(p => p.ProcessName == g.ProcessName && p.HasExited == false));

            if (newModule == null || module == newModule)
                return;

            // If it's not already enabled, do so.
            Logger.Info("Detected and enabling process bound module: {0}", newModule.Name);
            ModuleManager.ChangeActiveModule(newModule, LoopManager);
        }

        protected virtual void RaiseEnabledChangedEvent(EnabledChangedEventArgs e)
        {
            var handler = EnabledChanged;
            handler?.Invoke(this, e);
        }
    }
}