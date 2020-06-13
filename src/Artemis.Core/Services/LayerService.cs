using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
using Artemis.Core.Exceptions;
using Artemis.Core.Models.Profile;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.LayerBrush;
using Artemis.Core.Plugins.LayerBrush.Abstract;
using Artemis.Core.Plugins.LayerEffect;
using Artemis.Core.Plugins.LayerEffect.Abstract;
using Artemis.Core.Services.Interfaces;
using Artemis.Storage.Entities.Profile;
using Ninject;
using Ninject.Injection;
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

            // With the properties loaded, the layer brush and effect can be instantiated
            InstantiateLayerBrush(layer);
            InstantiateLayerEffects(layer);

            return layer;
        }

        public BaseLayerBrush InstantiateLayerBrush(Layer layer)
        {
            if (layer.LayerBrush != null)
                throw new ArtemisCoreException("Layer already has an instantiated layer brush");

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
            brush.Initialize(this);
            brush.Update(0);

            layer.LayerBrush = brush;
            layer.OnLayerBrushUpdated();

            return brush;
        }

        public void InstantiateLayerEffects(Layer layer)
        {
            if (layer.LayerEffects.Any())
                throw new ArtemisCoreException("Layer already has instantiated layer effects");

            foreach (var layerEntityLayerEffect in layer.LayerEntity.LayerEffects.OrderByDescending(e => e.Order))
            {
                // Get a matching descriptor
                var layerEffectProviders = _pluginService.GetPluginsOfType<LayerEffectProvider>();
                var descriptors = layerEffectProviders.SelectMany(l => l.LayerEffectDescriptors).ToList();
                var descriptor = descriptors.FirstOrDefault(d => d.LayerEffectProvider.PluginInfo.Guid == layerEntityLayerEffect.PluginGuid &&
                                                                 d.LayerEffectType.Name == layerEntityLayerEffect.EffectType);

                if (descriptor == null)
                    continue;

                var effect = (BaseLayerEffect) _kernel.Get(descriptor.LayerEffectType);
                effect.Layer = layer;
                effect.Order = layerEntityLayerEffect.Order;
                effect.Name = layerEntityLayerEffect.Name;
                effect.Descriptor = descriptor;
                effect.Initialize(this);
                effect.Update(0);

                layer.AddLayerEffect(effect);
                _logger.Debug("Added layer effect with root path {rootPath}", effect.PropertyRootPath);
            }

            layer.OnLayerEffectsUpdated();
        }

        public BaseLayerEffect AddLayerEffect(Layer layer, LayerEffectDescriptor layerEffectDescriptor)
        {
            var effect = (BaseLayerEffect) _kernel.Get(layerEffectDescriptor.LayerEffectType);
            effect.Layer = layer;
            effect.Order = layer.LayerEffects.Count + 1;
            effect.Descriptor = layerEffectDescriptor;

            effect.Initialize(this);
            effect.Update(0);

            layer.AddLayerEffect(effect);
            _logger.Debug("Added layer effect with root path {rootPath}", effect.PropertyRootPath);

            layer.OnLayerEffectsUpdated();
            return effect;
        }

        public void RemoveLayerEffect(BaseLayerEffect layerEffect)
        {
            // // Make sure the group is collapsed or the effect that gets this effect's order gets expanded
            // layerEffect.Layer.SetPropertyGroupExpanded(layerEffect.BaseProperties, false);
            layerEffect.Layer.RemoveLayerEffect(layerEffect);
        }
    }
}