using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Core.Annotations;
using Artemis.Core.Models.Profile.Conditions;
using Artemis.Core.Models.Profile.LayerProperties;
using Artemis.Core.Plugins.LayerEffect.Abstract;
using Artemis.Storage.Entities.Profile;
using Artemis.Storage.Entities.Profile.Abstract;
using SkiaSharp;

namespace Artemis.Core.Models.Profile
{
    public abstract class RenderProfileElement : ProfileElement
    {
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
        private bool _repeatMainSegment;
        private bool _alwaysFinishTimeline;

        /// <summary>
        ///     Gets or sets the length of the start segment
        /// </summary>
        public TimeSpan StartSegmentLength
        {
            get => _startSegmentLength;
            set => SetAndNotify(ref _startSegmentLength, value);
        }

        /// <summary>
        ///     Gets or sets the length of the main segment
        /// </summary>
        public TimeSpan MainSegmentLength
        {
            get => _mainSegmentLength;
            set => SetAndNotify(ref _mainSegmentLength, value);
        }

        /// <summary>
        ///     Gets or sets the length of the end segment
        /// </summary>
        public TimeSpan EndSegmentLength
        {
            get => _endSegmentLength;
            set => SetAndNotify(ref _endSegmentLength, value);
        }

        /// <summary>
        ///     Gets the total combined length of all three segments
        /// </summary>
        public TimeSpan TimelineLength => StartSegmentLength + MainSegmentLength + EndSegmentLength;

        /// <summary>
        ///     Gets or sets whether main timeline should repeat itself as long as display conditions are met
        /// </summary>
        public bool RepeatMainSegment
        {
            get => _repeatMainSegment;
            set => SetAndNotify(ref _repeatMainSegment, value);
        }

        /// <summary>
        ///     Gets or sets whether the timeline should finish when conditions are no longer met
        /// </summary>
        public bool AlwaysFinishTimeline
        {
            get => _alwaysFinishTimeline;
            set => SetAndNotify(ref _alwaysFinishTimeline, value);
        }

        #endregion

        #region Effects

        protected List<BaseLayerEffect> _layerEffects;

        /// <summary>
        ///     Gets a read-only collection of the layer effects on this entity
        /// </summary>
        public ReadOnlyCollection<BaseLayerEffect> LayerEffects => _layerEffects.AsReadOnly();

        protected void ApplyLayerEffectsToEntity()
        {
            RenderElementEntity.LayerEffects.Clear();
            foreach (var layerEffect in LayerEffects)
            {
                var layerEffectEntity = new LayerEffectEntity
                {
                    Id = layerEffect.EntityId,
                    PluginGuid = layerEffect.PluginInfo.Guid,
                    EffectType = layerEffect.GetType().Name,
                    Name = layerEffect.Name,
                    Enabled = layerEffect.Enabled,
                    HasBeenRenamed = layerEffect.HasBeenRenamed,
                    Order = layerEffect.Order
                };
                RenderElementEntity.LayerEffects.Add(layerEffectEntity);
                layerEffect.BaseProperties.ApplyToEntity();
            }
        }

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

        #endregion

        #region Conditions

        private DisplayConditionGroup _displayConditionGroup;
        
        /// <summary>
        ///     Gets or sets the root display condition group
        /// </summary>
        public DisplayConditionGroup DisplayConditionGroup
        {
            get => _displayConditionGroup;
            set => SetAndNotify(ref _displayConditionGroup, value);
        }

        public void UpdateDisplayCondition()
        {
            var rootGroupResult = DisplayConditionGroup.Evaluate();
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