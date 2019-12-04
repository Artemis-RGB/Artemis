using System;
using System.Linq;
using System.Reflection;
using Artemis.Core.Models.Profile;
using Artemis.Core.Plugins.Exceptions;
using Artemis.Core.Plugins.LayerElement;
using Artemis.Core.Services.Interfaces;
using Newtonsoft.Json;
using Ninject;
using Ninject.Parameters;

namespace Artemis.Core.Services
{
    public class LayerService : ILayerService
    {
        private readonly IKernel _kernel;

        public LayerService(IKernel kernel)
        {
            _kernel = kernel;
        }

        public LayerElement InstantiateLayerElement(Layer layer, LayerElementDescriptor layerElementDescriptor, string settings, Guid? guid)
        {
            if (guid == null)
                guid = Guid.NewGuid();

            // Determine the settings type declared by the layer element
            object settingsInstance = null;
            var properties = layerElementDescriptor.LayerElementType.GetProperties();
            var settingsType = properties.FirstOrDefault(p => p.Name == "Settings" &&
                                                              p.DeclaringType == layerElementDescriptor.LayerElementType)?.PropertyType;

            // Deserialize the settings if provided, check for null in JSON as well
            if (settings != null && settings != "null")
            {
                // Setting where provided but no settings type was found, something is wrong
                if (settingsType == null)
                {
                    throw new ArtemisPluginException(
                        layerElementDescriptor.LayerElementProvider.PluginInfo,
                        $"Settings where provided but layer element of type {layerElementDescriptor.LayerElementType.Name} has no Settings property."
                    );
                }
                settingsInstance = JsonConvert.DeserializeObject(settings, settingsType);
            }
            // If no settings found, provide a fresh instance of the settings type
            else if (settingsType != null)
            {
                settingsInstance = Activator.CreateInstance(settingsType);
            }

            var arguments = new IParameter[]
            {
                new ConstructorArgument("layer", layer),
                new ConstructorArgument("guid", guid.Value),
                new ConstructorArgument("settings", settingsInstance),
                new ConstructorArgument("descriptor", layerElementDescriptor)
            };
            var layerElement = (LayerElement) _kernel.Get(layerElementDescriptor.LayerElementType, arguments);
            layer.AddLayerElement(layerElement);

            return layerElement;
        }

        public void RemoveLayerElement(Layer layer, LayerElement layerElement)
        {
            layer.RemoveLayerElement(layerElement);
            layerElement.Dispose();
        }
    }
}