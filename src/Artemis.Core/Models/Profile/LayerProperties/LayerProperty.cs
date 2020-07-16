using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Artemis.Core.Exceptions;
using Artemis.Core.Utilities;
using Artemis.Storage.Entities.Profile;
using Newtonsoft.Json;

namespace Artemis.Core.Models.Profile.LayerProperties
{
    /// <summary>
    ///     Represents a property on a layer. Properties are saved in storage and can optionally be modified from the UI.
    ///     <para>
    ///         Note: You cannot initialize layer properties yourself. If properly placed and annotated, the Artemis core will
    ///         initialize these for you.
    ///     </para>
    /// </summary>
    /// <typeparam name="T">The type of property encapsulated in this layer property</typeparam>
    public class LayerProperty<T> : BaseLayerProperty
    {
        private T _baseValue;
        private T _currentValue;
        private bool _isInitialized;
        private List<LayerPropertyKeyframe<T>> _keyframes;

        protected LayerProperty()
        {
            _keyframes = new List<LayerPropertyKeyframe<T>>();
        }

        /// <summary>
        ///     Gets or sets the base value of this layer property without any keyframes applied
        /// </summary>
        public T BaseValue
        {
            get => _baseValue;
            set
            {
                if (_baseValue != null && !_baseValue.Equals(value) || _baseValue == null && value != null)
                {
                    _baseValue = value;
                    OnBaseValueChanged();
                }
            }
        }

        /// <summary>
        ///     Gets the current value of this property as it is affected by it's keyframes, updated once every frame
        /// </summary>
        public T CurrentValue
        {
            get => !KeyframesEnabled || !KeyframesSupported ? BaseValue : _currentValue;
            internal set => _currentValue = value;
        }

        /// <summary>
        ///     Gets or sets the default value of this layer property. If set, this value is automatically applied if the property
        ///     has no
        ///     value in storage
        /// </summary>
        public T DefaultValue { get; set; }

        /// <summary>
        ///     Gets a read-only list of all the keyframes on this layer property
        /// </summary>
        public IReadOnlyList<LayerPropertyKeyframe<T>> Keyframes => _keyframes.AsReadOnly();

        /// <summary>
        ///     Gets the current keyframe in the timeline according to the current progress
        /// </summary>
        public LayerPropertyKeyframe<T> CurrentKeyframe { get; protected set; }

        /// <summary>
        ///     Gets the next keyframe in the timeline according to the current progress
        /// </summary>
        public LayerPropertyKeyframe<T> NextKeyframe { get; protected set; }

        public override IReadOnlyList<BaseLayerPropertyKeyframe> BaseKeyframes => _keyframes.Cast<BaseLayerPropertyKeyframe>().ToList().AsReadOnly();

        /// <summary>
        ///     Sets the current value, using either keyframes if enabled or the base value.
        /// </summary>
        /// <param name="value">The value to set.</param>
        /// <param name="time">
        ///     An optional time to set the value add, if provided and property is using keyframes the value will be set to an new
        ///     or existing keyframe.
        /// </param>
        public void SetCurrentValue(T value, TimeSpan? time)
        {
            if (time == null || !KeyframesEnabled || !KeyframesSupported)
                BaseValue = value;
            else
            {
                // If on a keyframe, update the keyframe
                var currentKeyframe = Keyframes.FirstOrDefault(k => k.Position == time.Value);
                // Create a new keyframe if none found
                if (currentKeyframe == null)
                    AddKeyframe(new LayerPropertyKeyframe<T>(value, time.Value, Easings.Functions.Linear, this));
                else
                    currentKeyframe.Value = value;

                // Update the property so that the new keyframe is reflected on the current value
                Update(0);
            }
        }

        /// <summary>
        ///     Adds a keyframe to the layer property
        /// </summary>
        /// <param name="keyframe">The keyframe to add</param>
        public void AddKeyframe(LayerPropertyKeyframe<T> keyframe)
        {
            if (_keyframes.Contains(keyframe))
                return;

            keyframe.LayerProperty?.RemoveKeyframe(keyframe);

            keyframe.LayerProperty = this;
            keyframe.BaseLayerProperty = this;
            _keyframes.Add(keyframe);
            SortKeyframes();
            OnKeyframeAdded();
        }

        /// <summary>
        ///     Removes a keyframe from the layer property
        /// </summary>
        /// <param name="keyframe">The keyframe to remove</param>
        public LayerPropertyKeyframe<T> CopyKeyframe(LayerPropertyKeyframe<T> keyframe)
        {
            var newKeyframe = new LayerPropertyKeyframe<T>(
                keyframe.Value,
                keyframe.Position,
                keyframe.EasingFunction,
                keyframe.LayerProperty
            );
            AddKeyframe(newKeyframe);

            return newKeyframe;
        }

        /// <summary>
        ///     Removes a keyframe from the layer property
        /// </summary>
        /// <param name="keyframe">The keyframe to remove</param>
        public void RemoveKeyframe(LayerPropertyKeyframe<T> keyframe)
        {
            if (!_keyframes.Contains(keyframe))
                return;

            _keyframes.Remove(keyframe);
            keyframe.LayerProperty = null;
            keyframe.BaseLayerProperty = null;
            SortKeyframes();
            OnKeyframeRemoved();
        }

