using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Artemis.Events;
using Caliburn.Micro;
using Ninject.Extensions.Logging;
using Brush = System.Windows.Media.Brush;

namespace Artemis.Managers
{
    /// <summary>
    ///     Manages the main programn loop
    /// </summary>
    public class LoopManager : IDisposable, IHandle<ActiveKeyboardChanged>, IHandle<ActiveEffectChanged>
    {
        private readonly DeviceManager _deviceManager;
        private readonly EffectManager _effectManager;
        private readonly ILogger _logger;
        private readonly Timer _loopTimer;
        private Bitmap _keyboardBitmap;

        public LoopManager(IEventAggregator events, ILogger logger, EffectManager effectManager,
            DeviceManager deviceManager)
        {
            events.Subscribe(this);
            _logger = logger;
            _effectManager = effectManager;
            _deviceManager = deviceManager;

            // Setup timers
            _loopTimer = new Timer(40);
            _loopTimer.Elapsed += Render;
            _loopTimer.Start();

            _logger.Info("Intialized LoopManager");
        }

        /// <summary>
        ///     Gets whether the loop is running
        /// </summary>
        public bool Running { get; private set; }

        public void Dispose()
        {
            _loopTimer.Stop();
            _loopTimer.Dispose();
            _keyboardBitmap?.Dispose();
        }

        public void Handle(ActiveEffectChanged message)
        {
            if (_deviceManager.ActiveKeyboard != null && _effectManager.ActiveEffect != null)
                _keyboardBitmap = _deviceManager.ActiveKeyboard.KeyboardBitmap(_effectManager.ActiveEffect.KeyboardScale);
        }

        public void Handle(ActiveKeyboardChanged message)
        {
            if (_deviceManager.ActiveKeyboard != null && _effectManager.ActiveEffect != null)
                _keyboardBitmap = _deviceManager.ActiveKeyboard.KeyboardBitmap(_effectManager.ActiveEffect.KeyboardScale);
        }

        public Task StartAsync()
        {
            return Task.Run(() => Start());
        }

        private void Start()
        {
            if (Running)
                return;

            _logger.Debug("Starting LoopManager");

            if (_deviceManager.ActiveKeyboard == null)
                _deviceManager.EnableLastKeyboard();
            // If still null, no last keyboard, so stop.
            if (_deviceManager.ActiveKeyboard == null)
            {
                _logger.Debug("Cancel LoopManager start, no keyboard");
                return;
            }

            if (_effectManager.ActiveEffect == null)
            {
                var lastEffect = _effectManager.GetLastEffect();
                if (lastEffect == null)
                {
                    _logger.Debug("Cancel LoopManager start, no effect");
                    return;
                }
                _effectManager.ChangeEffect(lastEffect);
            }

            Running = true;
        }

        public void Stop()
        {
            if (!Running)
                return;

            _logger.Debug("Stopping LoopManager");
            Running = false;

            _deviceManager.ReleaseActiveKeyboard();
            _keyboardBitmap?.Dispose();
            _keyboardBitmap = null;
        }

        private void Render(object sender, ElapsedEventArgs e)
        {
            if (!Running)
                return;

            // Stop if no active effect
            if (_effectManager.ActiveEffect == null)
            {
                _logger.Debug("No active effect, stopping");
                Stop();
                return;
            }
            var renderEffect = _effectManager.ActiveEffect;

            if (_deviceManager.ChangingKeyboard || _keyboardBitmap == null)
                return;

            // Stop if no active keyboard
            if (_deviceManager.ActiveKeyboard == null)
            {
                _logger.Debug("No active keyboard, stopping");
                Stop();
                return;
            }

            lock (_deviceManager.ActiveKeyboard)
            {
                // Skip frame if effect is still initializing
                if (renderEffect.Initialized == false)
                    return;

                // ApplyProperties the current effect
                if (renderEffect.Initialized)
                    renderEffect.Update();

                // Get ActiveEffect's bitmap
                Brush mouseBrush = null;
                Brush headsetBrush = null;
                var mice = _deviceManager.MiceProviders.Where(m => m.CanUse).ToList();
                var headsets = _deviceManager.HeadsetProviders.Where(m => m.CanUse).ToList();

                using (var keyboardGraphics = Graphics.FromImage(_keyboardBitmap))
                {
                    // Fill the bitmap's background with black to avoid trailing colors on some keyboards
                    keyboardGraphics.Clear(Color.Black);

                    if (renderEffect.Initialized)
                        renderEffect.Render(keyboardGraphics, out mouseBrush, out headsetBrush, mice.Any(),
                            headsets.Any());

                    // Draw enabled overlays on top of the renderEffect
                    foreach (var overlayModel in _effectManager.EnabledOverlays)
                    {
                        overlayModel.Update();
                        overlayModel.RenderOverlay(keyboardGraphics, ref mouseBrush, ref headsetBrush, mice.Any(),
                            headsets.Any());
                    }

                    // Update mice and headsets
                    foreach (var mouse in mice)
                        mouse.UpdateDevice(mouseBrush);
                    foreach (var headset in headsets)
                        headset.UpdateDevice(headsetBrush);
                }

                // Update the keyboard
                _deviceManager.ActiveKeyboard?.DrawBitmap(_keyboardBitmap);
            }
        }
    }
}