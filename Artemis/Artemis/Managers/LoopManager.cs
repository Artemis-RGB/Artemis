using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Media;
using Artemis.DeviceProviders;
using Artemis.ViewModels;
using Ninject.Extensions.Logging;
using Color = System.Drawing.Color;
using Timer = System.Timers.Timer;

namespace Artemis.Managers
{
    /// <summary>
    ///     Manages the main programn loop
    /// </summary>
    public class LoopManager : IDisposable
    {
        private readonly DebugViewModel _debugViewModel;
        private readonly DeviceManager _deviceManager;
        private readonly ILogger _logger;
        private readonly Timer _loopTimer;
        private readonly ModuleManager _moduleManager;

        public LoopManager(ILogger logger, ModuleManager moduleManager, DeviceManager deviceManager,
            DebugViewModel debugViewModel)
        {
            _logger = logger;
            _moduleManager = moduleManager;
            _deviceManager = deviceManager;
            _debugViewModel = debugViewModel;

            // Setup timers
            _loopTimer = new Timer(40);
            _loopTimer.Elapsed += LoopTimerOnElapsed;
            _loopTimer.Start();

            _logger.Info("Intialized LoopManager");
        }

        public DebugViewModel DebugViewModel { get; set; }

        /// <summary>
        ///     Gets whether the loop is running
        /// </summary>
        public bool Running { get; private set; }

        public void Dispose()
        {
            _loopTimer.Stop();
            _loopTimer.Dispose();
        }

        private void LoopTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            try
            {
                Render();
            }
            catch (Exception e)
            {
                _logger.Warn(e, "Exception in render loop");
            }
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

            while (_deviceManager.ChangingKeyboard)
                Thread.Sleep(200);

            // If still null, no last keyboard, so stop.
            if (_deviceManager.ActiveKeyboard == null)
            {
                _logger.Debug("Cancel LoopManager start, no keyboard");
                return;
            }

            if (_moduleManager.ActiveModule == null)
            {
                var lastModule = _moduleManager.GetLastModule();
                if (lastModule == null)
                {
                    _logger.Debug("Cancel LoopManager start, no module");
                    return;
                }
                _moduleManager.ChangeActiveModule(lastModule);
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

        private void Render()
        {
            if (!Running || _deviceManager.ChangingKeyboard)
                return;

            // Stop if no active module
            if (_moduleManager.ActiveModule == null)
            {
                _logger.Debug("No active module, stopping");
                Stop();
                return;
            }
            var renderModule = _moduleManager.ActiveModule;

            // Stop if no active keyboard
            if (_deviceManager.ActiveKeyboard == null)
            {
                _logger.Debug("No active keyboard, stopping");
                Stop();
                return;
            }

            lock (_deviceManager.ActiveKeyboard)
            {
                // Skip frame if module is still initializing
                if (renderModule.IsInitialized == false)
                    return;

                // ApplyProperties the current module
                if (renderModule.IsInitialized)
                    renderModule.Update();

                // Get the devices that must be rendered to
                var mice = _deviceManager.MiceProviders.Where(m => m.CanUse).ToList();
                var headsets = _deviceManager.HeadsetProviders.Where(m => m.CanUse).ToList();
                var generics = _deviceManager.GenericProviders.Where(m => m.CanUse).ToList();
                var mousemats = _deviceManager.MousematProviders.Where(m => m.CanUse).ToList();
                var keyboardOnly = !mice.Any() && !headsets.Any() && !generics.Any() && !mousemats.Any();

                // Setup the frame for this tick
                using (var frame = new RenderFrame(_deviceManager.ActiveKeyboard))
                {
                    if (renderModule.IsInitialized)
                        renderModule.Render(frame, keyboardOnly);

                    // Draw enabled overlays on top of the renderModule
                    foreach (var overlayModel in _moduleManager.OverlayModules.Where(o => o.Settings.IsEnabled))
                    {
                        overlayModel.Update();
                        overlayModel.Render(frame, keyboardOnly);
                    }

                    // Update the keyboard
                    _deviceManager.ActiveKeyboard?.DrawBitmap(frame.KeyboardBitmap);

                    // Update the other devices
                    foreach (var mouse in mice)
                        mouse.UpdateDevice(frame.MouseBitmap);
                    foreach (var headset in headsets)
                        headset.UpdateDevice(frame.HeadsetBitmap);
                    foreach (var generic in generics)
                        generic.UpdateDevice(frame.GenericBitmap);
                    foreach (var mousemat in mousemats)
                        mousemat.UpdateDevice(frame.MousematBitmap);

                    _debugViewModel.DrawFrame(frame);

                    OnRenderCompleted();
                }
            }
        }

        public event EventHandler RenderCompleted;

        protected virtual void OnRenderCompleted()
        {
            RenderCompleted?.Invoke(this, EventArgs.Empty);
        }
    }

    public class RenderFrame : IDisposable
    {
        public RenderFrame(KeyboardProvider keyboard)
        {
            if (keyboard == null)
                return;

            KeyboardBitmap = keyboard.KeyboardBitmap(4);
            KeyboardBitmap.SetResolution(96, 96);

            MouseBitmap = new Bitmap(40, 40);
            MouseBitmap.SetResolution(96, 96);

            HeadsetBitmap = new Bitmap(40, 40);
            HeadsetBitmap.SetResolution(96, 96);

            GenericBitmap = new Bitmap(40, 40);
            GenericBitmap.SetResolution(96, 96);

            MousematBitmap = new Bitmap(40, 40);
            MousematBitmap.SetResolution(96, 96);

            using (var g = Graphics.FromImage(KeyboardBitmap))
            {
                g.Clear(Color.Black);
            }
            using (var g = Graphics.FromImage(MouseBitmap))
            {
                g.Clear(Color.Black);
            }
            using (var g = Graphics.FromImage(HeadsetBitmap))
            {
                g.Clear(Color.Black);
            }
            using (var g = Graphics.FromImage(GenericBitmap))
            {
                g.Clear(Color.Black);
            }
            using (var g = Graphics.FromImage(MousematBitmap))
            {
                g.Clear(Color.Black);
            }
        }

        public Bitmap KeyboardBitmap { get; set; }
        public Bitmap MouseBitmap { get; set; }
        public Bitmap HeadsetBitmap { get; set; }
        public Bitmap GenericBitmap { get; set; }
        public Bitmap MousematBitmap { get; set; }

        public void Dispose()
        {
            KeyboardBitmap?.Dispose();
            MouseBitmap?.Dispose();
            HeadsetBitmap?.Dispose();
            GenericBitmap?.Dispose();
            MousematBitmap?.Dispose();
        }
    }
}