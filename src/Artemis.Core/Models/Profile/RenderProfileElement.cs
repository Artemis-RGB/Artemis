using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Core.LayerEffects;
using Artemis.Core.LayerEffects.Placeholder;
using Artemis.Core.Properties;
using Artemis.Storage.Entities.Profile;
using Artemis.Storage.Entities.Profile.Abstract;
using SkiaSharp;

namespace Artemis.Core
{
    public abstract class RenderProfileElement : ProfileElement
    {
        protected RenderProfileElement()
        {
            ApplyDataBindingsEnabled = true;
            ApplyKeyframesEnabled = true;

            LayerEffectStore.LayerEffectAdded += LayerEffectStoreOnLayerEffectAdded;
            LayerEffectStore.LayerEffectRemoved += LayerEffectStoreOnLayerEffectRemoved;
        }

        public abstract List<ILayerProperty> GetAllLayerProperties();

        internal void LoadRenderElement()
        {
            DisplayCondition = RenderElementEntity.DisplayCondition != null
                ? new DataModelConditionGroup(null, RenderElementEntity.DisplayCondition)
                : new DataModelConditionGroup(null);

            Timeline = RenderElementEntity.Timeline != null 
                ? new Timeline(RenderElementEntity.Timeline) 
                : new Timeline();

            ActivateEffects();
        }

        internal void SaveRenderElement()
        {
            RenderElementEntity.LayerEffects.Clear();
            foreach (BaseLayerEffect layerEffect in LayerEffects)
            {
                LayerEffectEntity layerEffectEntity = new LayerEffectEntity
                {
                    Id = layerEffect.EntityId,
                    PluginGuid = layerEffect.Descriptor.PlaceholderFor ?? layerEffect.PluginInfo.Guid,
                    EffectType = layerEffect.GetEffectTypeName(),
                    Name = layerEffect.Name,
                    Enabled = layerEffect.Enabled,
                    HasBeenRenamed = layerEffect.HasBeenRenamed,
                    Order = layerEffect.Order
                };
                RenderElementEntity.LayerEffects.Add(layerEffectEntity);
                layerEffect.BaseProperties.ApplyToEntity();
            }

            // Conditions
            RenderElementEntity.DisplayCondition = DisplayCondition?.Entity;
            DisplayCondition?.Save();

            // Timeline
            RenderElementEntity.Timeline = Timeline?.Entity;
            Timeline?.Save();
        }

        #region Timeline

        /// <summary>
        ///     Gets the timeline associated with this render element
        /// </summary>
        public Timeline? Timeline { get; private set; }

        #endregion

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            LayerEffectStore.LayerEffectAdded -= LayerEffectStoreOnLayerEffectAdded;
            LayerEffectStore.LayerEffectRemoved -= LayerEffectStoreOnLayerEffectRemoved;

            foreach (BaseLayerEffect baseLayerEffect in LayerEffects)
                baseLayerEffect.Dispose();

            base.Dispose(disposing);
        }

        #endregion

        #region Properties

        private SKPath _path;
        internal abstract RenderElementEntity RenderElementEntity { get; }

        /// <summary>
        ///     Gets the path containing all the LEDs this entity is applied to, any rendering outside the entity Path is
        ///     clipped.
        /// </summary>
        public SKPath Path
        {
            get => _path;
            protected set
            {
                SetAndNotify(ref _path, value);
                // I can't really be sure about the performance impact of calling Bounds often but
                // SkiaSharp calls SkiaApi.sk_path_get_bounds (Handle, &rect); which sounds expensive
                Bounds = value?.Bounds ?? SKRect.Empty;
            }
        }

        /// <summary>
        ///     The bounds of this entity
        /// </summary>
        public SKRect Bounds
        {
            get => _bounds;
            private set => SetAndNotify(ref _bounds, value);
        }

        #region Property group expansion

        protected List<string> _expandedPropertyGroups;
        private SKRect _bounds;

        public bool IsPropertyGroupExpanded(LayerPropertyGroup layerPropertyGroup)
        {
            return _expandedPropertyGroups.Contains(layerPropertyGroup.Path);
        }

        public void SetPropertyGroupExpanded(LayerPropertyGroup layerPropertyGroup, bool expanded)
        {
            if (!expanded && IsPropertyGroupExpanded(layerPropertyGroup))
                _expandedPropertyGroups.Remove(layerPropertyGroup.Path);
            else if (expanded && !IsPropertyGroupExpanded(layerPropertyGroup))
                _expandedPropertyGroups.Add(layerPropertyGroup.Path);
        }

        #endregion

        #endregion

        #region Effect management

        protected List<BaseLayerEffect> _layerEffects;

        /// <summary>
        ///     Gets a read-only collection of the layer effects on this entity
        /// </summary>
        public ReadOnlyCollection<BaseLayerEffect> LayerEffects => _layerEffects.AsReadOnly();

