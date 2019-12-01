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

        public LayerElement InstantiateLayerElement(Layer layer, LayerElementDescriptor layerElementDescriptor, string settings)
        {
            // Deserialize the settings, if provided
            object deserializedSettings = null;
            if (settings != null)
            {
                var settingsType = layerElementDescriptor.LayerElementType.GetProperty(nameof(LayerElement.Settings))?.PropertyType;
                if (settingsType == null)
                {
                    throw new ArtemisPluginException(
                        layerElementDescriptor.LayerElementProvider.PluginInfo,
                        $"Layer element of type {layerElementDescriptor.LayerElementType} has no Settings property."
                    );
                }

                deserializedSettings = JsonConvert.DeserializeObject(settings, settingsType);
            }

            var arguments = new IParameter[]
            {
                new ConstructorArgument("layer", layer),
                new ConstructorArgument("settings", deserializedSettings),
                new ConstructorArgument("descriptor", layerElementDescriptor)
            };
            var layerElement = (LayerElement) _kernel.Get(layerElementDescriptor.LayerElementType, arguments);
            layer.AddLayerElement(layerElement);

            return layerElement;
        }
    }
}