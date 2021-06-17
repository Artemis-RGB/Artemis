using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Artemis.Core.ScriptingProviders;

namespace Artemis.Core.Services
{
    internal class ScriptingService : IScriptingService
    {
        private readonly IPluginManagementService _pluginManagementService;
        public List<GlobalScript> GlobalScripts { get; }

        public ScriptingService(IPluginManagementService pluginManagementService, IProfileService profileService)
        {
            _pluginManagementService = pluginManagementService;

            GlobalScripts = new List<GlobalScript>();

            // No need to sub to Deactivated, scripts will deactivate themselves
            profileService.ProfileActivated += ProfileServiceOnProfileActivated;
        }

        private void ProfileServiceOnProfileActivated(object? sender, ProfileConfigurationEventArgs e)
        {
            if (e.ProfileConfiguration.Profile != null)
                InitializeProfileScripts(e.ProfileConfiguration.Profile);
        }

        private void InitializeProfileScripts(Profile profile)
        {
            List<ScriptingProvider> providers = _pluginManagementService.GetFeaturesOfType<ScriptingProvider>();

            // Initialize the scripts on the profile
            foreach (ProfileScript script in profile.Scripts) 
                InitializeScript(profile, script, providers);

            foreach (Layer layer in profile.GetAllLayers())
            {
                // Initialize the scripts on the layers
                foreach (LayerScript script in layer.Scripts)
                    InitializeScript(layer, script, providers);

                // Initialize the scripts on the layer properties of layers
                foreach (ILayerProperty layerProperty in layer.GetAllLayerProperties())
                {
                    foreach (PropertyScript script in layerProperty.Scripts) 
                        InitializeScript(layerProperty, script, providers);
                }
                   
            }

            foreach (Folder folder in profile.GetAllFolders())
            {
                // Initialize the scripts on the layer properties of folders
                foreach (ILayerProperty layerProperty in folder.GetAllLayerProperties())
                {
                    foreach (PropertyScript script in layerProperty.Scripts)
                        InitializeScript(layerProperty, script, providers);
                }
            }
        }

        private void InitializeScript(Profile profile, ProfileScript script, List<ScriptingProvider> providers)
        {
            if (script.ScriptingProvider != null)
                return;

            script.Profile = profile;
            // Find a matching provider

            // Initialize with the provider
        }

        private void InitializeScript(Layer layer, LayerScript script, List<ScriptingProvider> providers)
        {
            if (script.ScriptingProvider != null)
                return;

            script.Layer = layer;
            // Find a matching provider

            // Initialize with the provider
        }

        private void InitializeScript(ILayerProperty layerProperty, PropertyScript script, List<ScriptingProvider> providers)
        {
            if (script.ScriptingProvider != null)
                return;

            script.LayerProperty = layerProperty;
            // Find a matching provider

            // Initialize with the provider
        }
    }

    public interface IScriptingService : IArtemisService
    {
        List<GlobalScript> GlobalScripts { get; }
    }
}