        /// <summary>
        ///     Adds a the layer effect described inthe provided <paramref name="descriptor" />
        /// </summary>
        public void AddLayerEffect(LayerEffectDescriptor descriptor)
        {
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));

            LayerEffectEntity entity = new LayerEffectEntity
            {
                Id = Guid.NewGuid(),
                Enabled = true,
                Order = LayerEffects.Count + 1
            };
            descriptor.CreateInstance(this, entity);

            OrderEffects();
            OnLayerEffectsUpdated();
        }

        /// <summary>
        ///     Removes the provided layer
        /// </summary>
        /// <param name="effect"></param>
        public void RemoveLayerEffect([NotNull] BaseLayerEffect effect)
        {
            if (effect == null) throw new ArgumentNullException(nameof(effect));

            // Remove the effect from the layer and dispose it
            _layerEffects.Remove(effect);
            effect.Dispose();

            // Update the order on the remaining effects
            OrderEffects();
            OnLayerEffectsUpdated();
        }

        private void OrderEffects()
        {
            int index = 0;
            foreach (BaseLayerEffect baseLayerEffect in LayerEffects.OrderBy(e => e.Order))
            {
                baseLayerEffect.Order = Order = index + 1;
                index++;
            }

            _layerEffects.Sort((a, b) => a.Order.CompareTo(b.Order));
        }

        internal void ActivateEffects()
        {
            foreach (LayerEffectEntity layerEffectEntity in RenderElementEntity.LayerEffects)
            {
                // If there is a non-placeholder existing effect, skip this entity
                BaseLayerEffect existing = _layerEffects.FirstOrDefault(e => e.EntityId == layerEffectEntity.Id);
                if (existing != null && existing.Descriptor.PlaceholderFor == null)
                    continue;

                LayerEffectDescriptor descriptor = LayerEffectStore.Get(layerEffectEntity.PluginGuid, layerEffectEntity.EffectType)?.LayerEffectDescriptor;
                if (descriptor != null)
                {
                    // If a descriptor is found but there is an existing placeholder, remove the placeholder
                    if (existing != null)
                    {
                        _layerEffects.Remove(existing);
                        existing.Dispose();
                    }

                    // Create an instance with the descriptor
                    descriptor.CreateInstance(this, layerEffectEntity);
                }
                else if (existing == null)
                {
                    // If no descriptor was found and there was no existing placeholder, create a placeholder
                    descriptor = PlaceholderLayerEffectDescriptor.Create(layerEffectEntity.PluginGuid);
                    descriptor.CreateInstance(this, layerEffectEntity);
                }
            }

            OrderEffects();
        }


        internal void ActivateLayerEffect(BaseLayerEffect layerEffect)
        {
            _layerEffects.Add(layerEffect);
            OnLayerEffectsUpdated();
        }

        private void LayerEffectStoreOnLayerEffectRemoved(object sender, LayerEffectStoreEvent e)
        {
            // If effects provided by the plugin are on the element, replace them with placeholders
            List<BaseLayerEffect> pluginEffects = _layerEffects.Where(ef => ef.Descriptor.LayerEffectProvider != null &&
                                                                            ef.PluginInfo.Guid == e.Registration.Plugin.PluginInfo.Guid).ToList();
            foreach (BaseLayerEffect pluginEffect in pluginEffects)
            {
                LayerEffectEntity entity = RenderElementEntity.LayerEffects.First(en => en.Id == pluginEffect.EntityId);
                _layerEffects.Remove(pluginEffect);
                pluginEffect.Dispose();

                LayerEffectDescriptor descriptor = PlaceholderLayerEffectDescriptor.Create(pluginEffect.PluginInfo.Guid);
                descriptor.CreateInstance(this, entity);
            }
        }

        private void LayerEffectStoreOnLayerEffectAdded(object sender, LayerEffectStoreEvent e)
        {
            if (RenderElementEntity.LayerEffects.Any(ef => ef.PluginGuid == e.Registration.Plugin.PluginInfo.Guid))
                ActivateEffects();
        }

        #endregion

        #region Conditions

        /// <summary>
        ///     Gets whether the display conditions applied to this layer where met or not during last update
        ///     <para>Always true if the layer has no display conditions</para>
        /// </summary>
        public bool DisplayConditionMet
        {
            get => _displayConditionMet;
            protected set => SetAndNotify(ref _displayConditionMet, value);
        }

        private DataModelConditionGroup _displayCondition;
        private bool _displayConditionMet;

        /// <summary>
        ///     Gets or sets the root display condition group
        /// </summary>
        public DataModelConditionGroup DisplayCondition
        {
            get => _displayCondition;
            set => SetAndNotify(ref _displayCondition, value);
        }

        /// <summary>
        ///     Gets or sets whether keyframes should be applied when this profile element updates
        /// </summary>
        public bool ApplyKeyframesEnabled { get; set; }

        /// <summary>
        ///     Gets or sets whether data bindings should be applied when this profile element updates
        /// </summary>
        public bool ApplyDataBindingsEnabled { get; set; }

        public void UpdateDisplayCondition()
        {
            if (DisplayCondition == null)
            {
                DisplayConditionMet = true;
                return;
            }

            bool conditionMet = DisplayCondition.Evaluate();

            // Regular conditions reset the timeline whenever their condition is met and was not met before that
            if (!DisplayCondition.ContainsEvents)
            {
                if (conditionMet && !DisplayConditionMet && Timeline.IsFinished)
                    Timeline.JumpToStart();
            }
            // Event conditions reset if the timeline finished and otherwise apply their overlap mode
            else if (conditionMet)
            {
                if (Timeline.IsFinished)
                {
                    Timeline.JumpToStart();
                }
                else
                {
                    if (Timeline.EventOverlapMode == TimeLineEventOverlapMode.Restart)
                        Timeline.JumpToStart();
                    else if (Timeline.EventOverlapMode == TimeLineEventOverlapMode.Copy)
                        Timeline.AddExtraTimeline();
                }
            }

            DisplayConditionMet = conditionMet;
        }

        #endregion

        #region Events

        public event EventHandler LayerEffectsUpdated;

        internal void OnLayerEffectsUpdated()
        {
            LayerEffectsUpdated?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}