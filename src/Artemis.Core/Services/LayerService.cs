using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Profile.KeyframeEngines;
using Artemis.Core.Models.Profile.LayerProperties;
using Artemis.Core.Plugins.Exceptions;
using Artemis.Core.Plugins.LayerBrush;
using Artemis.Core.Services.Interfaces;
using Newtonsoft.Json;
using Ninject;
using Ninject.Parameters;
using Serilog;

namespace Artemis.Core.Services
{
    public class LayerService : ILayerService
    {
        private readonly IKernel _kernel;
        private readonly ILogger _logger;

        public LayerService(IKernel kernel, ILogger logger)
        {
            _kernel = kernel;
            _logger = logger;
        }

        public LayerBrush InstantiateLayerBrush(Layer layer, LayerBrushDescriptor brushDescriptor, string settings)
        {
            // Determine the settings type declared by the layer element
            object settingsInstance = null;
            var properties = brushDescriptor.LayerBrushType.GetProperties();
            var settingsType = properties.FirstOrDefault(p => p.Name == "Settings" &&
                                                              p.DeclaringType == brushDescriptor.LayerBrushType)?.PropertyType;

            // Deserialize the settings if provided, check for null in JSON as well
            if (settings != null && settings != "null")
            {
                // Setting where provided but no settings type was found, something is wrong
                if (settingsType == null)
                {
                    throw new ArtemisPluginException(
                        brushDescriptor.LayerBrushProvider.PluginInfo,
                        $"Settings where provided but layer element of type {brushDescriptor.LayerBrushType.Name} has no Settings property."
                    );
                }

                try
                {
                    settingsInstance = JsonConvert.DeserializeObject(settings, settingsType);
                }
                catch (JsonSerializationException e)
                {
                    _logger.Warning(e, "Failed to deserialize settings for layer type {type}, resetting element settings - Plugin info: {pluginInfo}",
                        brushDescriptor.LayerBrushType.Name,
                        brushDescriptor.LayerBrushProvider.PluginInfo);

                    settingsInstance = Activator.CreateInstance(settingsType);
                }
            }
            // If no settings found, provide a fresh instance of the settings type
            else if (settingsType != null)
            {
                settingsInstance = Activator.CreateInstance(settingsType);
            }

            var arguments = new IParameter[]
            {
                new ConstructorArgument("layer", layer),
                new ConstructorArgument("settings", settingsInstance),
                new ConstructorArgument("descriptor", brushDescriptor)
            };
            var layerElement = (LayerBrush) _kernel.Get(brushDescriptor.LayerBrushType, arguments);
            layer.LayerBrush = (layerElement);

            return layerElement;
        }

        public KeyframeEngine InstantiateKeyframeEngine<T>(LayerProperty<T> layerProperty)
        {
            return InstantiateKeyframeEngine((BaseLayerProperty) layerProperty);
        }

        public KeyframeEngine InstantiateKeyframeEngine(BaseLayerProperty layerProperty)
        {
            // This creates an instance of each keyframe engine, which is pretty cheap since all the expensive stuff is done during
            // Initialize() call but it's not ideal
            var keyframeEngines = _kernel.Get<List<KeyframeEngine>>();
            var keyframeEngine = keyframeEngines.FirstOrDefault(k => k.CompatibleTypes.Contains(layerProperty.Type));
            if (keyframeEngine == null)
                return null;

            keyframeEngine.Initialize(layerProperty);
            return keyframeEngine;
        }

        public void RemoveLayerBrush(Layer layer, LayerBrush layerElement)
        {
            var brush = layer.LayerBrush;
            layer.LayerBrush = null;
            brush.Dispose();
        }
    }
}