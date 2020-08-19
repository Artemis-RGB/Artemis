using System.Diagnostics;
using System.Linq;
using System.Timers;
using Artemis.Core.Events;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Services.Interfaces;
using Artemis.Core.Services.Storage.Interfaces;
using Artemis.Storage.Repositories.Interfaces;
using Serilog;

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
            activationUpdateTimer.Elapsed += ActivationUpdateTimerOnElapsed;
            activationUpdateTimer.Start();
            PopulatePriorities();
        }

        private void ActivationUpdateTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            UpdateModuleActivation();
        }

        public void UpdateModuleActivation()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var modules = _pluginService.GetPluginsOfType<Module>().ToList();
            foreach (var module in modules)
            {
                var shouldBeActivated = module.EvaluateActivationRequirements();
                if (shouldBeActivated && !module.IsActivated)
                {
                    module.Activate();
                    // If this is a profile module, activate the last active profile after module activation
                    if (module is ProfileModule profileModule)
                        _profileService.ActivateLastProfile(profileModule);
                }
                else if (!shouldBeActivated && module.IsActivated)
                    module.Deactivate();
            }

            stopwatch.Stop();
            if (stopwatch.ElapsedMilliseconds > 100)
                _logger.Warning("Activation requirements evaluation took too long: {moduleCount} module(s) in {elapsed}", modules.Count, stopwatch.Elapsed);
        }

        public void PopulatePriorities()
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

    public interface IModuleService : IArtemisService
    {
    }
}