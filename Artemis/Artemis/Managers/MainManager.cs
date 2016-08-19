using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;
using Artemis.Events;
using Artemis.Models;
using Artemis.Utilities;
using Artemis.Utilities.DataReaders;
using Artemis.Utilities.GameState;
using Artemis.ViewModels;
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
            EffectManager effectManager, ProfileManager profileManager, PipeServer pipeServer)
        {
            Logger = logger;
            LoopManager = loopManager;
            DeviceManager = deviceManager;
            EffectManager = effectManager;
            ProfileManager = profileManager;
            PipeServer = pipeServer;

            _processTimer = new Timer(1000);
            _processTimer.Elapsed += ScanProcesses;
            _processTimer.Start();

            ProgramEnabled = false;
            Running = false;

            // Create and start the web server
            GameStateWebServer = new GameStateWebServer();
            GameStateWebServer.Start();

            // Start the named pipe
            PipeServer.Start("artemis");

            // Start the update task
            var updateTask = new Task(Updater.UpdateApp);
            updateTask.Start();

            Logger.Info("Intialized MainManager");
            Logger.Info($"Artemis version {Assembly.GetExecutingAssembly().GetName().Version} is ready!");
        }

        [Inject]
        public Lazy<ShellViewModel> ShellViewModel { get; set; }

        public ILogger Logger { get; set; }
        public LoopManager LoopManager { get; }
        public DeviceManager DeviceManager { get; set; }
        public EffectManager EffectManager { get; set; }
        public ProfileManager ProfileManager { get; set; }

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
            EffectManager?.ActiveEffect?.Dispose();
            GameStateWebServer?.Stop();
            PipeServer?.Stop();
        }

        public event EventHandler<EnabledChangedEventArgs> OnEnabledChangedEvent;

        /// <summary>
        ///     Loads the last active effect and starts the program
        /// </summary>
        public void EnableProgram()
        {
            Logger.Debug("Enabling program");
            ProgramEnabled = true;
            LoopManager.StartAsync();
            RaiseEnabledChangedEvent(new EnabledChangedEventArgs(ProgramEnabled));
        }

        /// <summary>
        ///     Stops the program
        /// </summary>
        public void DisableProgram()
        {
            Logger.Debug("Disabling program");
            LoopManager.Stop();
            ProgramEnabled = false;
            RaiseEnabledChangedEvent(new EnabledChangedEventArgs(ProgramEnabled));
        }

        /// <summary>
        ///     Manages active games by keeping an eye on their processes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScanProcesses(object sender, ElapsedEventArgs e)
        {
            if (!ProgramEnabled)
                return;

            var runningProcesses = Process.GetProcesses();

            // If the currently active effect is a disabled game, get rid of it.
            if (EffectManager.ActiveEffect != null)
                EffectManager.DisableInactiveGame();

            // If the currently active effect is a no longer running game, get rid of it.
            var activeGame = EffectManager.ActiveEffect as GameModel;
            if (activeGame != null)
            {
                if (!runningProcesses.Any(p => p.ProcessName == activeGame.ProcessName && p.HasExited == false))
                {
                    Logger.Info("Disabling game: {0}", activeGame.Name);
                    EffectManager.DisableGame(activeGame);
                }
            }

            // Look for running games, stopping on the first one that's found.
            var newGame = EffectManager.EnabledGames
                .FirstOrDefault(g => runningProcesses
                    .Any(p => p.ProcessName == g.ProcessName && p.HasExited == false));

            if (newGame == null || EffectManager.ActiveEffect == newGame)
                return;
            // If it's not already enabled, do so.
            Logger.Info("Detected and enabling game: {0}", newGame.Name);
            EffectManager.ChangeEffect(newGame, LoopManager);
        }

        protected virtual void RaiseEnabledChangedEvent(EnabledChangedEventArgs e)
        {
            var handler = OnEnabledChangedEvent;
            handler?.Invoke(this, e);
        }
    }
}