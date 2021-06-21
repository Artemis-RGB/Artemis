using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Artemis.Core.ScriptingProviders;
using Ninject;
using Ninject.Parameters;

namespace Artemis.Core.Services
{
    internal class ScriptingService : IScriptingService
    {
        private readonly IPluginManagementService _pluginManagementService;
        private List<ScriptingProvider> _scriptingProviders;

        public ScriptingService(IPluginManagementService pluginManagementService, IProfileService profileService)
        {
            _pluginManagementService = pluginManagementService;

            InternalGlobalScripts = new List<GlobalScript>();

            _pluginManagementService.PluginFeatureEnabled += PluginManagementServiceOnPluginFeatureToggled;
            _pluginManagementService.PluginFeatureDisabled += PluginManagementServiceOnPluginFeatureToggled;
            _scriptingProviders = _pluginManagementService.GetFeaturesOfType<ScriptingProvider>();

            // No need to sub to Deactivated, scripts will deactivate themselves
            profileService.ProfileActivated += ProfileServiceOnProfileActivated;

            foreach (ProfileConfiguration profileConfiguration in profileService.ProfileConfigurations)
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

            foreach (Layer layer in profile.GetAllLayers())
            {
                // Initialize the scripts on the layers
                foreach (ScriptConfiguration scriptConfiguration in layer.ScriptConfigurations.Where(c => c.Script == null))
                    CreateScriptInstance(layer, scriptConfiguration);

                // Initialize the scripts on the layer properties of layers
                foreach (ILayerProperty layerProperty in layer.GetAllLayerProperties())
                {
                    foreach (ScriptConfiguration scriptConfiguration in layerProperty.ScriptConfigurations.Where(c => c.Script == null))
                        CreateScriptInstance(layerProperty, scriptConfiguration);
                }
            }

            foreach (Folder folder in profile.GetAllFolders())
            {
                // Initialize the scripts on the layer properties of folders
                foreach (ILayerProperty layerProperty in folder.GetAllLayerProperties())
                {
                    foreach (ScriptConfiguration scriptConfiguration in layerProperty.ScriptConfigurations.Where(c => c.Script == null))
                        CreateScriptInstance(layerProperty, scriptConfiguration);
                }
            }
        }

        public ReadOnlyCollection<GlobalScript> GlobalScripts => InternalGlobalScripts.AsReadOnly();

        public GlobalScript? CreateScriptInstance(ScriptConfiguration scriptConfiguration)
        {
            if (scriptConfiguration.Script != null)
                throw new ArtemisCoreException("The provided script configuration already has an active script");

            ScriptingProvider? provider = _scriptingProviders.FirstOrDefault(p => p.Id == scriptConfiguration.ScriptingProviderId);
            if (provider == null)
                return null;

            GlobalScript script = (GlobalScript) provider.Plugin.Kernel!.Get(
                provider.GlobalScriptType,
                CreateScriptConstructorArgument(provider.GlobalScriptType, scriptConfiguration)
            );

            script.ScriptingProvider = provider;
            script.ScriptingService = this;
            provider.InternalScripts.Add(script);
            InternalGlobalScripts.Add(script);
            return script;
        }

        public ProfileScript? CreateScriptInstance(Profile profile, ScriptConfiguration scriptConfiguration)
        {
            if (scriptConfiguration.Script != null)
                throw new ArtemisCoreException("The provided script configuration already has an active script");

            ScriptingProvider? provider = _scriptingProviders.FirstOrDefault(p => p.Id == scriptConfiguration.ScriptingProviderId);
            if (provider == null)
                return null;

            ProfileScript script = (ProfileScript) provider.Plugin.Kernel!.Get(
                provider.ProfileScriptType,
                CreateScriptConstructorArgument(provider.ProfileScriptType, profile),
                CreateScriptConstructorArgument(provider.ProfileScriptType, scriptConfiguration)
            );

            script.ScriptingProvider = provider;
            provider.InternalScripts.Add(script);
            return script;
        }

        public LayerScript? CreateScriptInstance(Layer layer, ScriptConfiguration scriptConfiguration)
        {
            if (scriptConfiguration.Script != null)
                throw new ArtemisCoreException("The provided script configuration already has an active script");

            ScriptingProvider? provider = _scriptingProviders.FirstOrDefault(p => p.Id == scriptConfiguration.ScriptingProviderId);
            if (provider == null)
                return null;

            LayerScript script = (LayerScript) provider.Plugin.Kernel!.Get(
                provider.LayerScriptType,
                CreateScriptConstructorArgument(provider.LayerScriptType, layer),
                CreateScriptConstructorArgument(provider.LayerScriptType, scriptConfiguration)
            );

            script.ScriptingProvider = provider;
            provider.InternalScripts.Add(script);
            return script;
        }

        public PropertyScript? CreateScriptInstance(ILayerProperty layerProperty, ScriptConfiguration scriptConfiguration)
        {
            if (scriptConfiguration.Script != null)
                throw new ArtemisCoreException("The provided script configuration already has an active script");

            ScriptingProvider? provider = _scriptingProviders.FirstOrDefault(p => p.Id == scriptConfiguration.ScriptingProviderId);
            if (provider == null)
                return null;

            PropertyScript script = (PropertyScript) provider.Plugin.Kernel!.Get(
                provider.PropertyScriptType,
                CreateScriptConstructorArgument(provider.PropertyScriptType, layerProperty),
                CreateScriptConstructorArgument(provider.PropertyScriptType, scriptConfiguration)
            );

            script.ScriptingProvider = provider;
            provider.InternalScripts.Add(script);
            return script;
        }

        /// <inheritdoc />
        public void DeleteScript(ScriptConfiguration scriptConfiguration)
        {
        }
    }
}