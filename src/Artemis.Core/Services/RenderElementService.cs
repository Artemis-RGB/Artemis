﻿using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core.Exceptions;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Profile.Conditions;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.LayerBrush.Abstract;
using Artemis.Core.Plugins.LayerEffect;
using Artemis.Core.Plugins.LayerEffect.Abstract;
using Artemis.Core.Services.Interfaces;
using Artemis.Storage.Entities.Profile;
using Ninject;
using Serilog;

namespace Artemis.Core.Services
{
    public class RenderElementService : IRenderElementService
    {
        private readonly IKernel _kernel;
        private readonly ILogger _logger;
        private readonly IPluginService _pluginService;
        private readonly IDataModelService _dataModelService;

        public RenderElementService(IKernel kernel, ILogger logger, IPluginService pluginService, IDataModelService dataModelService)
        {
            _kernel = kernel;
            _logger = logger;
            _pluginService = pluginService;
            _dataModelService = dataModelService;
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
            InstantiateDisplayConditions(layer);
            return layer;
        }

        public void RemoveLayerBrush(Layer layer)
        {
            layer.RemoveLayerBrush();
            layer.OnLayerBrushUpdated();
        }

        public void DeactivateLayerBrush(Layer layer)
        {
            layer.DeactivateLayerBrush();
            layer.OnLayerBrushUpdated();
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

        public BaseLayerEffect AddLayerEffect(RenderProfileElement renderElement, LayerEffectDescriptor layerEffectDescriptor)
        {
            // Create the effect with dependency injection
            var effect = (BaseLayerEffect) _kernel.Get(layerEffectDescriptor.LayerEffectType);

            effect.ProfileElement = renderElement;
            effect.EntityId = Guid.NewGuid();
            effect.Enabled = true;
            effect.Order = renderElement.LayerEffects.Count + 1;
            effect.Descriptor = layerEffectDescriptor;

            effect.Initialize(this);
            effect.Update(0);

            renderElement.AddLayerEffect(effect);
            _logger.Debug("Added layer effect with root path {rootPath}", effect.PropertyRootPath);
            return effect;
        }

        public void RemoveLayerEffect(BaseLayerEffect layerEffect)
        {
            layerEffect.ProfileElement.RemoveLayerEffect(layerEffect);
        }

        public void InstantiateLayerEffects(RenderProfileElement renderElement)
        {
            var layerEffectProviders = _pluginService.GetPluginsOfType<LayerEffectProvider>();
            var descriptors = layerEffectProviders.SelectMany(l => l.LayerEffectDescriptors).ToList();
            var entities = renderElement.RenderElementEntity.LayerEffects.OrderByDescending(e => e.Order).ToList();

            foreach (var layerEffectEntity in entities)
            {
                // Skip effects already on the element
                if (renderElement.LayerEffects.Any(e => e.EntityId == layerEffectEntity.Id))
                    continue;

                // Get a matching descriptor
                var descriptor = descriptors.FirstOrDefault(d => d.LayerEffectProvider.PluginInfo.Guid == layerEffectEntity.PluginGuid &&
                                                                 d.LayerEffectType.Name == layerEffectEntity.EffectType);
                if (descriptor == null)
                    continue;

                // Create the effect with dependency injection
                var effect = (BaseLayerEffect) _kernel.Get(descriptor.LayerEffectType);

                effect.ProfileElement = renderElement;
                effect.EntityId = layerEffectEntity.Id;
                effect.Order = layerEffectEntity.Order;
                effect.Name = layerEffectEntity.Name;
                effect.Enabled = layerEffectEntity.Enabled;
                effect.Descriptor = descriptor;

                effect.Initialize(this);
                effect.Update(0);

                renderElement.AddLayerEffect(effect);
                _logger.Debug("Instantiated layer effect with root path {rootPath}", effect.PropertyRootPath);
            }
        }

        public void InstantiateDisplayConditions(RenderProfileElement renderElement)
        {
            var displayCondition = renderElement.RenderElementEntity.RootDisplayCondition != null
                ? new DisplayConditionGroup(null, renderElement.RenderElementEntity.RootDisplayCondition)
                : new DisplayConditionGroup(null);

            displayCondition.Initialize(_dataModelService);
            renderElement.DisplayConditionGroup = displayCondition;
        }
    }
}