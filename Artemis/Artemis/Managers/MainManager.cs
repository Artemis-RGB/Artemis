using System;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using Artemis.Events;
using Artemis.Models;
using Artemis.Modules.Effects.ProfilePreview;
using Artemis.Utilities.GameState;
using Artemis.Utilities.Keyboard;
using Artemis.Utilities.LogitechDll;
using Artemis.ViewModels;
using Caliburn.Micro;
using Ninject;
using Ninject.Extensions.Logging;

namespace Artemis.Managers
{
    /// <summary>
    ///     Contains all the other managers and non-loop related components
    /// </summary>
    public class MainManager : IDisposable
    {
        public delegate void PauseCallbackHandler();

        private readonly IEventAggregator _events;
        private readonly ILogger _logger;
        private readonly Timer _processTimer;

        public MainManager(IEventAggregator events, ILogger logger, LoopManager loopManager,
            DeviceManager deviceManager, EffectManager effectManager, ProfileManager profileManager)
        {
            _logger = logger;
            LoopManager = loopManager;
            DeviceManager = deviceManager;
            EffectManager = effectManager;
            ProfileManager = profileManager;

            _logger.Info("Intializing MainManager");

            _events = events;

            _processTimer = new Timer(1000);
            _processTimer.Elapsed += ScanProcesses;
            _processTimer.Start();

            ProgramEnabled = false;
            Running = false;

            // TODO: Dependency inject utilities?
            KeyboardHook = new KeyboardHook();

            // Create and start the web server
            GameStateWebServer = new GameStateWebServer();
            GameStateWebServer.Start();

            // Start the named pipe
            PipeServer = new PipeServer();
            PipeServer.Start("artemis");

            _logger.Info("Intialized MainManager");
        }

        [Inject]
        public Lazy<ShellViewModel> ShellViewModel { get; set; }

        public LoopManager LoopManager { get; }
        public DeviceManager DeviceManager { get; set; }
        public EffectManager EffectManager { get; set; }
        public ProfileManager ProfileManager { get; set; }

        public PipeServer PipeServer { get; set; }
        public KeyboardHook KeyboardHook { get; set; }
        public GameStateWebServer GameStateWebServer { get; set; }
        public bool ProgramEnabled { get; private set; }
        public bool Running { get; private set; }

        public void Dispose()
        {
            _logger.Debug("Shutting down MainManager");

            _processTimer.Stop();
            _processTimer.Dispose();
            LoopManager.Stop();
            EffectManager.ActiveEffect.Dispose();
            GameStateWebServer.Stop();
            PipeServer.Stop();
        }

        /// <summary>
        ///     Loads the last active effect and starts the program
        /// </summary>
        public void EnableProgram()
        {
            _logger.Debug("Enabling program");
            ProgramEnabled = true;
            LoopManager.StartAsync();
            _events.PublishOnUIThread(new ToggleEnabled(ProgramEnabled));
        }

        /// <summary>
        ///     Stops the program
        /// </summary>
        public void DisableProgram()
        {
            _logger.Debug("Disabling program");
            LoopManager.Stop();
            ProgramEnabled = false;
            _events.PublishOnUIThread(new ToggleEnabled(ProgramEnabled));
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

            if (EffectManager.ActiveEffect is ProfilePreviewModel)
                return;

            // If the currently active effect is a no longer running game, get rid of it.
            var activeGame = EffectManager.ActiveEffect as GameModel;
            if (activeGame != null)
            {
                if (!runningProcesses.Any(p => p.ProcessName == activeGame.ProcessName && p.HasExited == false))
                {
                    _logger.Info("Disabling game: {0}", activeGame.Name);
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
            _logger.Info("Detected and enabling game: {0}", newGame.Name);
            EffectManager.ChangeEffect(newGame, LoopManager);
        }
    }
}