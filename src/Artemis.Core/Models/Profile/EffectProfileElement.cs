using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Core.Annotations;
using Artemis.Core.Plugins.LayerEffect.Abstract;
using Artemis.Storage.Entities.Profile;
using SkiaSharp;

namespace Artemis.Core.Models.Profile
{
    public abstract class EffectProfileElement : PropertiesProfileElement
    {
        internal abstract EffectsEntity EffectsEntity { get; }

        protected List<BaseLayerEffect> _layerEffects;
        
        /// <summary>
        ///     Gets a read-only collection of the layer effects on this entity
        /// </summary>
        public ReadOnlyCollection<BaseLayerEffect> LayerEffects => _layerEffects.AsReadOnly();
        
        internal void RemoveLayerEffect([NotNull] BaseLayerEffect effect)
        {
            if (effect == null) throw new ArgumentNullException(nameof(effect));

            DeactivateLayerEffect(effect);

            // Update the order on the remaining effects
            var index = 0;
            foreach (var baseLayerEffect in LayerEffects.OrderBy(e => e.Order))
            {
                baseLayerEffect.Order = Order = index + 1;
                index++;
            }

            OnLayerEffectsUpdated();
        }

        internal void AddLayerEffect([NotNull] BaseLayerEffect effect)
        {
            if (effect == null) throw new ArgumentNullException(nameof(effect));
            _layerEffects.Add(effect);
            OnLayerEffectsUpdated();
        }

        internal void DeactivateLayerEffect([NotNull] BaseLayerEffect effect)
        {
            if (effect == null) throw new ArgumentNullException(nameof(effect));

            // Remove the effect from the layer and dispose it
            _layerEffects.Remove(effect);
            effect.Dispose();
        }

        internal SKPath CreateShapeClip()
        {
            var shapeClip = new SKPath();
            if (Path == null)
                return shapeClip;

            if (Parent is EffectProfileElement effectParent)
                shapeClip = shapeClip.Op(effectParent.CreateShapeClip(), SKPathOp.Union);

            foreach (var baseLayerEffect in LayerEffects)
            {
                var effectClip = baseLayerEffect.InternalCreateShapeClip(Path);
                shapeClip = shapeClip.Op(effectClip, SKPathOp.Difference);
            }

            return shapeClip;
        }

        protected void ApplyLayerEffectsToEntity()
        {
            EffectsEntity.LayerEffects.Clear();
            foreach (var layerEffect in LayerEffects)
            {
                var layerEffectEntity = new LayerEffectEntity
                {
                    Id = layerEffect.EntityId,
                    PluginGuid = layerEffect.PluginInfo.Guid,
                    EffectType = layerEffect.GetType().Name,
                    Name = layerEffect.Name,
                    HasBeenRenamed = layerEffect.HasBeenRenamed,
                    Order = layerEffect.Order
                };
                EffectsEntity.LayerEffects.Add(layerEffectEntity);
                layerEffect.BaseProperties.ApplyToEntity();
            }
        }

        #region Events

        public event EventHandler LayerEffectsUpdated;

        internal void OnLayerEffectsUpdated()
        {
            LayerEffectsUpdated?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}