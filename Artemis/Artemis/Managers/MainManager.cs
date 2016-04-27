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
using Caliburn.Micro;
using NLog;
using LogManager = NLog.LogManager;

namespace Artemis.Managers
{
    public class MainManager
    {
        public delegate void PauseCallbackHandler();

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly int _fps;
        private bool _paused;
        private bool _restarting;

        public MainManager(IEventAggregator events, MetroDialogService dialogService)
        {
            Logger.Info("Intializing MainManager");

            Events = events;
            DialogService = dialogService;

            KeyboardManager = new KeyboardManager(this, Events);
            EffectManager = new EffectManager(this, Events);
            KeyboardHook = new KeyboardHook();

            _fps = 25;
            UpdateWorker = new BackgroundWorker {WorkerSupportsCancellation = true};
            ProcessWorker = new BackgroundWorker {WorkerSupportsCancellation = true};

            UpdateWorker.DoWork += UpdateWorker_DoWork;
            UpdateWorker.RunWorkerCompleted += BackgroundWorkerExceptionCatcher;

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

        public PipeServer PipeServer { get; set; }
        public BackgroundWorker UpdateWorker { get; set; }
        public BackgroundWorker ProcessWorker { get; set; }

        public KeyboardManager KeyboardManager { get; set; }
        public EffectManager EffectManager { get; set; }

        public KeyboardHook KeyboardHook { get; set; }

        public GameStateWebServer GameStateWebServer { get; set; }
        public IEventAggregator Events { get; set; }
        public MetroDialogService DialogService { get; set; }

        public bool ProgramEnabled { get; private set; }
        public bool Suspended { get; set; }

        public bool Running { get; private set; }

        public event PauseCallbackHandler PauseCallback;

        /// <summary>
        ///     Take control of the keyboard and start sending data to it
        /// </summary>
        /// <returns>Whether starting was successful or not</returns>
        public bool Start(EffectModel effect = null)
        {
            Logger.Debug("Starting MainManager");
            // Can't take control when not enabled
            if (!ProgramEnabled || UpdateWorker.CancellationPending || UpdateWorker.IsBusy || _paused)
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

            // Start the update worker
            if (!UpdateWorker.IsBusy)
                UpdateWorker.RunWorkerAsync();

            return Running;
        }

        /// <summary>
        ///     Releases control of the keyboard and stop sending data to it
        /// </summary>
        public void Stop()
        {
            Logger.Debug("Stopping MainManager");
            if (!Running || UpdateWorker.CancellationPending || _paused)
                return;

            // Stop the update worker
            UpdateWorker.CancelAsync();
            UpdateWorker.RunWorkerCompleted += FinishStop;
        }

        private void FinishStop(object sender, RunWorkerCompletedEventArgs e)
        {
            UpdateWorker.RunWorkerCompleted -= FinishStop;
            KeyboardManager.ReleaseActiveKeyboard();
            Running = false;

            Logger.Debug("Stopped MainManager");
            if (e.Error != null || !_restarting)
                return;

            Start();
            _restarting = false;
        }

        public void Pause()
        {
            if (!Running || UpdateWorker.CancellationPending || _paused)
                return;

            Logger.Debug("Pausing MainManager");
            _paused = true;
        }

        public void Unpause()
        {
            if (!_paused)
                return;

            Logger.Debug("Unpausing MainManager");
            _paused = false;
        }

        public void Shutdown()
        {
            Logger.Debug("Shutting down MainManager");
            Stop();
            ProcessWorker.CancelAsync();
            ProcessWorker.CancelAsync();
            GameStateWebServer.Stop();
            PipeServer.Stop();
        }

        public void Restart()
        {
            if (_restarting)
                return;

            Logger.Debug("Restarting MainManager");
            if (!Running)
            {
                Start();
                return;
            }

            _restarting = true;
            Stop();
        }

        /// <summary>
        ///     Loads the last active effect and starts the program
        /// </summary>
        public void EnableProgram()
        {
            Logger.Debug("Enabling program");
            ProgramEnabled = true;
            Start(EffectManager.GetLastEffect());
            Events.PublishOnUIThread(new ToggleEnabled(ProgramEnabled));
        }

        /// <summary>
        ///     Stops the program
        /// </summary>
        public void DisableProgram()
        {
            Logger.Debug("Disabling program");
            Stop();
            ProgramEnabled = false;
            Events.PublishOnUIThread(new ToggleEnabled(ProgramEnabled));
        }

        #region Workers

        private void UpdateWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var sw = new Stopwatch();
            while (!UpdateWorker.CancellationPending)
            {
                // Skip frame when paused
                if (_paused)
                {
                    PauseCallback?.Invoke();
                    Thread.Sleep(1000/_fps);
                    continue;
                }

                // Stop if no keyboard/effect are present
                if (KeyboardManager.ActiveKeyboard == null || EffectManager.ActiveEffect == null)
                {
                    Thread.Sleep(1000/_fps);
                    Logger.Debug("No active effect/keyboard, stopping");

                    if (EffectManager.PauseEffect != null)
                    {
                        PauseCallback?.Invoke();
                        Thread.Sleep(1000/_fps);
                    }
                    else
                        Stop();
                    continue;
                }

                // Don't stop when the effect is still initialized, just skip this frame
                if (!EffectManager.ActiveEffect.Initialized)
                {
                    Thread.Sleep(1000/_fps);
                    continue;
                }

                sw.Start();

                // Update the current effect
                if (EffectManager.ActiveEffect.Initialized)
                    EffectManager.ActiveEffect.Update();

                // Get ActiveEffect's bitmap
                var bitmap = EffectManager.ActiveEffect.Initialized
                    ? EffectManager.ActiveEffect.GenerateBitmap()
                    : null;

                // Draw enabled overlays on top
                foreach (var overlayModel in EffectManager.EnabledOverlays)
                {
                    overlayModel.Update();
                    bitmap = bitmap != null ? overlayModel.GenerateBitmap(bitmap) : overlayModel.GenerateBitmap();
                }

                // If it exists, send bitmap to the device
                if (bitmap != null && KeyboardManager.ActiveKeyboard != null)
                {
                    KeyboardManager.ActiveKeyboard.DrawBitmap(bitmap);

                    // debugging TODO: Disable when window isn't shown
                    Events.PublishOnUIThread(new ChangeBitmap(bitmap));
                }

                // Sleep according to time left this frame
                var sleep = (int) (1000/_fps - sw.ElapsedMilliseconds);
                if (sleep > 0)
                    Thread.Sleep(sleep);
                sw.Reset();
            }
        }

        private void BackgroundWorkerExceptionCatcher(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error == null)
                return;

            Logger.Error(e.Error, "Exception in the BackgroundWorker");
            throw e.Error;
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

        #endregion
    }
}