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
            ExtraTimeLines = new List<TimeSpan>();

            LayerEffectStore.LayerEffectAdded += LayerEffectStoreOnLayerEffectAdded;
            LayerEffectStore.LayerEffectRemoved += LayerEffectStoreOnLayerEffectRemoved;
        }

        public abstract List<ILayerProperty> GetAllLayerProperties();

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

        internal void ApplyRenderElementDefaults()
        {
            MainSegmentLength = TimeSpan.FromSeconds(5);
        }

        internal void LoadRenderElement()
        {
            StartSegmentLength = RenderElementEntity.StartSegmentLength;
            MainSegmentLength = RenderElementEntity.MainSegmentLength;
            EndSegmentLength = RenderElementEntity.EndSegmentLength;
            PlayMode = (TimelinePlayMode) RenderElementEntity.PlayMode;
            StopMode = (TimelineStopMode) RenderElementEntity.StopMode;
            EventOverlapMode = (TimeLineEventOverlapMode) RenderElementEntity.EventOverlapMode;

            DisplayCondition = RenderElementEntity.DisplayCondition != null
                ? new DataModelConditionGroup(null, RenderElementEntity.DisplayCondition)
                : new DataModelConditionGroup(null);

            ActivateEffects();
        }

        internal void SaveRenderElement()
        {
            RenderElementEntity.StartSegmentLength = StartSegmentLength;
            RenderElementEntity.MainSegmentLength = MainSegmentLength;
            RenderElementEntity.EndSegmentLength = EndSegmentLength;
            RenderElementEntity.PlayMode = (int) PlayMode;
            RenderElementEntity.StopMode = (int) StopMode;
            RenderElementEntity.EventOverlapMode = (int) EventOverlapMode;

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
        private TimeSpan _timeLine;
        private TimelinePlayMode _playMode;
        private TimelineStopMode _stopMode;
        private TimeLineEventOverlapMode _eventOverlapMode;

        /// <summary>
        ///     Gets or sets the length of the start segment
        /// </summary>
        public TimeSpan StartSegmentLength
        {
            get => _startSegmentLength;
            set
            {
                if (!SetAndNotify(ref _startSegmentLength, value)) return;
                UpdateTimeLineLength();
                if (Parent is RenderProfileElement renderElement)
                    renderElement.UpdateTimeLineLength();
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
                UpdateTimeLineLength();
                if (Parent is RenderProfileElement renderElement)
                    renderElement.UpdateTimeLineLength();
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
                UpdateTimeLineLength();
                if (Parent is RenderProfileElement renderElement)
                    renderElement.UpdateTimeLineLength();
            }
        }

        /// <summary>
        ///     Gets the current time line position
        /// </summary>
        public TimeSpan TimeLine
        {
            get => _timeLine;
            protected set => SetAndNotify(ref _timeLine, value);
        }

        /// <summary>
        ///     Gets a list of extra time lines to render the element at each frame. Extra time lines are removed when they reach
        ///     their end
        /// </summary>
        public List<TimeSpan> ExtraTimeLines { get; }

        /// <summary>
        ///     Gets or sets the mode in which the render element starts its timeline when display conditions are met
        /// </summary>
        public TimelinePlayMode PlayMode
        {
            get => _playMode;
            set => SetAndNotify(ref _playMode, value);
        }

        /// <summary>
        ///     Gets or sets the mode in which the render element stops its timeline when display conditions are no longer met
        /// </summary>
        public TimelineStopMode StopMode
        {
            get => _stopMode;
            set => SetAndNotify(ref _stopMode, value);
        }

        /// <summary>
        ///     Gets or sets the mode in which the render element responds to display condition events being fired before the
        ///     timeline finished
        /// </summary>
        public TimeLineEventOverlapMode EventOverlapMode
        {
            get => _eventOverlapMode;
            set => SetAndNotify(ref _eventOverlapMode, value);
        }

        /// <summary>
        ///     Gets the max length of this element and any of its children
        /// </summary>
        public TimeSpan TimelineLength { get; protected set; }

        /// <summary>
        ///     Updates the time line and any extra time lines present in <see cref="ExtraTimeLines" />
        /// </summary>
        /// <param name="deltaTime">The delta with which to move the time lines</param>
        protected void UpdateTimeLines(double deltaTime)
        {
            bool repeatMainSegment = PlayMode == TimelinePlayMode.Repeat;
            bool finishMainSegment = StopMode == TimelineStopMode.Finish;

            // If the condition is event-based, never display continuously and always finish the timeline
            if (DisplayCondition != null && DisplayCondition.ContainsEvents)
            {
                repeatMainSegment = false;
                finishMainSegment = true;
            }

            TimeSpan deltaTimeSpan = TimeSpan.FromSeconds(deltaTime);
            TimeSpan mainSegmentEnd = StartSegmentLength + MainSegmentLength;

            // Update the main time line
            if (TimeLine != TimeSpan.Zero || DisplayConditionMet)
            {
                TimeLine += deltaTimeSpan;

                // Apply play and stop mode
                if (DisplayConditionMet && repeatMainSegment && TimeLine >= mainSegmentEnd)
                    TimeLine = StartSegmentLength;
                else if (!DisplayConditionMet && !finishMainSegment)
                    TimeLine = mainSegmentEnd.Add(new TimeSpan(1));
            }

            lock (ExtraTimeLines)
            {
                // Remove extra time lines that have finished
                ExtraTimeLines.RemoveAll(t => t >= mainSegmentEnd);
                // Update remaining extra time lines
                for (int index = 0; index < ExtraTimeLines.Count; index++)
                    ExtraTimeLines[index] += deltaTimeSpan;
            }
        }

        /// <summary>
        /// Overrides the time line to the specified time and clears any extra time lines
        /// </summary>
        /// <param name="time">The time to override to</param>
        /// <param name="stickToMainSegment">Whether to stick to the main segment, wrapping around if needed</param>
        public void OverrideTimeLines(TimeSpan time, bool stickToMainSegment)
        {
            ExtraTimeLines.Clear();
            TimeLine = time;

            if (stickToMainSegment && TimeLine > StartSegmentLength)
                TimeLine = StartSegmentLength + TimeSpan.FromMilliseconds(time.TotalMilliseconds % MainSegmentLength.TotalMilliseconds);
        }

        protected internal abstract void UpdateTimeLineLength();

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
                if (conditionMet && !DisplayConditionMet && TimeLine > TimelineLength)
                    TimeLine = TimeSpan.Zero;
            }
            // Event conditions reset if the timeline finished and otherwise apply their overlap mode
            else if (conditionMet)
            {
                if (TimeLine > TimelineLength)
                    TimeLine = TimeSpan.Zero;
                else
                {
                    if (EventOverlapMode == TimeLineEventOverlapMode.Restart)
                        TimeLine = TimeSpan.Zero;
                    else if (EventOverlapMode == TimeLineEventOverlapMode.Copy)
                        ExtraTimeLines.Add(new TimeSpan());
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

    /// <summary>
    ///     Represents a mode for render elements to start their timeline when display conditions are met
    /// </summary>
    public enum TimelinePlayMode
    {
        /// <summary>
        ///     Continue repeating the main segment of the timeline while the condition is met
        /// </summary>
        Repeat,

        /// <summary>
        ///     Only play the timeline once when the condition is met
        /// </summary>
        Once
    }

    /// <summary>
    ///     Represents a mode for render elements to stop their timeline when display conditions are no longer met
    /// </summary>
    public enum TimelineStopMode
    {
        /// <summary>
        ///     When conditions are no longer met, finish the the current run of the main timeline
        /// </summary>
        Finish,

        /// <summary>
        ///     When conditions are no longer met, skip to the end segment of the timeline
        /// </summary>
        SkipToEnd
    }

    /// <summary>
    ///     Represents a mode for render elements to start their timeline when display conditions events are fired
    /// </summary>
    public enum TimeLineEventOverlapMode
    {
        /// <summary>
        ///     Stop the current run and restart the timeline
        /// </summary>
        Restart,

        /// <summary>
        ///     Ignore subsequent event fires until the timeline finishes
        /// </summary>
        Ignore,

        /// <summary>
        ///     Play another copy of the timeline on top of the current run
        /// </summary>
        Copy
    }
}