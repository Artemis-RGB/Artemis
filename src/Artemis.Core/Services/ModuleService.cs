using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Artemis.Core.Modules;
using Artemis.Storage.Repositories.Interfaces;
using Serilog;
using Timer = System.Timers.Timer;

namespace Artemis.Core.Services
{
    internal class ModuleService : IModuleService
    {
        private static readonly SemaphoreSlim ActiveModuleSemaphore = new(1, 1);
        private readonly ILogger _logger;
        private readonly IModuleRepository _moduleRepository;
        private readonly IPluginManagementService _pluginManagementService;
        private readonly IProfileService _profileService;

        public ModuleService(ILogger logger, IModuleRepository moduleRepository, IPluginManagementService pluginManagementService, IProfileService profileService)
        {
            _logger = logger;
            _moduleRepository = moduleRepository;
            _pluginManagementService = pluginManagementService;
            _profileService = profileService;
            _pluginManagementService.PluginFeatureEnabled += OnPluginFeatureEnabled;

            Timer activationUpdateTimer = new(2000);
            activationUpdateTimer.Start();
            activationUpdateTimer.Elapsed += ActivationUpdateTimerOnElapsed;

            foreach (Module module in _pluginManagementService.GetFeaturesOfType<Module>())
                InitialiseOrApplyPriority(module);
        }

        private async void ActivationUpdateTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            await UpdateModuleActivation();
        }

        private async Task ActivateModule(Module module)
        {
            try
            {
                module.Activate(false);

                // If this is a profile module, activate the last active profile after module activation
                if (module is ProfileModule profileModule)
                    await _profileService.ActivateLastProfileAnimated(profileModule);
            }
            catch (Exception e)
            {
                _logger.Error(new ArtemisPluginFeatureException(
                        module, "Failed to activate module and last profile.", e), "Failed to activate module and last profile"
                );
                throw;
            }
        }

