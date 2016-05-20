using System;
using System.Drawing;
using System.Linq;
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
        private readonly DeviceManager _deviceManager;
        private readonly ILogger _logger;
        private readonly Timer _loopTimer;

        public LoopManager(ILogger logger, IEventAggregator events, EffectManager effectManager,
            DeviceManager deviceManager)
        {
            _logger = logger;
            _events = events;
            _effectManager = effectManager;
            _deviceManager = deviceManager;

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

            if (_deviceManager.ChangingKeyboard)
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
                var bitmap = renderEffect.Initialized
                    ? renderEffect.GenerateBitmap()
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

                // Fill the bitmap's background with black to avoid trailing colors on some keyboards
                var fixedBmp = new Bitmap(bitmap.Width, bitmap.Height);
                using (var g = Graphics.FromImage(fixedBmp))
                {
                    g.Clear(Color.Black);
                    g.DrawImage(bitmap, 0, 0);
                }

                bitmap = fixedBmp;

                // If it exists, send bitmap to the device
                _deviceManager.ActiveKeyboard?.DrawBitmap(bitmap);

                foreach (var mouse in _deviceManager.MiceProviders.Where(m => m.CanUse))
                    mouse.UpdateDevice(renderEffect.GenerateMouseBrush());
                foreach (var headset in _deviceManager.HeadsetProviders.Where(h => h.CanUse))
                    headset.UpdateDevice(renderEffect.GenerateHeadsetBrush());

                // debugging TODO: Disable when window isn't shown (in Debug VM, or get rid of it, w/e)
                _events.PublishOnUIThread(new ChangeBitmap(bitmap));
            }
        }
    }
}