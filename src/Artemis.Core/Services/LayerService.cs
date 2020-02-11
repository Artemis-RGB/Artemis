using System.Collections.Generic;
using System.Linq;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Profile.KeyframeEngines;
using Artemis.Core.Models.Profile.LayerProperties;
using Artemis.Core.Plugins.LayerBrush;
using Artemis.Core.Services.Interfaces;
using Ninject;
using Ninject.Parameters;
using Serilog;

namespace Artemis.Core.Services
{
    public class LayerService : ILayerService
    {
        private readonly IKernel _kernel;
        private readonly ILogger _logger;
        private readonly IPluginService _pluginService;

        public LayerService(IKernel kernel, ILogger logger, IPluginService pluginService)
        {
            _kernel = kernel;
            _logger = logger;
            _pluginService = pluginService;
        }

        public LayerBrush InstantiateLayerBrush(Layer layer)
        {
            RemoveLayerBrush(layer);

            var descriptorReference = layer.BrushReferenceProperty.CurrentValue;
            if (descriptorReference == null)
                return null;

            // Get a matching descriptor
            var layerBrushProviders = _pluginService.GetPluginsOfType<LayerBrushProvider>();
            var descriptors = layerBrushProviders.SelectMany(l => l.LayerBrushDescriptors).ToList();
            var descriptor = descriptors.FirstOrDefault(d => d.LayerBrushProvider.PluginInfo.Guid == descriptorReference.BrushPluginGuid &&
                                                             d.LayerBrushType.Name == descriptorReference.BrushType);

            if (descriptor == null)
                return null;

            var arguments = new IParameter[]
            {
                new ConstructorArgument("layer", layer),
                new ConstructorArgument("descriptor", descriptor)
            };
            var layerElement = (LayerBrush) _kernel.Get(descriptor.LayerBrushType, arguments);
            layer.LayerBrush = layerElement;

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

        public void RemoveLayerBrush(Layer layer)
        {
            if (layer.LayerBrush == null)
                return;

            var brush = layer.LayerBrush;
            layer.LayerBrush = null;

            var propertiesToRemove = layer.Properties.Where(l => l.PluginInfo == brush.Descriptor.LayerBrushProvider.PluginInfo).ToList();
            foreach (var layerProperty in propertiesToRemove)
                layer.RemoveLayerProperty(layerProperty);
            brush.Dispose();
        }
    }
}