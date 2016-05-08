using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Artemis.Events;
using Artemis.Models;
using Artemis.Services;
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

        public MainManager(IEventAggregator events, ILogger logger, LoopManager loopManager,
            KeyboardManager keyboardManager, EffectManager effectManager)
        {
            Logger = logger;
            LoopManager = loopManager;
            KeyboardManager = keyboardManager;
            EffectManager = effectManager;

            Logger.Info("Intializing MainManager");

            _events = events;

            //DialogService = dialogService;
            KeyboardHook = new KeyboardHook();

            ProcessWorker = new BackgroundWorker {WorkerSupportsCancellation = true};
            ProcessWorker.DoWork += ProcessWorker_DoWork;
            ProcessWorker.RunWorkerCompleted += BackgroundWorkerExceptionCatcher;

            // Process worker will always run (and just do nothing when ProgramEnabled is false)
            ProcessWorker.RunWorkerAsync();

            ProgramEnabled = false;
            Running = false;

            // Create and start the web server
            GameStateWebServer = new GameStateWebServer();
            GameStateWebServer.Start();

            // Start the named pipe
            PipeServer = new PipeServer();
            PipeServer.Start("artemis");

            Logger.Info("Intialized MainManager");
        }

        [Inject]
        public Lazy<ShellViewModel> ShellViewModel { get; set; }

        public ILogger Logger { get; }
        private LoopManager LoopManager { get; }
        public KeyboardManager KeyboardManager { get; set; }
        public EffectManager EffectManager { get; set; }

        public PipeServer PipeServer { get; set; }
        public BackgroundWorker ProcessWorker { get; set; }
        public MetroDialogService DialogService { get; set; }
        public KeyboardHook KeyboardHook { get; set; }
        public GameStateWebServer GameStateWebServer { get; set; }
        public bool ProgramEnabled { get; private set; }
        public bool Suspended { get; set; }
        public bool Running { get; private set; }

        public void Dispose()
        {
            Logger.Debug("Shutting down MainManager");
            Stop();
            ProcessWorker.CancelAsync();
            ProcessWorker.CancelAsync();
            GameStateWebServer.Stop();
            PipeServer.Stop();
        }

        /// <summary>
        ///     Take control of the keyboard and start sending data to it
        /// </summary>
        /// <returns>Whether starting was successful or not</returns>
        public bool Start(EffectModel effect = null)
        {
            Logger.Debug("Starting MainManager");
            // Can't take control when not enabled
            if (!ProgramEnabled)
                return false;

            // Do nothing if already running
            if (Running)
                return true;

            // Only continue if a keyboard was loaded
            KeyboardManager.EnableLastKeyboard();
            if (KeyboardManager.ActiveKeyboard == null)
                return false;

            Running = true;
            if (effect != null)
                EffectManager.ChangeEffect(effect);

            LoopManager.Start();

            return Running;
        }

        /// <summary>
        ///     Releases control of the keyboard and stop sending data to it
        /// </summary>
        public void Stop()
        {
            if (!Running)
                return;

            Logger.Debug("Stopping MainManager");

            LoopManager.Stop();
        }

        /// <summary>
        ///     Loads the last active effect and starts the program
        /// </summary>
        public void EnableProgram()
        {
            Logger.Debug("Enabling program");
            ProgramEnabled = true;
            Start(EffectManager.GetLastEffect());
            _events.PublishOnUIThread(new ToggleEnabled(ProgramEnabled));
        }

        /// <summary>
        ///     Stops the program
        /// </summary>
        public void DisableProgram()
        {
            Logger.Debug("Disabling program");
            Stop();
            ProgramEnabled = false;
            _events.PublishOnUIThread(new ToggleEnabled(ProgramEnabled));
        }

        private void ProcessWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!ProcessWorker.CancellationPending)
            {
                if (!ProgramEnabled)
                {
                    Thread.Sleep(1000);
                    continue;
                }

                var runningProcesses = Process.GetProcesses();

                // If the currently active effect is a disabled game, get rid of it.
                if (EffectManager.ActiveEffect != null)
                    EffectManager.DisableInactiveGame();

                // If the currently active effect is a no longer running game, get rid of it.
                var activeGame = EffectManager.ActiveEffect as GameModel;
                if (activeGame != null)
                    if (!runningProcesses.Any(p => p.ProcessName == activeGame.ProcessName && p.HasExited == false))
                        EffectManager.DisableGame(activeGame);

                // Look for running games, stopping on the first one that's found.
                var newGame = EffectManager.EnabledGames
                    .FirstOrDefault(
                        g => runningProcesses.Any(p => p.ProcessName == g.ProcessName && p.HasExited == false));

                // If it's not already enabled, do so.
                if (newGame != null && EffectManager.ActiveEffect != newGame)
                    EffectManager.ChangeEffect(newGame);

                Thread.Sleep(1000);
            }
        }

        private void BackgroundWorkerExceptionCatcher(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error == null)
                return;

            Logger.Error(e.Error, "Exception in the BackgroundWorker");
            throw e.Error;
        }
    }
}