        private async Task DeactivateModule(Module module)
        {
            try
            {
                // If this is a profile module, animate profile disable
                // module.Deactivate would do the same but without animation
                if (module.IsActivated && module is ProfileModule profileModule)
                    await profileModule.ChangeActiveProfileAnimated(null, null);

                module.Deactivate(false);
            }
            catch (Exception e)
            {
                _logger.Error(new ArtemisPluginFeatureException(
                        module, "Failed to deactivate module and last profile.", e), "Failed to deactivate module and last profile"
                );
                throw;
            }
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

                // If this is a profile module, activate the last active profile after module activation
                if (module is ProfileModule profileModule)
                    _profileService.ActivateLastProfile(profileModule);
            }
            catch (Exception e)
            {
                _logger.Error(new ArtemisPluginFeatureException(
                        module, "Failed to activate module and last profile.", e), "Failed to activate module and last profile"
                );
                throw;
            }
        }

        private void OverrideDeactivate(Module module, bool clearingOverride)
        {
            try
            {
                if (!module.IsActivated)
                    return;

                // If deactivating while it should be activated, its an override
                bool shouldBeActivated = module.EvaluateActivationRequirements();
                // No need to deactivate if it is not in an overridden state
                if (shouldBeActivated && !module.IsActivatedOverride && !clearingOverride)
                    return;

                module.Deactivate(true);
            }
            catch (Exception e)
            {
                _logger.Error(new ArtemisPluginFeatureException(
                        module, "Failed to deactivate module and last profile.", e), "Failed to deactivate module and last profile"
                );
                throw;
            }
        }

        private void OnPluginFeatureEnabled(object? sender, PluginFeatureEventArgs e)
        {
            if (e.PluginFeature is Module module)
                InitialiseOrApplyPriority(module);
        }

        private void InitialiseOrApplyPriority(Module module)
        {
            ModulePriorityCategory category = module.DefaultPriorityCategory;
            int priority = 1;

            module.SettingsEntity = _moduleRepository.GetByModuleId(module.Id);
            if (module.SettingsEntity != null)
            {
                category = (ModulePriorityCategory) module.SettingsEntity.PriorityCategory;
                priority = module.SettingsEntity.Priority;
            }

            UpdateModulePriority(module, category, priority);
        }

        public Module? ActiveModuleOverride { get; private set; }

        public async Task SetActiveModuleOverride(Module? overrideModule)
        {
            try
            {
                await ActiveModuleSemaphore.WaitAsync();

                if (ActiveModuleOverride == overrideModule)
                    return;

                if (overrideModule != null)
                {
                    OverrideActivate(overrideModule);
                    _logger.Information($"Setting active module override to {overrideModule.DisplayName}");
                }
                else
                {
                    _logger.Information("Clearing active module override");
                }

                // Always deactivate all other modules whenever override is called
                List<Module> modules = _pluginManagementService.GetFeaturesOfType<Module>().ToList();
                foreach (Module module in modules.Where(m => m != overrideModule))
                    OverrideDeactivate(module, overrideModule != null);

                ActiveModuleOverride = overrideModule;
            }
            finally
            {
                ActiveModuleSemaphore.Release();
            }
        }

        public async Task UpdateModuleActivation()
        {
            if (ActiveModuleSemaphore.CurrentCount == 0)
                return;

            try
            {
                await ActiveModuleSemaphore.WaitAsync();

                if (ActiveModuleOverride != null)
                {
                    // The conditions of the active module override may be matched, in that case reactivate as a non-override
                    // the principle is different for this service but not for the module
                    bool shouldBeActivated = ActiveModuleOverride.EvaluateActivationRequirements();
                    if (shouldBeActivated && ActiveModuleOverride.IsActivatedOverride)
                        ActiveModuleOverride.Reactivate(true, false);
                    else if (!shouldBeActivated && !ActiveModuleOverride.IsActivatedOverride) ActiveModuleOverride.Reactivate(false, true);

                    return;
                }

                Stopwatch stopwatch = new();
                stopwatch.Start();

                List<Module> modules = _pluginManagementService.GetFeaturesOfType<Module>().ToList();
                List<Task> tasks = new();
                foreach (Module module in modules)
                    lock (module)
                    {
                        bool shouldBeActivated = module.EvaluateActivationRequirements() && module.IsEnabled;
                        if (shouldBeActivated && !module.IsActivated)
                            tasks.Add(ActivateModule(module));
                        else if (!shouldBeActivated && module.IsActivated)
                            tasks.Add(DeactivateModule(module));
                    }

                await Task.WhenAll(tasks);

                stopwatch.Stop();
                if (stopwatch.ElapsedMilliseconds > 100 && !tasks.Any())
                    _logger.Warning("Activation requirements evaluation took too long: {moduleCount} module(s) in {elapsed}", modules.Count, stopwatch.Elapsed);
            }
            finally
            {
                ActiveModuleSemaphore.Release();
            }
        }

        public void UpdateModulePriority(Module module, ModulePriorityCategory category, int priority)
        {
            if (module.PriorityCategory == category && module.Priority == priority)
                return;

            List<Module> modules = _pluginManagementService
                .GetFeaturesOfType<Module>()
                .Where(m => m.PriorityCategory == category)
                .OrderBy(m => m.Priority)
                .ToList();

            if (modules.Contains(module))
                modules.Remove(module);

            priority = Math.Min(modules.Count, Math.Max(0, priority));
            modules.Insert(priority, module);

            module.PriorityCategory = category;
            for (int index = 0; index < modules.Count; index++)
            {
                Module categoryModule = modules[index];
                categoryModule.Priority = index;

                // Don't save modules whose priority hasn't been initialized yet
                if (categoryModule == module || categoryModule.SettingsEntity != null)
                {
                    categoryModule.ApplyToEntity();
                    _moduleRepository.Save(categoryModule.SettingsEntity);
                }
            }

            ModulePriorityUpdated?.Invoke(this, EventArgs.Empty);
        }

        #region Events

        public event EventHandler? ModulePriorityUpdated;

        #endregion
    }
}