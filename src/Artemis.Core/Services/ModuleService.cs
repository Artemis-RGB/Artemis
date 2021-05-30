using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Artemis.Core.Modules;
using Serilog;

namespace Artemis.Core.Services
{
    internal class ModuleService : IModuleService
    {
        private readonly ILogger _logger;
        private readonly IPluginManagementService _pluginManagementService;
        private readonly object _updateLock = new();

        private Module? _activationOverride;
        private readonly Timer _activationUpdateTimer;

        public ModuleService(ILogger logger, IPluginManagementService pluginManagementService)
        {
            _logger = logger;
            _pluginManagementService = pluginManagementService;

            _activationUpdateTimer = new Timer(2000);
            _activationUpdateTimer.Start();
            _activationUpdateTimer.Elapsed += ActivationUpdateTimerOnElapsed;
        }

        protected virtual void OnModuleActivated(ModuleEventArgs e)
        {
            ModuleActivated?.Invoke(this, e);
        }

        protected virtual void OnModuleDeactivated(ModuleEventArgs e)
        {
            ModuleDeactivated?.Invoke(this, e);
        }

        private void OverrideActivate(Module module)
        {
            try
            {
                if (module.IsActivated)
                    return;

                // If activating while it should be deactivated, its an override
                bool shouldBeActivated = module.EvaluateActivationRequirements();
                module.Activate(!shouldBeActivated);
            }
            catch (Exception e)
            {
                _logger.Error(new ArtemisPluginFeatureException(module, "Failed to activate module.", e), "Failed to activate module");
                throw;
            }
        }

        private void OverrideDeactivate(Module module)
        {
            try
            {
                if (!module.IsActivated)
                    return;

                // If deactivating while it should be activated, its an override
                bool shouldBeActivated = module.EvaluateActivationRequirements();
                // No need to deactivate if it is not in an overridden state
                if (shouldBeActivated && !module.IsActivatedOverride)
                    return;

                module.Deactivate(true);
            }
            catch (Exception e)
            {
                _logger.Error(new ArtemisPluginFeatureException(module, "Failed to deactivate module.", e), "Failed to deactivate module");
                throw;
            }
        }

        private void ActivationUpdateTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            UpdateModuleActivation();
        }

        public void UpdateModuleActivation()
        {
            lock (_updateLock)
            {
                try
                {
                    _activationUpdateTimer.Elapsed -= ActivationUpdateTimerOnElapsed;
                    List<Module> modules = _pluginManagementService.GetFeaturesOfType<Module>().ToList();
                    foreach (Module module in modules)
                    {
                        if (module.IsActivatedOverride)
                            continue;

                        if (module.IsAlwaysAvailable)
                        {
                            module.Activate(false);
                            continue;
                        }

                        module.Profiler.StartMeasurement("EvaluateActivationRequirements");
                        bool shouldBeActivated = module.IsEnabled && module.EvaluateActivationRequirements();
                        module.Profiler.StopMeasurement("EvaluateActivationRequirements");

                        if (shouldBeActivated && !module.IsActivated)
                        {
                            module.Activate(false);
                            OnModuleActivated(new ModuleEventArgs(module));
                        }
                        else if (!shouldBeActivated && module.IsActivated)
                        {
                            module.Deactivate(false);
                            OnModuleDeactivated(new ModuleEventArgs(module));
                        }
                    }
                }
                finally
                {
                    _activationUpdateTimer.Elapsed += ActivationUpdateTimerOnElapsed;
                }
            }
        }

        public void SetActivationOverride(Module? module)
        {
            lock (_updateLock)
            {
                if (_activationOverride != null)
                    OverrideDeactivate(_activationOverride);
                _activationOverride = module;
                if (_activationOverride != null)
                    OverrideActivate(_activationOverride);
            }
        }

        public void UpdateActiveModules(double deltaTime)
        {
            lock (_updateLock)
            {
                List<Module> modules = _pluginManagementService.GetFeaturesOfType<Module>().ToList();
                foreach (Module module in modules)
                    module.InternalUpdate(deltaTime);
            }
        }

        public event EventHandler<ModuleEventArgs>? ModuleActivated;
        public event EventHandler<ModuleEventArgs>? ModuleDeactivated;
    }
}