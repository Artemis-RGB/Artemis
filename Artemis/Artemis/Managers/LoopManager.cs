using System;
using System.Timers;
using Artemis.Events;
using Caliburn.Micro;
using Ninject.Extensions.Logging;

namespace Artemis.Managers
{
    /// <summary>
    ///     Manages the main programn loop
    /// </summary>
    public class LoopManager : IDisposable
    {
        private readonly EffectManager _effectManager;
        private readonly IEventAggregator _events;
        private readonly KeyboardManager _keyboardManager;
        private readonly Timer _loopTimer;

        public LoopManager(ILogger logger, IEventAggregator events, EffectManager effectManager,
            KeyboardManager keyboardManager)
        {
            Logger = logger;
            _events = events;
            _effectManager = effectManager;
            _keyboardManager = keyboardManager;

            _loopTimer = new Timer(40);
            _loopTimer.Elapsed += Render;
            _loopTimer.Start();
        }

        public ILogger Logger { get; }

        /// <summary>
        ///     Gets whether the loop is running
        /// </summary>
        public bool Running { get; private set; }

        public void Dispose()
        {
            _loopTimer.Stop();
            _loopTimer.Dispose();
        }

        public void Start()
        {
            Logger.Debug("Starting LoopManager");

            if (_keyboardManager.ActiveKeyboard == null)
                _keyboardManager.EnableLastKeyboard();

            if (_effectManager.ActiveEffect == null)
            {
                var lastEffect = _effectManager.GetLastEffect();
                if (lastEffect == null)
                    return;
                
                _effectManager.ChangeEffect(lastEffect);
            }

            Running = true;
        }

        public void Stop()
        {
            Logger.Debug("Stopping LoopManager");
            Running = false;

            _keyboardManager.ReleaseActiveKeyboard();
        }

        private void Render(object sender, ElapsedEventArgs e)
        {
            if (!Running)
                return;

            // Stop if no active keyboard/efffect
            if (_keyboardManager.ActiveKeyboard == null || _effectManager.ActiveEffect == null)
            {
                Logger.Debug("No active keyboard/effect, stopping. " +
                             $"Keyboard={_keyboardManager.ActiveKeyboard?.Name}, " +
                             $"effect={_effectManager.ActiveEffect?.Name}");
                Stop();
                return;
            }

            // Lock both the active keyboard and active effect so they will not change while rendering.
            lock (_keyboardManager.ActiveKeyboard)
            {
                lock (_effectManager.ActiveEffect)
                {
                    // Skip frame if effect is still initializing
                    if (_effectManager.ActiveEffect.Initialized == false)
                        return;

                    // Update the current effect
                    if (_effectManager.ActiveEffect.Initialized)
                        _effectManager.ActiveEffect.Update();

                    // Get ActiveEffect's bitmap
                    var bitmap = _effectManager.ActiveEffect.Initialized
                        ? _effectManager.ActiveEffect.GenerateBitmap()
                        : null;

                    lock (_effectManager.EnabledOverlays)
                    {
                        // Draw enabled overlays on top
                        foreach (var overlayModel in _effectManager.EnabledOverlays)
                        {
                            overlayModel.Update();
                            bitmap = bitmap != null
                                ? overlayModel.GenerateBitmap(bitmap)
                                : overlayModel.GenerateBitmap();
                        }
                    }

                    if (bitmap == null)
                        return;

                    // If it exists, send bitmap to the device
                    _keyboardManager.ActiveKeyboard.DrawBitmap(bitmap);

                    // debugging TODO: Disable when window isn't shown (in Debug VM, or get rid of it, w/e)
                    _events.PublishOnUIThread(new ChangeBitmap(bitmap));
                }
            }
        }
    }
}