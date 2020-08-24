using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Artemis.Core.Events;
using Artemis.Core.Plugins.Modules;
using Artemis.Core.Services.Interfaces;
using Artemis.Core.Services.Storage.Interfaces;
using Artemis.Storage.Repositories.Interfaces;
using Serilog;
using Timer = System.Timers.Timer;

namespace Artemis.Core.Services
{
    internal class ModuleService : IModuleService
    {
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

            PopulatePriorities();
        }

        public bool ApplyingOverride { get; private set; }
        public Module ActiveModuleOverride { get; private set; }

        public async Task SetActiveModuleOverride(Module overrideModule)
        {
            if (ActiveModuleOverride == overrideModule)
                return;

            try
            {
                // Not the cleanest way but locks don't work async and I cba with a mutex
                while (ApplyingOverride)
                    await Task.Delay(50);

                ApplyingOverride = true;
                ActiveModuleOverride = overrideModule;

                // If set to null, resume regular activation
                if (ActiveModuleOverride == null)
                {
                    await UpdateModuleActivation();
                    _logger.Information("Cleared active module override");
                    return;
                }

                // If a module was provided, activate it and deactivate everything else
                var modules = _pluginService.GetPluginsOfType<Module>().ToList();
                var deactivationTasks = new List<Task>();
                foreach (var module in modules)
                {
                    if (module != ActiveModuleOverride)
                        deactivationTasks.Add(DeactivateModule(module, true));
                }

                await Task.WhenAll(deactivationTasks);

                if (!ActiveModuleOverride.IsActivated)
                    await ActivateModule(ActiveModuleOverride, true);

                _logger.Information($"Set active module override to {ActiveModuleOverride.DisplayName}");
            }
            finally
            {
                ApplyingOverride = false;
            }
        }

        public async Task UpdateModuleActivation()
        {
            if (ActiveModuleOverride != null)
                return;

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var modules = _pluginService.GetPluginsOfType<Module>().ToList();
            var tasks = new List<Task>();
            foreach (var module in modules)
            {
                var shouldBeActivated = module.EvaluateActivationRequirements();
                if (shouldBeActivated && !module.IsActivated)
                    tasks.Add(ActivateModule(module, false));
                else if (!shouldBeActivated && module.IsActivated)
                    tasks.Add(DeactivateModule(module, false));
            }

            await Task.WhenAll(tasks);

            stopwatch.Stop();
            if (stopwatch.ElapsedMilliseconds > 100)
                _logger.Warning("Activation requirements evaluation took too long: {moduleCount} module(s) in {elapsed}", modules.Count, stopwatch.Elapsed);
        }

        public void UpdateModulePriority(Module module, ModulePriorityCategory category, int priority)
        {
            var modules = _pluginService.GetPluginsOfType<Module>().Where(m => m.PriorityCategory == category).OrderBy(m => m.Priority).ToList();

            if (modules.Contains(module))
                modules.Remove(module);

            if (modules.Count == 0)
                priority = 1;
            else if (priority < 1)
                priority = 1;
            else if (priority > modules.Count)
                priority = modules.Count;

            module.PriorityCategory = category;
            modules.Insert(priority - 1, module);

            for (var index = 0; index < modules.Count; index++)
            {
                var categoryModule = modules[index];
                categoryModule.Priority = index + 1;
                categoryModule.ApplyToEntity();

                _moduleRepository.Save(categoryModule.Entity);
            }
        }

        private async void ActivationUpdateTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            await UpdateModuleActivation();
        }

        private async Task ActivateModule(Module module, bool isOverride)
        {
            module.Activate(isOverride);

            // If this is a profile module, activate the last active profile after module activation
            if (module is ProfileModule profileModule)
                await _profileService.ActivateLastProfileAnimated(profileModule);
        }

        private async Task DeactivateModule(Module module, bool isOverride)
        {
            // If this is a profile module, activate the last active profile after module activation
            if (module.IsActivated && module is ProfileModule profileModule)
                await profileModule.ChangeActiveProfileAnimated(null, null);

            module.Deactivate(isOverride);
        }

        private void PopulatePriorities()
        {
            var modules = _pluginService.GetPluginsOfType<Module>().ToList();
            var moduleEntities = _moduleRepository.GetAll();

            foreach (var module in modules)
            {
                var entity = moduleEntities.FirstOrDefault(e => e.PluginGuid == module.PluginInfo.Guid);
                if (entity != null)
                {
                    module.Entity = entity;
                    module.PriorityCategory = (ModulePriorityCategory) entity.PriorityCategory;
                    module.Priority = entity.Priority;
                }
            }
        }

        private void PluginServiceOnPluginEnabled(object sender, PluginEventArgs e)
        {
            if (e.PluginInfo.Instance is Module module)
                InitialiseOrApplyPriority(module);
        }

        private void InitialiseOrApplyPriority(Module module)
        {
            var entity = _moduleRepository.GetByPluginGuid(module.PluginInfo.Guid);
            if (entity != null)
            {
                module.Entity = entity;
                module.PriorityCategory = (ModulePriorityCategory) entity.PriorityCategory;
                module.Priority = entity.Priority;
            }
            else
                UpdateModulePriority(module, module.DefaultPriorityCategory, 1);
        }
    }
}