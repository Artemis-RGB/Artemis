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
        private readonly ILogger _logger;
        private readonly Timer _loopTimer;

        public LoopManager(ILogger logger, IEventAggregator events, EffectManager effectManager,
            KeyboardManager keyboardManager)
        {
            _logger = logger;
            _events = events;
            _effectManager = effectManager;
            _keyboardManager = keyboardManager;

            _loopTimer = new Timer(40);
            _loopTimer.Elapsed += Render;
            _loopTimer.Start();
        }

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
            if (Running)
                return;

            _logger.Debug("Starting LoopManager");

            if (_keyboardManager.ActiveKeyboard == null)
                _keyboardManager.EnableLastKeyboard();
            // If still null, no last keyboard, so stop.
            if (_keyboardManager.ActiveKeyboard == null)
            {
                _logger.Debug("Cancel LoopManager start, no keyboard");
                return;
            }

            // TODO: Deadlock maybe? I don't know what Resharper is on about
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

            _keyboardManager.ReleaseActiveKeyboard();
        }

        private void Render(object sender, ElapsedEventArgs e)
        {
            if (!Running)
                return;

            // Stop if no active keyboard
            if (_keyboardManager.ActiveKeyboard == null)
            {
                _logger.Debug("No active keyboard, stopping");
                Stop();
                return;
            }
            // Lock both the active keyboard and active effect so they will not change while rendering.
            lock (_keyboardManager)
            {
                lock (_effectManager)
                {
                    // Stop if no active effect
                    if (_effectManager.ActiveEffect == null)
                    {
                        _logger.Debug("No active effect, stopping");
                        Stop();
                        return;
                    }

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

                    // Draw enabled overlays on top
                    foreach (var overlayModel in _effectManager.EnabledOverlays)
                    {
                        overlayModel.Update();
                        bitmap = bitmap != null
                            ? overlayModel.GenerateBitmap(bitmap)
                            : overlayModel.GenerateBitmap();
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