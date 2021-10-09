using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Artemis.Core.ScriptingProviders;
using Ninject;
using Ninject.Parameters;
using Serilog;

namespace Artemis.Core.Services
{
    internal class ScriptingService : IScriptingService
    {
        private readonly ILogger _logger;
        private readonly IPluginManagementService _pluginManagementService;
        private readonly IProfileService _profileService;
        private List<ScriptingProvider> _scriptingProviders;

        public ScriptingService(ILogger logger, IPluginManagementService pluginManagementService, IProfileService profileService)
        {
            _logger = logger;
            _pluginManagementService = pluginManagementService;
            _profileService = profileService;

            InternalGlobalScripts = new List<GlobalScript>();
            GlobalScripts = new ReadOnlyCollection<GlobalScript>(InternalGlobalScripts);

            _pluginManagementService.PluginFeatureEnabled += PluginManagementServiceOnPluginFeatureToggled;
            _pluginManagementService.PluginFeatureDisabled += PluginManagementServiceOnPluginFeatureToggled;
            _scriptingProviders = _pluginManagementService.GetFeaturesOfType<ScriptingProvider>();

            // No need to sub to Deactivated, scripts will deactivate themselves
            profileService.ProfileActivated += ProfileServiceOnProfileActivated;

            foreach (ProfileConfiguration profileConfiguration in _profileService.ProfileConfigurations)
            {
                if (profileConfiguration.Profile != null)
                    InitializeProfileScripts(profileConfiguration.Profile);
            }
        }

        internal List<GlobalScript> InternalGlobalScripts { get; }

        private ConstructorArgument CreateScriptConstructorArgument(Type scriptType, object value)
        {
            // Limit to one constructor, there's no need to have more and it complicates things anyway
            ConstructorInfo[] constructors = scriptType.GetConstructors();
            if (constructors.Length != 1)
                throw new ArtemisCoreException("Scripts must have exactly one constructor");

            // Find the ScriptConfiguration parameter, it is required by the base constructor so its there for sure
            ParameterInfo configurationParameter = constructors.First().GetParameters().First(p => value.GetType().IsAssignableFrom(p.ParameterType));

            if (configurationParameter.Name == null)
                throw new ArtemisCoreException($"Couldn't find a valid constructor argument on {scriptType.Name} with type {value.GetType().Name}");
            return new ConstructorArgument(configurationParameter.Name, value);
        }

        private void PluginManagementServiceOnPluginFeatureToggled(object? sender, PluginFeatureEventArgs e)
        {
            _scriptingProviders = _pluginManagementService.GetFeaturesOfType<ScriptingProvider>();

            foreach (ProfileConfiguration profileConfiguration in _profileService.ProfileConfigurations)
            {
                if (profileConfiguration.Profile != null)
                    InitializeProfileScripts(profileConfiguration.Profile);
            }
        }

        private void ProfileServiceOnProfileActivated(object? sender, ProfileConfigurationEventArgs e)
        {
            if (e.ProfileConfiguration.Profile != null)
                InitializeProfileScripts(e.ProfileConfiguration.Profile);
        }

        private void InitializeProfileScripts(Profile profile)
        {
            // Initialize the scripts on the profile
            foreach (ScriptConfiguration scriptConfiguration in profile.ScriptConfigurations.Where(c => c.Script == null))
                CreateScriptInstance(profile, scriptConfiguration);
        }

        public ReadOnlyCollection<GlobalScript> GlobalScripts { get; }

        public GlobalScript? CreateScriptInstance(ScriptConfiguration scriptConfiguration)
        {
            GlobalScript? script = null;
            try
            {
                if (scriptConfiguration.Script != null)
                    throw new ArtemisCoreException("The provided script configuration already has an active script");

                ScriptingProvider? provider = _scriptingProviders.FirstOrDefault(p => p.Id == scriptConfiguration.ScriptingProviderId);
                if (provider == null)
                    return null;

                script = (GlobalScript) provider.Plugin.Kernel!.Get(
                    provider.GlobalScriptType,
                    CreateScriptConstructorArgument(provider.GlobalScriptType, scriptConfiguration)
                );

                script.ScriptingProvider = provider;
                script.ScriptingService = this;
                provider.InternalScripts.Add(script);
                InternalGlobalScripts.Add(script);

                scriptConfiguration.Script = script;
                return script;
            }
            catch (Exception e)
            {
                _logger.Warning(e, "Failed to initialize global script");
                script?.Dispose();
                return null;
            }
        }

        public ProfileScript? CreateScriptInstance(Profile profile, ScriptConfiguration scriptConfiguration)
        {
            ProfileScript? script = null;
            try
            {
                if (scriptConfiguration.Script != null)
                    throw new ArtemisCoreException("The provided script configuration already has an active script");

                ScriptingProvider? provider = _scriptingProviders.FirstOrDefault(p => p.Id == scriptConfiguration.ScriptingProviderId);
                if (provider == null)
                    return null;

                script = (ProfileScript) provider.Plugin.Kernel!.Get(
                    provider.ProfileScriptType,
                    CreateScriptConstructorArgument(provider.ProfileScriptType, profile),
                    CreateScriptConstructorArgument(provider.ProfileScriptType, scriptConfiguration)
                );

                script.ScriptingProvider = provider;
                provider.InternalScripts.Add(script);
                lock (profile)
                {
                    scriptConfiguration.Script = script;
                    profile.Scripts.Add(script);
                }

                return script;
            }
            catch (Exception e)
            {
                _logger.Warning(e, "Failed to initialize profile script");
                script?.Dispose();
                return null;
            }
        }

        /// <inheritdoc />
        public void DeleteScript(ScriptConfiguration scriptConfiguration)
        {
        }
    }
}