        /// <summary>
        ///     Removes all keyframes from the layer property
        /// </summary>
        public void ClearKeyframes()
        {
            var keyframes = new List<LayerPropertyKeyframe<T>>(_keyframes);
            foreach (var layerPropertyKeyframe in keyframes)
                RemoveKeyframe(layerPropertyKeyframe);
        }

        public override void ApplyDefaultValue()
        {
            BaseValue = DefaultValue;
            CurrentValue = DefaultValue;
        }

        /// <summary>
        ///     Called every update (if keyframes are both supported and enabled) to determine the new <see cref="CurrentValue" />
        ///     based on the provided progress
        /// </summary>
        /// <param name="keyframeProgress">The linear current keyframe progress</param>
        /// <param name="keyframeProgressEased">The current keyframe progress, eased with the current easing function</param>
        protected virtual void UpdateCurrentValue(float keyframeProgress, float keyframeProgressEased)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Updates the property, moving the timeline forwards by the provided <paramref name="deltaTime" />
        /// </summary>
        /// <param name="deltaTime">The amount of time to move the timeline forwards</param>
        internal void Update(double deltaTime)
        {
            TimelineProgress = TimelineProgress.Add(TimeSpan.FromSeconds(deltaTime));
            if (!KeyframesSupported || !KeyframesEnabled)
                return;

            // The current keyframe is the last keyframe before the current time
            CurrentKeyframe = _keyframes.LastOrDefault(k => k.Position <= TimelineProgress);
            // Keyframes are sorted by position so we can safely assume the next keyframe's position is after the current 
            var nextIndex = _keyframes.IndexOf(CurrentKeyframe) + 1;
            NextKeyframe = _keyframes.Count > nextIndex ? _keyframes[nextIndex] : null;

            // No need to update the current value if either of the keyframes are null
            if (CurrentKeyframe == null)
                CurrentValue = _keyframes.Any() ? _keyframes[0].Value : BaseValue;
            else if (NextKeyframe == null)
                CurrentValue = CurrentKeyframe.Value;
            // Only determine progress and current value if both keyframes are present
            else
            {
                var timeDiff = NextKeyframe.Position - CurrentKeyframe.Position;
                var keyframeProgress = (float)((TimelineProgress - CurrentKeyframe.Position).TotalMilliseconds / timeDiff.TotalMilliseconds);
                var keyframeProgressEased = (float)Easings.Interpolate(keyframeProgress, CurrentKeyframe.EasingFunction);
                UpdateCurrentValue(keyframeProgress, keyframeProgressEased);
            }

            OnUpdated();
        }

        /// <summary>
        ///     Overrides the timeline progress to match the provided <paramref name="overrideTime" />
        /// </summary>
        /// <param name="overrideTime">The new progress to set the layer property timeline to.</param>
        internal void OverrideProgress(TimeSpan overrideTime)
        {
            TimelineProgress = TimeSpan.Zero;
            Update(overrideTime.TotalSeconds);
        }

        /// <summary>
        ///     Sorts the keyframes in ascending order by position
        /// </summary>
        internal void SortKeyframes()
        {
            _keyframes = _keyframes.OrderBy(k => k.Position).ToList();
        }

        internal override void ApplyToLayerProperty(PropertyEntity entity, LayerPropertyGroup layerPropertyGroup, bool fromStorage)
        {
            // Doubt this will happen but let's make sure
            if (_isInitialized)
                throw new ArtemisCoreException("Layer property already initialized, wut");

            PropertyEntity = entity;
            LayerPropertyGroup = layerPropertyGroup;
            LayerPropertyGroup.PropertyGroupUpdating += (sender, args) => Update(args.DeltaTime);
            LayerPropertyGroup.PropertyGroupOverriding += (sender, args) => OverrideProgress(args.OverrideTime);

            try
            {
                if (entity.Value != null)
                    BaseValue = JsonConvert.DeserializeObject<T>(entity.Value);

                IsLoadedFromStorage = fromStorage;
                CurrentValue = BaseValue;
                KeyframesEnabled = entity.KeyframesEnabled;

                _keyframes.Clear();
                _keyframes.AddRange(entity.KeyframeEntities.Select(k => new LayerPropertyKeyframe<T>(
                    JsonConvert.DeserializeObject<T>(k.Value),
                    k.Position,
                    (Easings.Functions)k.EasingFunction,
                    this
                )));
            }
            catch (JsonException e)
            {
                // TODO: Properly log the JSON exception
                Debug.WriteLine($"JSON exception while deserializing: {e}");
                IsLoadedFromStorage = false;
            }
            finally
            {
                SortKeyframes();
                _isInitialized = true;
            }
        }

        internal override void ApplyToEntity()
        {
            if (!_isInitialized)
                throw new ArtemisCoreException("Layer property is not yet initialized");

            PropertyEntity.Value = JsonConvert.SerializeObject(BaseValue);
            PropertyEntity.KeyframesEnabled = KeyframesEnabled;
            PropertyEntity.KeyframeEntities.Clear();
            PropertyEntity.KeyframeEntities.AddRange(Keyframes.Select(k => new KeyframeEntity
            {
                Value = JsonConvert.SerializeObject(k.Value),
                Position = k.Position,
                EasingFunction = (int)k.EasingFunction
            }));
        }
    }
}