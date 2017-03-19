using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Models;
using Artemis.ViewModels;
using Ninject.Extensions.Logging;

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
        //private readonly Timer _loopTimer;
        private readonly Task _loopTask;
        private readonly ModuleManager _moduleManager;

        public LoopManager(ILogger logger, ModuleManager moduleManager, DeviceManager deviceManager,
            DebugViewModel debugViewModel)
        {
            _logger = logger;
            _moduleManager = moduleManager;
            _deviceManager = deviceManager;
            _debugViewModel = debugViewModel;

            // Setup timers
            _loopTask = Task.Factory.StartNew(ProcessLoop);
            _logger.Info("Intialized LoopManager");
        }

        public DebugViewModel DebugViewModel { get; set; }

        /// <summary>
        ///     Gets whether the loop is running
        /// </summary>
        public bool Running { get; private set; }

        public void Dispose()
        {
            _loopTask.Dispose();
        }

        private void ProcessLoop()
        {
            //TODO DarthAffe 14.01.2017: A stop-condition and a real cleanup instead of just aborting might be better
            while (true)
            {
                try
                {
                    long preUpdateTicks = DateTime.Now.Ticks;

                    Render();

                    int sleep = (int) (40f - (DateTime.Now.Ticks - preUpdateTicks) / 10000f);
                    if (sleep > 0)
                        Thread.Sleep(sleep);
                }
                catch (Exception e)
                {
                    if (Debugger.IsAttached)
                        throw;

                    _logger.Warn(e, "Exception in render loop");
                }
            }
            // ReSharper disable once FunctionNeverReturns
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
                if (lastModule == null || !lastModule.Settings.IsEnabled)
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
                using (
                    var frame = new FrameModel(_deviceManager.ActiveKeyboard, mice.Any(), headsets.Any(), generics.Any(),
                        mousemats.Any()))
                {
                    if (renderModule.IsInitialized)
                        renderModule.Render(frame, keyboardOnly);

                    // Draw enabled overlays on top of the renderModule
                    foreach (var overlayModel in _moduleManager.OverlayModules.Where(o => o.Settings.IsEnabled))
                    {
                        overlayModel.Update();
                        overlayModel.Render(frame, keyboardOnly);
                    }

                    // Render the frame's drawing context to bitmaps
                    frame.RenderBitmaps();

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
}