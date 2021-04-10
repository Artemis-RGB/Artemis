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
    /// <summary>
    ///     Represents an element of a <see cref="Profile" /> that has advanced rendering capabilities
    /// </summary>
    public abstract class RenderProfileElement : ProfileElement
    {
        private SKRectI _bounds;
        private SKPath? _path;

        internal RenderProfileElement(Profile profile) : base(profile)
        {
            Timeline = new Timeline();
            ExpandedPropertyGroups = new List<string>();
            LayerEffectsList = new List<BaseLayerEffect>();

            LayerEffectStore.LayerEffectAdded += LayerEffectStoreOnLayerEffectAdded;
            LayerEffectStore.LayerEffectRemoved += LayerEffectStoreOnLayerEffectRemoved;
        }

        /// <summary>
        ///     Gets a boolean indicating whether this render element and its layers/brushes are enabled
        /// </summary>
        public bool Enabled { get; protected set; }

        /// <summary>
        ///     Gets a boolean indicating whether this render element and its layers/brushes should be enabled
        /// </summary>
        public abstract bool ShouldBeEnabled { get; }

        /// <summary>
        ///     Creates a list of all layer properties present on this render element
        /// </summary>
        /// <returns>A list of all layer properties present on this render element</returns>
        public abstract List<ILayerProperty> GetAllLayerProperties();

        /// <summary>
        ///     Occurs when a layer effect has been added or removed to this render element
        /// </summary>
        public event EventHandler? LayerEffectsUpdated;

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            LayerEffectStore.LayerEffectAdded -= LayerEffectStoreOnLayerEffectAdded;
            LayerEffectStore.LayerEffectRemoved -= LayerEffectStoreOnLayerEffectRemoved;

            foreach (BaseLayerEffect baseLayerEffect in LayerEffects)
                baseLayerEffect.Dispose();

            base.Dispose(disposing);
        }

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
                LayerEffectEntity layerEffectEntity = new()
                {
                    Id = layerEffect.EntityId,
                    ProviderId = layerEffect.Descriptor?.PlaceholderFor ?? layerEffect.ProviderId,
                    EffectType = layerEffect.GetEffectTypeName(),
                    Name = layerEffect.Name,
                    Suspended = layerEffect.Suspended,
                    HasBeenRenamed = layerEffect.HasBeenRenamed,
                    Order = layerEffect.Order
                };
                RenderElementEntity.LayerEffects.Add(layerEffectEntity);
                layerEffect.BaseProperties?.ApplyToEntity();
            }

            // Conditions
            RenderElementEntity.DisplayCondition = DisplayCondition?.Entity;
            DisplayCondition?.Save();

            // Timeline
            RenderElementEntity.Timeline = Timeline?.Entity;
            Timeline?.Save();
        }

        internal void OnLayerEffectsUpdated()
        {
            LayerEffectsUpdated?.Invoke(this, EventArgs.Empty);
        }

        #region Timeline

        /// <summary>
        ///     Gets the timeline associated with this render element
        /// </summary>
        public Timeline Timeline { get; private set; }

        /// <summary>
        ///     Updates the <see cref="Timeline" /> according to the provided <paramref name="deltaTime" /> and current display
        ///     condition status
        /// </summary>
        public void UpdateTimeline(double deltaTime)
        {
            // The play mode dictates whether to stick to the main segment unless the display conditions contains events
            bool stickToMainSegment = Timeline.PlayMode == TimelinePlayMode.Repeat && DisplayConditionMet;
            if (DisplayCondition != null && DisplayCondition.ContainsEvents)
                stickToMainSegment = false;
            Timeline.Update(TimeSpan.FromSeconds(deltaTime), stickToMainSegment);
        }

        #endregion

        #region Properties

        internal abstract RenderElementEntity RenderElementEntity { get; }

        /// <summary>
        ///     Gets the parent of this element
        /// </summary>
        public new ProfileElement? Parent
        {
            get => base.Parent;
            internal set
            {
                base.Parent = value;
                OnPropertyChanged(nameof(Parent));
            }
        }

        /// <summary>
        ///     Gets the path containing all the LEDs this entity is applied to, any rendering outside the entity Path is
        ///     clipped.
        /// </summary>
        public SKPath? Path
        {
            get => _path;
            protected set
            {
                SetAndNotify(ref _path, value);
                // I can't really be sure about the performance impact of calling Bounds often but
                // SkiaSharp calls SkiaApi.sk_path_get_bounds (Handle, &rect); which sounds expensive
                Bounds = SKRectI.Round(value?.Bounds ?? SKRect.Empty);
            }
        }

        /// <summary>
        ///     The bounds of this entity
        /// </summary>
        public SKRectI Bounds
        {
            get => _bounds;
            private set => SetAndNotify(ref _bounds, value);
        }


        #region Property group expansion

        internal List<string> ExpandedPropertyGroups;

        /// <summary>
        ///     Determines whether the provided property group is expanded
        /// </summary>
        /// <param name="layerPropertyGroup">The property group to check</param>
        /// <returns>A boolean indicating whether the provided property group is expanded</returns>
        public bool IsPropertyGroupExpanded(LayerPropertyGroup layerPropertyGroup)
        {
            return ExpandedPropertyGroups.Contains(layerPropertyGroup.Path);
        }

        /// <summary>
        ///     Expands or collapses the provided property group
        /// </summary>
        /// <param name="layerPropertyGroup">The group to expand or collapse</param>
        /// <param name="expanded">Whether to expand or collapse the property group</param>
        public void SetPropertyGroupExpanded(LayerPropertyGroup layerPropertyGroup, bool expanded)
        {
            if (!expanded && IsPropertyGroupExpanded(layerPropertyGroup))
                ExpandedPropertyGroups.Remove(layerPropertyGroup.Path);
            else if (expanded && !IsPropertyGroupExpanded(layerPropertyGroup))
                ExpandedPropertyGroups.Add(layerPropertyGroup.Path);
        }

        #endregion

        #endregion

        #region State

        /// <summary>
        ///     Enables the render element and its brushes and effects
        /// </summary>
        public abstract void Disable();

        /// <summary>
        ///     Disables the render element and its brushes and effects
        /// </summary>
        public abstract void Enable();

        #endregion

        #region Effect management

        internal List<BaseLayerEffect> LayerEffectsList;

        /// <summary>
        ///     Gets a read-only collection of the layer effects on this entity
        /// </summary>
        public ReadOnlyCollection<BaseLayerEffect> LayerEffects => LayerEffectsList.AsReadOnly();

        /// <summary>
        ///     Adds a the layer effect described inthe provided <paramref name="descriptor" />
        /// </summary>
        public void AddLayerEffect(LayerEffectDescriptor descriptor)
        {
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));

            LayerEffectEntity entity = new()
            {
                Id = Guid.NewGuid(),
                Suspended = false,
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
            LayerEffectsList.Remove(effect);
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

            LayerEffectsList.Sort((a, b) => a.Order.CompareTo(b.Order));
        }

        internal void ActivateEffects()
        {
            foreach (LayerEffectEntity layerEffectEntity in RenderElementEntity.LayerEffects)
            {
                // If there is a non-placeholder existing effect, skip this entity
                BaseLayerEffect? existing = LayerEffectsList.FirstOrDefault(e => e.EntityId == layerEffectEntity.Id);
                if (existing != null && existing.Descriptor.PlaceholderFor == null)
                    continue;

                LayerEffectDescriptor? descriptor = LayerEffectStore.Get(layerEffectEntity.ProviderId, layerEffectEntity.EffectType)?.LayerEffectDescriptor;
                if (descriptor != null)
                {
                    // If a descriptor is found but there is an existing placeholder, remove the placeholder
                    if (existing != null)
                    {
                        LayerEffectsList.Remove(existing);
                        existing.Dispose();
                    }

                    // Create an instance with the descriptor
                    descriptor.CreateInstance(this, layerEffectEntity);
                }
                else if (existing == null)
                {
                    // If no descriptor was found and there was no existing placeholder, create a placeholder
                    descriptor = PlaceholderLayerEffectDescriptor.Create(layerEffectEntity.ProviderId);
                    descriptor.CreateInstance(this, layerEffectEntity);
                }
            }

            OrderEffects();
        }


        internal void ActivateLayerEffect(BaseLayerEffect layerEffect)
        {
            LayerEffectsList.Add(layerEffect);
            OnLayerEffectsUpdated();
        }

        private void LayerEffectStoreOnLayerEffectRemoved(object? sender, LayerEffectStoreEvent e)
        {
            // If effects provided by the plugin are on the element, replace them with placeholders
            List<BaseLayerEffect> pluginEffects = LayerEffectsList.Where(ef => ef.ProviderId == e.Registration.PluginFeature.Id).ToList();
            foreach (BaseLayerEffect pluginEffect in pluginEffects)
            {
                LayerEffectEntity entity = RenderElementEntity.LayerEffects.First(en => en.Id == pluginEffect.EntityId);
                LayerEffectsList.Remove(pluginEffect);
                pluginEffect.Dispose();

                LayerEffectDescriptor descriptor = PlaceholderLayerEffectDescriptor.Create(pluginEffect.ProviderId);
                descriptor.CreateInstance(this, entity);
            }
        }

        private void LayerEffectStoreOnLayerEffectAdded(object? sender, LayerEffectStoreEvent e)
        {
            if (RenderElementEntity.LayerEffects.Any(ef => ef.ProviderId == e.Registration.PluginFeature.Id))
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

        private DataModelConditionGroup? _displayCondition;
        private bool _displayConditionMet;

        /// <summary>
        ///     Gets or sets the root display condition group
        /// </summary>
        public DataModelConditionGroup? DisplayCondition
        {
            get => _displayCondition;
            set => SetAndNotify(ref _displayCondition, value);
        }

        /// <summary>
        ///     Evaluates the display conditions on this element and applies any required changes to the <see cref="Timeline" />
        /// </summary>
        public void UpdateDisplayCondition()
        {
            if (Suspended)
            {
                DisplayConditionMet = false;
                return;
            }

            if (DisplayCondition == null)
            {
                DisplayConditionMet = true;
                return;
            }

            bool conditionMet = DisplayCondition.Evaluate();
            if (Parent is RenderProfileElement parent && !parent.DisplayConditionMet)
                conditionMet = false;

            if (!DisplayCondition.ContainsEvents)
            {
                // Regular conditions reset the timeline whenever their condition is met and was not met before that
                if (conditionMet && !DisplayConditionMet && Timeline.IsFinished)
                    Timeline.JumpToStart();
                // If regular conditions are no longer met, jump to the end segment if stop mode requires it
                if (!conditionMet && DisplayConditionMet && Timeline.StopMode == TimelineStopMode.SkipToEnd)
                    Timeline.JumpToEndSegment();
            }
            else if (conditionMet)
            {
                // Event conditions reset if the timeline finished
                if (Timeline.IsFinished)
                {
                    Timeline.JumpToStart();
                }
                // and otherwise apply their overlap mode
                else
                {
                    if (Timeline.EventOverlapMode == TimeLineEventOverlapMode.Restart)
                        Timeline.JumpToStart();
                    else if (Timeline.EventOverlapMode == TimeLineEventOverlapMode.Copy)
                        Timeline.AddExtraTimeline();
                    // The third option is ignore which is handled below:

                    // done
                }
            }

            DisplayConditionMet = conditionMet;
        }

        #endregion
    }
}