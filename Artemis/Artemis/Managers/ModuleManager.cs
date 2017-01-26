using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Events;
using Artemis.Modules.Abstract;
using Artemis.Settings;
using Ninject.Extensions.Logging;

namespace Artemis.Managers
{
    public class ModuleManager
    {
        private readonly DeviceManager _deviceManager;
        private readonly ILogger _logger;
        private LoopManager _waitLoopManager;
        private ModuleModel _waitEffect;
        private readonly GeneralSettings _generalSettings;

        public ModuleManager(ILogger logger, DeviceManager deviceManager, List<ModuleModel> moduleModels)
        {
            _generalSettings = DAL.SettingsProvider.Load<GeneralSettings>();
            _logger = logger;
            _deviceManager = deviceManager;

            Modules = new List<ModuleModel>(moduleModels.Where(m => !m.IsOverlay && !m.IsBoundToProcess));
            OverlayModules = new List<ModuleModel>(moduleModels.Where(m => m.IsOverlay));
            // Exclude WoW if needed
            ProcessModules = _generalSettings.GamestatePort == 62575
                ? new List<ModuleModel>(moduleModels.Where(m => m.IsBoundToProcess))
                : new List<ModuleModel>(moduleModels.Where(m => m.IsBoundToProcess && m.Name != "WoW"));

            _logger.Info("Intialized ModuleManager");
        }

        public List<ModuleModel> Modules { get; set; }
        public List<ModuleModel> ProcessModules { get; set; }
        public List<ModuleModel> OverlayModules { get; set; }
        public ModuleModel ActiveModule { get; private set; }

        public event EventHandler<ModuleChangedEventArgs> EffectChanged;

        /// <summary>
        ///     Loads the last active module from settings and enables it.
        /// </summary>
        public ModuleModel GetLastModule()
        {
            _logger.Debug("Getting last module: {0}", _generalSettings.LastModule);

            if (_generalSettings.LastModule != null)
            {
                var lastModule = Modules.FirstOrDefault(e => e.Name == _generalSettings.LastModule);
                if (lastModule != null)
                    return lastModule;
            }

            _logger.Debug("Getting last module not found, defaulting to GeneralProfile");
            return Modules.First(e => e.Name == "GeneralProfile");
        }

        /// <summary>
        ///     Disables the current module and changes it to the provided module.
        /// </summary>
        /// <param name="moduleModel">The module to activate</param>
        /// <param name="loopManager">Optionally pass the LoopManager to automatically start it, if it's not running.</param>
        /// <param name="storeAsLast">Whether or not to store this effect as the last effect</param>
        public void ChangeActiveModule(ModuleModel moduleModel, LoopManager loopManager = null, bool storeAsLast = true)
        {
            if (_waitEffect != null)
            {
                _logger.Debug("Stopping module change because a change is already queued");
                return;
            }

            if (moduleModel == null)
                throw new ArgumentNullException(nameof(moduleModel));

            if (_deviceManager.ActiveKeyboard == null)
            {
                _logger.Debug("Stopping module change until keyboard is enabled");
                _waitEffect = moduleModel;
                _waitLoopManager = loopManager;
                _deviceManager.OnKeyboardChanged += DeviceManagerOnOnKeyboardChanged;
                _deviceManager.EnableLastKeyboard();
                return;
            }

            // Process bound modules are only used if they are enabled
            if (moduleModel.Settings != null && !moduleModel.Settings.IsEnabled && moduleModel.IsBoundToProcess)
            {
                _logger.Debug("Cancelling module change, provided module is process bound and not enabled");
                return;
            }

            var wasNull = false;
            if (ActiveModule == null)
            {
                wasNull = true;
                ActiveModule = moduleModel;
            }

            lock (ActiveModule)
            {
                if (!wasNull)
                    ActiveModule.Dispose();
                lock (moduleModel)
                {
                    ActiveModule = moduleModel;
                    ActiveModule.Enable();
                    if (!ActiveModule.IsInitialized)
                    {
                        _logger.Debug("Cancelling module change, couldn't initialize the module ({0})", moduleModel.Name);
                        ActiveModule = null;
                        return;
                    }
                }
            }

            if (loopManager != null && !loopManager.Running)
            {
                _logger.Debug("Starting LoopManager for module change");
                loopManager.StartAsync();
            }
            
            if (!ActiveModule.IsBoundToProcess && !ActiveModule.IsOverlay && storeAsLast)
            {
                _generalSettings.LastModule = ActiveModule?.Name;
                _generalSettings.Save();
            }

            _logger.Debug("Changed active module to: {0}", moduleModel.Name);
            RaiseEffectChangedEvent(new ModuleChangedEventArgs(moduleModel));
        }

        private void DeviceManagerOnOnKeyboardChanged(object sender, KeyboardChangedEventArgs e)
        {
            _deviceManager.OnKeyboardChanged -= DeviceManagerOnOnKeyboardChanged;
            _logger.Debug("Resuming module change");

            var module = _waitEffect;
            _waitEffect = null;
            var loopManager = _waitLoopManager;
            _waitLoopManager = null;

            ChangeActiveModule(module, loopManager);
        }

        /// <summary>
        ///     Clears the current module
        /// </summary>
        public void ClearActiveModule()
        {
            if (ActiveModule == null)
                return;

            lock (ActiveModule)
            {
                ActiveModule.Dispose();
                ActiveModule = null;

                _generalSettings.LastModule = null;
                _generalSettings.Save();
            }

            _logger.Debug("Cleared active module");
        }

        /// <summary>
        ///     Disables the currently active process bound module
        /// </summary>
        public void DisableProcessBoundModule()
        {
            if (ActiveModule == null)
                return;
            if (!ActiveModule.IsBoundToProcess)
            {
                _logger.Warn("Active module {0} is not process bound but is being disabled as if it is.",
                    ActiveModule.Name);
                return;
            }

            var lastModule = GetLastModule();
            if (lastModule == null || !lastModule.Settings.IsEnabled)
                ClearActiveModule();
            else
                ChangeActiveModule(lastModule);
        }

        protected virtual void RaiseEffectChangedEvent(ModuleChangedEventArgs e)
        {
            var handler = EffectChanged;
            handler?.Invoke(this, e);
        }
    }
}