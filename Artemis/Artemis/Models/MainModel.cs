using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Artemis.Events;
using Artemis.KeyboardProviders;
using Artemis.Settings;
using Artemis.Utilities.GameState;
using Artemis.Utilities.Keyboard;
using Artemis.Utilities.Memory;
using Caliburn.Micro;

namespace Artemis.Models
{
    public class MainModel
    {
        private readonly int _fps;
        private readonly BackgroundWorker _processWorker;
        private readonly BackgroundWorker _updateWorker;

        private EffectModel _activeEffect;
        private bool _wasSuspendedBeforeGame;

        public MainModel(IEventAggregator events)
        {
            EffectModels = new List<EffectModel>();
            KeyboardProviders = ProviderHelper.GetKeyboardProviders();
            GameStateWebServer = new GameStateWebServer();
            KeyboardHook = new KeyboardHook();
            Suspended = false;

            Events = events;

            _fps = 25;
            _updateWorker = new BackgroundWorker {WorkerSupportsCancellation = true};
            _processWorker = new BackgroundWorker {WorkerSupportsCancellation = true};
            _updateWorker.DoWork += UpdateWorker_DoWork;
            _processWorker.DoWork += ProcessWorker_DoWork;
        }

        public KeyboardHook KeyboardHook { get; set; }
        public KeyboardProvider ActiveKeyboard { get; set; }
        public List<EffectModel> EffectModels { get; set; }
        public List<KeyboardProvider> KeyboardProviders { get; set; }
        public GameStateWebServer GameStateWebServer { get; set; }
        public IEventAggregator Events { get; set; }
        public bool Enabled { get; private set; }
        public bool Suspended { get; private set; }

        #region Effect methods

        public void StartEffects()
        {
            if (Enabled)
                return;
            if (_updateWorker.IsBusy || _processWorker.IsBusy)
            {
                Events.PublishOnUIThread(new ToggleEnabled(Enabled));
                return;
            }

            LoadLastKeyboard();
            // If no keyboard was loaded, don't enable effects.
            if (ActiveKeyboard == null)
                return;

            // Start the webserver
            GameStateWebServer.Start();

            // Load last non-game effect and enable
            LoadLastEffect();

            // Start the Background Workers
            _updateWorker.RunWorkerAsync();
            _processWorker.RunWorkerAsync();

            Enabled = true;
            Events.PublishOnUIThread(new ToggleEnabled(Enabled));

            if (General.Default.Suspended && !Suspended)
                ToggleSuspension();
        }

        public void ShutdownEffects()
        {
            if (!Enabled)
                return;

            // Stop the Background Worker
            _updateWorker.CancelAsync();
            _processWorker.CancelAsync();

            // Dispose the current active effect
            _activeEffect?.Dispose();
            _activeEffect = null;

            ActiveKeyboard?.Disable();
            ActiveKeyboard = null;

            Enabled = false;
            Events.PublishOnUIThread(new ToggleEnabled(Enabled));
        }

        private void LoadLastEffect()
        {
            var effect = EffectModels.FirstOrDefault(e => e.Name == General.Default.LastEffect);
            ChangeEffect(effect ?? EffectModels.First(e => e.Name == "TypeWave"));
        }

        private void ChangeEffect(EffectModel effectModel)
        {
            if (effectModel is OverlayModel)
                throw new ArgumentException("Can't set an Overlay effect as the active effect");

            // Game models are only used if they are enabled
            var gameModel = effectModel as GameModel;
            if (gameModel != null)
                if (!gameModel.Enabled)
                    return;

            if (_activeEffect != null && effectModel.Name == _activeEffect.Name)
                return;

            _activeEffect?.Dispose();

            // If needed, unsuspend when loading a new effect
            if (Suspended)
            {
                _wasSuspendedBeforeGame = true;
                ToggleSuspension();
            }

            _activeEffect = effectModel;
            _activeEffect.Enable();

            if (_activeEffect is GameModel)
                return;

            // Non-game effects are stored as the new LastEffect.
            General.Default.LastEffect = _activeEffect.Name;
            General.Default.Save();

            // Let the ViewModels know
            Events.PublishOnUIThread(new ChangeActiveEffect(_activeEffect.Name));
        }

