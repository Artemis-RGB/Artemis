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
            LayerEffectStore.LayerEffectAdded += LayerEffectStoreOnLayerEffectAdded;
            LayerEffectStore.LayerEffectRemoved += LayerEffectStoreOnLayerEffectRemoved;
        }

        public abstract List<ILayerProperty> GetAllLayerProperties();

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            LayerEffectStore.LayerEffectAdded -= LayerEffectStoreOnLayerEffectAdded;
            LayerEffectStore.LayerEffectRemoved -= LayerEffectStoreOnLayerEffectRemoved;

            foreach (var baseLayerEffect in LayerEffects)
                baseLayerEffect.Dispose();

            base.Dispose(disposing);
        }

        #endregion

        internal void ApplyRenderElementDefaults()
        {
            MainSegmentLength = TimeSpan.FromSeconds(5);
        }

        internal void LoadRenderElement()
        {
            StartSegmentLength = RenderElementEntity.StartSegmentLength;
            MainSegmentLength = RenderElementEntity.MainSegmentLength;
            EndSegmentLength = RenderElementEntity.EndSegmentLength;
            DisplayContinuously = RenderElementEntity.DisplayContinuously;
            AlwaysFinishTimeline = RenderElementEntity.AlwaysFinishTimeline;

            DataModelConditionGroup = RenderElementEntity.RootDisplayCondition != null
                ? new DataModelConditionGroup(null, RenderElementEntity.RootDisplayCondition)
                : new DataModelConditionGroup(null);

            ActivateEffects();
        }

        internal void SaveRenderElement()
        {
            RenderElementEntity.StartSegmentLength = StartSegmentLength;
            RenderElementEntity.MainSegmentLength = MainSegmentLength;
            RenderElementEntity.EndSegmentLength = EndSegmentLength;
            RenderElementEntity.DisplayContinuously = DisplayContinuously;
            RenderElementEntity.AlwaysFinishTimeline = AlwaysFinishTimeline;

            RenderElementEntity.LayerEffects.Clear();
            foreach (var layerEffect in LayerEffects)
            {
                var layerEffectEntity = new LayerEffectEntity
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
            RenderElementEntity.RootDisplayCondition = DataModelConditionGroup?.Entity;
            DataModelConditionGroup?.Save();
        }

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

        #region Timeline

        private TimeSpan _startSegmentLength;
        private TimeSpan _mainSegmentLength;
        private TimeSpan _endSegmentLength;
        private bool _displayContinuously;
        private bool _alwaysFinishTimeline;

        /// <summary>
        ///     Gets or sets the length of the start segment
        /// </summary>
        public TimeSpan StartSegmentLength
        {
            get => _startSegmentLength;
            set
            {
                if (!SetAndNotify(ref _startSegmentLength, value)) return;
                UpdateTimelineLength();
                if (Parent is RenderProfileElement renderElement)
                    renderElement.UpdateTimelineLength();
            }
        }

        /// <summary>
        ///     Gets or sets the length of the main segment
        /// </summary>
        public TimeSpan MainSegmentLength
        {
            get => _mainSegmentLength;
            set
            {
                if (!SetAndNotify(ref _mainSegmentLength, value)) return;
                UpdateTimelineLength();
                if (Parent is RenderProfileElement renderElement)
                    renderElement.UpdateTimelineLength();
            }
        }

        /// <summary>
        ///     Gets or sets the length of the end segment
        /// </summary>
        public TimeSpan EndSegmentLength
        {
            get => _endSegmentLength;
            set
            {
                if (!SetAndNotify(ref _endSegmentLength, value)) return;
                UpdateTimelineLength();
                if (Parent is RenderProfileElement renderElement)
                    renderElement.UpdateTimelineLength();
            }
        }

        /// <summary>
        ///     Gets the current timeline position
        /// </summary>
        public TimeSpan TimelinePosition
        {
            get => _timelinePosition;
            protected set => SetAndNotify(ref _timelinePosition, value);
        }

        /// <summary>
        ///     Gets or sets whether main timeline should repeat itself as long as display conditions are met
        /// </summary>
        public bool DisplayContinuously
        {
            get => _displayContinuously;
            set => SetAndNotify(ref _displayContinuously, value);
        }

        /// <summary>
        ///     Gets or sets whether the timeline should finish when conditions are no longer met
        /// </summary>
        public bool AlwaysFinishTimeline
        {
            get => _alwaysFinishTimeline;
            set => SetAndNotify(ref _alwaysFinishTimeline, value);
        }

        /// <summary>
        ///     Gets the max length of this element and any of its children
        /// </summary>
        public TimeSpan TimelineLength { get; protected set; }

        protected double UpdateTimeline(double deltaTime)
        {
            var oldPosition = _timelinePosition;
            var deltaTimeSpan = TimeSpan.FromSeconds(deltaTime);
            var mainSegmentEnd = StartSegmentLength + MainSegmentLength;

            TimelinePosition += deltaTimeSpan;
            // Manage segments while the condition is met
            if (DisplayConditionMet)
            {
                // If we are at the end of the main timeline, wrap around back to the beginning
                if (DisplayContinuously && TimelinePosition >= mainSegmentEnd)
                    TimelinePosition = StartSegmentLength + (mainSegmentEnd - TimelinePosition);
            }
            else
            {
                // Skip to the last segment if conditions are no longer met
                if (!AlwaysFinishTimeline && TimelinePosition < mainSegmentEnd)
                    TimelinePosition = mainSegmentEnd;
            }

            return (TimelinePosition - oldPosition).TotalSeconds;
        }

        protected internal abstract void UpdateTimelineLength();

        /// <summary>
        ///     Overrides the progress of the element
        /// </summary>
        /// <param name="timeOverride"></param>
        /// <param name="stickToMainSegment"></param>
        public abstract void OverrideProgress(TimeSpan timeOverride, bool stickToMainSegment);

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

            var entity = new LayerEffectEntity
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
            var index = 0;
            foreach (var baseLayerEffect in LayerEffects.OrderBy(e => e.Order))
            {
                baseLayerEffect.Order = Order = index + 1;
                index++;
            }

            _layerEffects.Sort((a, b) => a.Order.CompareTo(b.Order));
        }

        internal void ActivateEffects()
        {
            foreach (var layerEffectEntity in RenderElementEntity.LayerEffects)
            {
                // If there is a non-placeholder existing effect, skip this entity
                var existing = _layerEffects.FirstOrDefault(e => e.EntityId == layerEffectEntity.Id);
                if (existing != null && existing.Descriptor.PlaceholderFor == null)
                    continue;

                var descriptor = LayerEffectStore.Get(layerEffectEntity.PluginGuid, layerEffectEntity.EffectType)?.LayerEffectDescriptor;
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
            var pluginEffects = _layerEffects.Where(ef => ef.Descriptor.LayerEffectProvider != null &&
                                                          ef.PluginInfo.Guid == e.Registration.Plugin.PluginInfo.Guid).ToList();
            foreach (var pluginEffect in pluginEffects)
            {
                var entity = RenderElementEntity.LayerEffects.First(en => en.Id == pluginEffect.EntityId);
                _layerEffects.Remove(pluginEffect);
                pluginEffect.Dispose();

                var descriptor = PlaceholderLayerEffectDescriptor.Create(pluginEffect.PluginInfo.Guid);
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
            private set => SetAndNotify(ref _displayConditionMet, value);
        }

        private DataModelConditionGroup _dataModelConditionGroup;
        private TimeSpan _timelinePosition;
        private bool _displayConditionMet;

        /// <summary>
        ///     Gets or sets the root display condition group
        /// </summary>
        public DataModelConditionGroup DataModelConditionGroup
        {
            get => _dataModelConditionGroup;
            set => SetAndNotify(ref _dataModelConditionGroup, value);
        }

        public void UpdateDisplayCondition()
        {
            var conditionMet = DataModelConditionGroup == null || DataModelConditionGroup.Evaluate();
            if (conditionMet && !DisplayConditionMet)
                TimelinePosition = TimeSpan.Zero;

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