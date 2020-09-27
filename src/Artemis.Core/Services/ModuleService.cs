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
        private static readonly SemaphoreSlim ActiveModuleSemaphore = new SemaphoreSlim(1, 1);
        private readonly ILogger _logger;
        private readonly IModuleRepository _moduleRepository;
        private readonly IPluginService _pluginService;
        private readonly IProfileService _profileService;

        public ModuleService(ILogger logger, IModuleRepository moduleRepository, IPluginService pluginService, IProfileService profileService)
        {
            _logger = logger;
            _moduleRepository = moduleRepository;
            _pluginService = pluginService;
            _profileService = profileService;
            _pluginService.PluginEnabled += PluginServiceOnPluginEnabled;

            var activationUpdateTimer = new Timer(2000);
            activationUpdateTimer.Start();
            activationUpdateTimer.Elapsed += ActivationUpdateTimerOnElapsed;

            foreach (var module in _pluginService.GetPluginsOfType<Module>())
                InitialiseOrApplyPriority(module);
        }

        public Module ActiveModuleOverride { get; private set; }

        public async Task SetActiveModuleOverride(Module overrideModule)
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
                    _logger.Information("Clearing active module override");

                // Always deactivate all other modules whenever override is called
                var modules = _pluginService.GetPluginsOfType<Module>().ToList();
                foreach (var module in modules.Where(m => m != overrideModule))
                    OverrideDeactivate(module);

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
                    var shouldBeActivated = ActiveModuleOverride.EvaluateActivationRequirements();
                    if (shouldBeActivated && ActiveModuleOverride.IsActivatedOverride)
                    {
                        ActiveModuleOverride.Deactivate(true);
                        ActiveModuleOverride.Activate(false);
                    }
                    else if (!shouldBeActivated && !ActiveModuleOverride.IsActivatedOverride)
                    {
                        ActiveModuleOverride.Deactivate(false);
                        ActiveModuleOverride.Activate(true);
                    }

                    return;
                }

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var modules = _pluginService.GetPluginsOfType<Module>().ToList();
                var tasks = new List<Task>();
                foreach (var module in modules)
                {
                    var shouldBeActivated = module.EvaluateActivationRequirements();
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

            var modules = _pluginService.GetPluginsOfType<Module>().Where(m => m.PriorityCategory == category).OrderBy(m => m.Priority).ToList();
            if (modules.Contains(module))
                modules.Remove(module);

            priority = Math.Min(modules.Count, Math.Max(0, priority));
            modules.Insert(priority, module);

            module.PriorityCategory = category;
            for (var index = 0; index < modules.Count; index++)
            {
                var categoryModule = modules[index];
                categoryModule.Priority = index;

                // Don't save modules whose priority hasn't been initialized yet
                if (categoryModule == module || categoryModule.Entity != null)
                {
                    categoryModule.ApplyToEntity();
                    _moduleRepository.Save(categoryModule.Entity);
                }
            }
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
                _logger.Error(new ArtemisPluginException(module.PluginInfo, "Failed to activate module and last profile.", e), "Failed to activate module and last profile");
                throw;
            }
        }

        private void OverrideActivate(Module module)
        {
            try
            {
                if (module.IsActivated)
                    return;

                module.Activate(true);

                // If this is a profile module, activate the last active profile after module activation
                if (module is ProfileModule profileModule)
                    _profileService.ActivateLastProfile(profileModule);
            }
            catch (Exception e)
            {
                _logger.Error(new ArtemisPluginException(module.PluginInfo, "Failed to activate module and last profile.", e), "Failed to activate module and last profile");
                throw;
            }
        }

        private async Task DeactivateModule(Module module)
        {
            try
            {
                // If this is a profile module, activate the last active profile after module activation
                if (module.IsActivated && module is ProfileModule profileModule)
                    await profileModule.ChangeActiveProfileAnimated(null, null);

                module.Deactivate(false);
            }
            catch (Exception e)
            {
                _logger.Error(new ArtemisPluginException(module.PluginInfo, "Failed to deactivate module and last profile.", e), "Failed to deactivate module and last profile");
                throw;
            }
        }

        private void OverrideDeactivate(Module module)
        {
            try
            {
                if (!module.IsActivated)
                    return;

                // If this is a profile module, activate the last active profile after module activation
                if (module is ProfileModule profileModule)
                    profileModule.ChangeActiveProfile(null, null);

                module.Deactivate(true);
            }
            catch (Exception e)
            {
                _logger.Error(new ArtemisPluginException(module.PluginInfo, "Failed to deactivate module and last profile.", e), "Failed to deactivate module and last profile");
                throw;
            }
        }

        private void PluginServiceOnPluginEnabled(object sender, PluginEventArgs e)
        {
            if (e.PluginInfo.Instance is Module module)
                InitialiseOrApplyPriority(module);
        }

        private void InitialiseOrApplyPriority(Module module)
        {
            var category = module.DefaultPriorityCategory;
            var priority = 1;

            module.Entity = _moduleRepository.GetByPluginGuid(module.PluginInfo.Guid);
            if (module.Entity != null)
            {
                category = (ModulePriorityCategory) module.Entity.PriorityCategory;
                priority = module.Entity.Priority;
            }

            UpdateModulePriority(module, category, priority);
        }
    }
}