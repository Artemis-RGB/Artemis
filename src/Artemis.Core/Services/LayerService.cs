using System.Collections.Generic;
using System.Linq;
using Artemis.Core.Exceptions;
using Artemis.Core.Models.Profile;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.LayerBrush;
using Artemis.Core.Plugins.LayerBrush.Abstract;
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

        public Layer CreateLayer(Profile profile, ProfileElement parent, string name)
        {
            var layer = new Layer(profile, parent, name);
            parent.AddChild(layer);

            // Layers have two hardcoded property groups, instantiate them
            layer.General.InitializeProperties(this, layer, "General.");
            layer.Transform.InitializeProperties(this, layer, "Transform.");

            // With the properties loaded, the layer brush can be instantiated
            InstantiateLayerBrush(layer);

            return layer;
        }

        public BaseLayerBrush InstantiateLayerBrush(Layer layer)
        {
            layer.DeactivateLayerBrush();

            var descriptorReference = layer.General.BrushReference?.CurrentValue;
            if (descriptorReference == null)
                return null;

            // Get a matching descriptor
            var layerBrushProviders = _pluginService.GetPluginsOfType<LayerBrushProvider>();
            var descriptors = layerBrushProviders.SelectMany(l => l.LayerBrushDescriptors).ToList();
            var descriptor = descriptors.FirstOrDefault(d => d.LayerBrushProvider.PluginInfo.Guid == descriptorReference.BrushPluginGuid &&
                                                             d.LayerBrushType.Name == descriptorReference.BrushType);

            if (descriptor == null)
                return null;

            var brush = (BaseLayerBrush) _kernel.Get(descriptor.LayerBrushType);
            brush.Layer = layer;
            brush.Descriptor = descriptor;
            layer.LayerBrush = brush;

            brush.Initialize(this);
            brush.Update(0);
            layer.OnLayerBrushUpdated();

            return brush;
        }
    }
}