using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Artemis.Core.Modules;
using Newtonsoft.Json;
using Serilog;

namespace Artemis.Core.Services;

internal class ModuleService : IModuleService
{
    private readonly Timer _activationUpdateTimer;
    private readonly ILogger _logger;
    private readonly List<Module> _modules;
    private readonly IProfileService _profileService;
    private readonly object _updateLock = new();

    private Module? _activationOverride;

    public ModuleService(ILogger logger, IPluginManagementService pluginManagementService, IProfileService profileService)
    {
        _logger = logger;
        _profileService = profileService;

        _activationUpdateTimer = new Timer(2000);
        _activationUpdateTimer.Start();
        _activationUpdateTimer.Elapsed += ActivationUpdateTimerOnElapsed;

        pluginManagementService.PluginFeatureEnabled += PluginManagementServiceOnPluginFeatureEnabled;
        pluginManagementService.PluginFeatureDisabled += PluginManagementServiceOnPluginFeatureDisabled;
        _modules = pluginManagementService.GetFeaturesOfType<Module>().ToList();

        Task.Run(ImportDefaultProfiles);
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

    private void ActivationUpdateTimerOnElapsed(object? sender, ElapsedEventArgs e)
    {
        UpdateModuleActivation();
    }

    private void PluginManagementServiceOnPluginFeatureEnabled(object? sender, PluginFeatureEventArgs e)
    {
        lock (_updateLock)
        {
            if (e.PluginFeature is Module module && !_modules.Contains(module))
            {
                Task.Run(() => ImportDefaultProfiles(module));
                _modules.Add(module);
            }
        }
    }

    private void PluginManagementServiceOnPluginFeatureDisabled(object? sender, PluginFeatureEventArgs e)
    {
        lock (_updateLock)
        {
            if (e.PluginFeature is Module module)
                _modules.Remove(module);
        }
    }

    private async Task ImportDefaultProfiles()
    {
        foreach (Module module in _modules)
            await ImportDefaultProfiles(module);
    }
    
    private async Task ImportDefaultProfiles(Module module)
    {
        try
        {
            foreach ((DefaultCategoryName categoryName, string profilePath) in module.DefaultProfilePaths)
            {
                ProfileCategory category = _profileService.ProfileCategories.FirstOrDefault(c => c.Name == categoryName.ToString()) ?? _profileService.CreateProfileCategory(categoryName.ToString());
                await using FileStream fileStream = File.OpenRead(profilePath);
                await _profileService.ImportProfile(fileStream, category, false, true, null);
            }
        }
        catch (Exception e)
        {
            _logger.Warning(e, "Failed to import default profiles for module {module}", module);
        }
    }

    public void UpdateModuleActivation()
    {
        lock (_updateLock)
        {
            try
            {
                _activationUpdateTimer.Elapsed -= ActivationUpdateTimerOnElapsed;
                foreach (Module module in _modules)
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
            foreach (Module module in _modules)
                module.InternalUpdate(deltaTime);
        }
    }

    public event EventHandler<ModuleEventArgs>? ModuleActivated;
    public event EventHandler<ModuleEventArgs>? ModuleDeactivated;
}