        public void EnableEffect(EffectModel effectModel)
        {
            if (effectModel is GameModel || effectModel is OverlayModel)
                return;

            ChangeEffect(effectModel);
        }

        public void ToggleSuspension()
        {
            if (Suspended)
            {
                LoadLastKeyboard();
                Suspended = false;
                General.Default.Suspended = false;
                General.Default.Save();
                Events.PublishOnUIThread(new ChangeActiveEffect(_activeEffect?.Name));
                return;
            }

            Suspended = true;
            General.Default.Suspended = true;
            General.Default.Save();
            ActiveKeyboard?.Disable();
            ActiveKeyboard = null;
            Events.PublishOnUIThread(new ChangeActiveEffect(_activeEffect?.Name));
        }

        public bool IsEnabled(EffectModel effectModel)
        {
            if (Suspended)
                return false;
            if (effectModel is GameModel)
                return false;

            return General.Default.LastEffect == effectModel.Name;
        }

        #endregion

        #region Keyboard methods

        private void LoadLastKeyboard()
        {
            var keyboard = KeyboardProviders.FirstOrDefault(k => k.Name == General.Default.LastKeyboard);
            ChangeKeyboard(keyboard ?? KeyboardProviders.First());
        }

        public void ChangeKeyboard(KeyboardProvider keyboardProvider)
        {
            if (ActiveKeyboard != null && keyboardProvider.Name == ActiveKeyboard.Name)
                return;

            ActiveKeyboard?.Disable();

            // Disable everything if there's no active keyboard found
            if (!keyboardProvider.CanEnable())
            {
                ActiveKeyboard = null;
                MessageBox.Show(keyboardProvider.CantEnableText, "Artemis  (╯°□°）╯︵ ┻━┻",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ShutdownEffects();
                return;
            }

            ActiveKeyboard = keyboardProvider;
            ActiveKeyboard.Enable();

            General.Default.LastKeyboard = ActiveKeyboard.Name;
            General.Default.Save();
        }

        #endregion

        #region Workers

        private void UpdateWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var sw = new Stopwatch();
            while (!_updateWorker.CancellationPending)
            {
                if (ActiveKeyboard == null || Suspended)
                {
                    Thread.Sleep(1000/_fps);
                    continue;
                }

                sw.Start();

                // Update the current effect
                _activeEffect.Update();

                // Get ActiveEffect's bitmap
                var bitmap = _activeEffect.GenerateBitmap();

                // Draw enabled overlays on top
                foreach (var overlayModel in EffectModels.OfType<OverlayModel>()
                    .Where(overlayModel => overlayModel.Enabled))
                {
                    overlayModel.Update();
                    bitmap = bitmap != null ? overlayModel.GenerateBitmap(bitmap) : overlayModel.GenerateBitmap();
                }

                // If it exists, send bitmap to the device
                if (bitmap != null && ActiveKeyboard != null)
                {
                    ActiveKeyboard.DrawBitmap(bitmap);

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

        private void ProcessWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!_processWorker.CancellationPending)
            {
                var foundProcess = false;

                // ReSharper disable once LoopCanBePartlyConvertedToQuery
                foreach (var effectModel in EffectModels.OfType<GameModel>())
                {
                    var process = MemoryHelpers.GetProcessIfRunning(effectModel.ProcessName);
                    if (process == null)
                        continue;
                    if (process.HasExited)
                        continue;

                    // If the active effect is a disabled game model, disable it
                    var model = _activeEffect as GameModel;
                    if (model != null && !model.Enabled)
                        LoadLastEffect();
                    else
                    {
                        ChangeEffect(effectModel);
                        foundProcess = true;
                    }
                }

                // If no game process is found, but the active effect still belongs to a game,
                // set it to a normal effect, and if needed, suspend again.
                if (!foundProcess && _activeEffect is GameModel)
                {
                    LoadLastEffect();
                    if (_wasSuspendedBeforeGame)
                    {
                        ToggleSuspension();
                        _wasSuspendedBeforeGame = false;
                    }
                }

                Thread.Sleep(1000);
            }
        }

        #endregion
    }
}