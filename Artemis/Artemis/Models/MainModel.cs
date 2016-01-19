using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Artemis.Events;
using Artemis.KeyboardProviders;
using Artemis.Settings;
using Artemis.Utilities.GameState;
using Artemis.Utilities.Memory;
using Caliburn.Micro;

namespace Artemis.Models
{
    public class MainModel
    {
        private readonly BackgroundWorker _processWorker;
        private readonly BackgroundWorker _updateWorker;

        public MainModel(IEventAggregator events)
        {
            EffectModels = new List<EffectModel>();
            KeyboardProviders = ProviderHelper.GetKeyboardProviders();
            GameStateWebServer = new GameStateWebServer();

            Events = events;
            Fps = 25;

            _updateWorker = new BackgroundWorker {WorkerSupportsCancellation = true};
            _processWorker = new BackgroundWorker {WorkerSupportsCancellation = true};
            _updateWorker.DoWork += UpdateWorker_DoWork;
            _processWorker.DoWork += ProcessWorker_DoWork;
        }

        public EffectModel ActiveEffect { get; set; }
        public KeyboardProvider ActiveKeyboard { get; set; }
        public List<EffectModel> EffectModels { get; set; }
        public List<KeyboardProvider> KeyboardProviders { get; set; }

        public GameStateWebServer GameStateWebServer { get; set; }
        public IEventAggregator Events { get; set; }
        public int Fps { get; set; }
        public bool Enabled { get; set; }

        #region Effect methods

        public void StartEffects()
        {
            LoadLastKeyboard();

            // Start the webserver
            GameStateWebServer.Start();

            // Load last non-game effect and enable
            LoadLastEffect();

            // Start the Background Workers
            _updateWorker.RunWorkerAsync();
            _processWorker.RunWorkerAsync();
        }

        public void ShutdownEffects()
        {
            // Stop the Background Worker
            _updateWorker.CancelAsync();
            _processWorker.CancelAsync();

            // Dispose the current active effect
            ActiveEffect?.Dispose();
            ActiveEffect = null;

            ActiveKeyboard?.Disable();
            ActiveKeyboard = null;
        }

        private void LoadLastKeyboard()
        {
            var keyboard = KeyboardProviders.FirstOrDefault(k => k.Name == General.Default.LastKeyboard);
            ChangeKeyboard(keyboard ?? KeyboardProviders.First(k => k.Name == "Logitech G910 Orion Spark RGB"));
        }

        private void ChangeKeyboard(KeyboardProvider keyboardProvider)
        {
            if (ActiveKeyboard != null && keyboardProvider.Name == ActiveKeyboard.Name)
                return;

            ActiveKeyboard?.Disable();
            ActiveKeyboard = keyboardProvider;
            ActiveKeyboard.Enable();

            General.Default.LastKeyboard = ActiveKeyboard.Name;
            General.Default.Save();
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

            if (ActiveEffect != null && effectModel.Name == ActiveEffect.Name)
                return;

            ActiveEffect?.Dispose();
            ActiveEffect = effectModel;
            ActiveEffect.Enable();

            if (ActiveEffect is GameModel) return;

            // Non-game effects are stored as the new LastEffect.
            General.Default.LastEffect = ActiveEffect.Name;
            General.Default.Save();

            // Let the ViewModels know
            Events.PublishOnUIThread(new ChangeActiveEffect(ActiveEffect.Name));
        }

        public void EnableEffect(EffectModel effectModel)
        {
            if (effectModel is GameModel || effectModel is OverlayModel)
                return;

            ChangeEffect(effectModel);
        }

        public bool IsEnabled(EffectModel effectModel)
        {
            if (effectModel is GameModel)
                return false;

            return General.Default.LastEffect == effectModel.Name;
        }

        #endregion Effect methods

        #region Workers

        private void UpdateWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var sw = new Stopwatch();
            while (!_updateWorker.CancellationPending)
            {
                sw.Start();

                // Update the current effect
                ActiveEffect.Update();

                // Get ActiveEffect's bitmap
                var bitmap = ActiveEffect.GenerateBitmap();

                // Draw enabled overlays on top
                foreach (
                    var overlayModel in EffectModels.OfType<OverlayModel>().Where(overlayModel => overlayModel.Enabled))
                {
                    overlayModel.Update();
                    bitmap = bitmap != null ? overlayModel.GenerateBitmap(bitmap) : overlayModel.GenerateBitmap();
                }

                // If it exists, send bitmap to the device
                if (bitmap != null && ActiveKeyboard != null)
                {
                    ActiveKeyboard.DrawBitmap(bitmap);

                    // debugging
                    Events.PublishOnUIThread(new ChangeBitmap(bitmap));
                }

                // Sleep according to time left this frame
                var sleep = (int) ((1000/Fps) - sw.ElapsedMilliseconds);
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

                    ChangeEffect(effectModel);
                    foundProcess = true;
                }

                // If no game process is found, but the active effect still belongs to a game,
                // set it to a normal effect
                if (!foundProcess && ActiveEffect is GameModel)
                    LoadLastEffect();

                Thread.Sleep(1000);
            }
        }

        #endregion
    }
}