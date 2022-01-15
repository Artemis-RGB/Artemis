using System;
using System.Linq;
using Artemis.Core.LayerEffects.Placeholder;
using Artemis.Storage.Entities.Profile;
using Ninject;

namespace Artemis.Core.LayerEffects
{
    /// <summary>
    ///     A class that describes a layer effect
    /// </summary>
    public class LayerEffectDescriptor
    {
        internal LayerEffectDescriptor(string displayName, string description, string icon, Type? layerEffectType, LayerEffectProvider provider)
        {
            DisplayName = displayName;
            Description = description;
            Icon = icon;
            LayerEffectType = layerEffectType;
            Provider = provider;
        }

        /// <summary>
        ///     The name that is displayed in the UI
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        ///     The description that is displayed in the UI
        /// </summary>
        public string Description { get; }

        /// <summary>
        ///     The Material icon to display in the UI, a full reference can be found
        ///     <see href="https://materialdesignicons.com">here</see>
        /// </summary>
        public string Icon { get; }

        /// <summary>
        ///     The type of the layer effect
        /// </summary>
        public Type? LayerEffectType { get; }

        /// <summary>
        ///     The plugin that provided this <see cref="LayerEffectDescriptor" />
        /// </summary>
        public LayerEffectProvider Provider { get; }

        /// <summary>
        ///     Gets the GUID this descriptor is acting as a placeholder for. If null, this descriptor is not a placeholder
        /// </summary>
        public string? PlaceholderFor { get; internal set; }

        /// <summary>
        ///     Creates an instance of the described effect and applies it to the render element
        /// </summary>
        internal void CreateInstance(RenderProfileElement renderElement, LayerEffectEntity? entity)
        {
            if (LayerEffectType == null)
                throw new ArtemisCoreException("Cannot create an instance of a layer effect because this descriptor is not a placeholder but is still missing its LayerEffectType");

            if (entity == null)
            {
                entity = new LayerEffectEntity
                {
                    Id = Guid.NewGuid(),
                    Suspended = false,
                    Order = renderElement.LayerEffects.Count + 1,
                    ProviderId = Provider.Id,
                    EffectType = LayerEffectType.FullName
                };
            }
            else
            {
                // Skip effects already on the element
                if (renderElement.LayerEffects.Any(e => e.LayerEffectEntity.Id == entity.Id))
                    return;
            }

            if (PlaceholderFor != null)
            {
                CreatePlaceHolderInstance(renderElement, entity);
                return;
            }

            BaseLayerEffect effect = (BaseLayerEffect) Provider.Plugin.Kernel!.Get(LayerEffectType);
            effect.ProfileElement = renderElement;
            effect.LayerEffectEntity = entity;
            effect.Order = entity.Order;
            effect.Name = entity.Name;
            effect.Suspended = entity.Suspended;
            effect.Descriptor = this;

            effect.Initialize();
            effect.Update(0);

            renderElement.ActivateLayerEffect(effect);
        }

        private void CreatePlaceHolderInstance(RenderProfileElement renderElement, LayerEffectEntity entity)
        {
            if (PlaceholderFor == null)
                throw new ArtemisCoreException("Cannot create a placeholder instance using a layer effect descriptor that is not a placeholder for anything");
            PlaceholderLayerEffect effect = new(entity, PlaceholderFor)
            {
                ProfileElement = renderElement,
                Descriptor = this
            };
            effect.Initialize();
            renderElement.ActivateLayerEffect(effect);

            if (renderElement.ShouldBeEnabled)
                effect.InternalEnable();
        }
    }
}