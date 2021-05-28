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
        private List<Module> _activationOverride = new();

        public ModuleService(ILogger logger, IPluginManagementService pluginManagementService)
        {
            _logger = logger;
            _pluginManagementService = pluginManagementService;

            Timer activationUpdateTimer = new(2000);
            activationUpdateTimer.Start();
            activationUpdateTimer.Elapsed += ActivationUpdateTimerOnElapsed;
        }

        public void OverrideActivate(Module module)
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

        public void OverrideDeactivate(Module module)
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

        protected virtual void OnModuleActivated(ModuleEventArgs e)
        {
            ModuleActivated?.Invoke(this, e);
        }

        protected virtual void OnModuleDeactivated(ModuleEventArgs e)
        {
            ModuleDeactivated?.Invoke(this, e);
        }

        private void ActivationUpdateTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            UpdateModuleActivation();
        }

        public void UpdateModuleActivation()
        {
            List<Module> modules = _pluginManagementService.GetFeaturesOfType<Module>().ToList();
            foreach (Module module in modules)
            {
                lock (module)
                {
                    if (module.IsActivatedOverride)
                        continue;

                    if (module.IsAlwaysAvailable)
                    {
                        module.Activate(false);
                        return;
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
        }

        /// <inheritdoc />
        public void SetActivationOverride(IEnumerable<Module> modules)
        {
            foreach (Module module in _activationOverride)
                OverrideDeactivate(module);

            _activationOverride = modules.ToList();
            foreach (Module module in _activationOverride)
                OverrideActivate(module);
        }

        public void UpdateActiveModules(double deltaTime)
        {
            List<Module> modules = _pluginManagementService.GetFeaturesOfType<Module>().ToList();
            foreach (Module module in modules)
            {
                if (module.IsActivated)
                    module.InternalUpdate(deltaTime);
            }
        }

        public event EventHandler<ModuleEventArgs>? ModuleActivated;
        public event EventHandler<ModuleEventArgs>? ModuleDeactivated;
    }
}