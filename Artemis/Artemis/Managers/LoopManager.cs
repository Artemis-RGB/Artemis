using System;
using System.Drawing;
using System.Linq;
using System.Timers;
using Caliburn.Micro;
using Ninject.Extensions.Logging;
using Brush = System.Windows.Media.Brush;

namespace Artemis.Managers
{
    /// <summary>
    ///     Manages the main programn loop
    /// </summary>
    public class LoopManager : IDisposable
    {
        private readonly DeviceManager _deviceManager;
        private readonly EffectManager _effectManager;
        private readonly IEventAggregator _events;
        private readonly ILogger _logger;
        private readonly Timer _loopTimer;

        public LoopManager(ILogger logger, IEventAggregator events, EffectManager effectManager,
            DeviceManager deviceManager)
        {
            _logger = logger;
            _events = events;
            _effectManager = effectManager;
            _deviceManager = deviceManager;

            // Setup timers
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
                Bitmap bitmap = null;
                Brush mouseBrush = null;
                Brush headsetBrush = null;
                var mice = _deviceManager.MiceProviders.Where(m => m.CanUse).ToList();
                var headsets = _deviceManager.HeadsetProviders.Where(m => m.CanUse).ToList();

                if (renderEffect.Initialized)
                    renderEffect.Render(out bitmap, out mouseBrush, out headsetBrush, mice.Any(), headsets.Any());

                // Draw enabled overlays on top of the renderEffect
                foreach (var overlayModel in _effectManager.EnabledOverlays)
                {
                    overlayModel.Update();
                    overlayModel.RenderOverlay(ref bitmap, ref mouseBrush, ref headsetBrush, mice.Any(), headsets.Any());
                }

                // Update mice and headsets
                foreach (var mouse in mice)
                    mouse.UpdateDevice(mouseBrush);
                foreach (var headset in headsets)
                    headset.UpdateDevice(headsetBrush);

                // If no bitmap was generated this frame is done
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

                // Update the keyboard
                _deviceManager.ActiveKeyboard?.DrawBitmap(bitmap);
            }
        }
    }
}