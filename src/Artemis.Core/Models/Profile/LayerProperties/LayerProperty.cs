using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Artemis.Storage.Entities.Profile;
using Newtonsoft.Json;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a property on a layer. Properties are saved in storage and can optionally be modified from the UI.
    ///     <para>
    ///         Note: You cannot initialize layer properties yourself. If properly placed and annotated, the Artemis core will
    ///         initialize these for you.
    ///     </para>
    /// </summary>
    /// <typeparam name="T">The type of property encapsulated in this layer property</typeparam>
    public class LayerProperty<T> : CorePropertyChanged, ILayerProperty
    {
        private bool _disposed;

        /// <summary>
        ///     Creates a new instance of the <see cref="LayerProperty{T}" /> class
        /// </summary>
        protected LayerProperty()
        {
            // These are set right after construction to keep the constructor (and inherited constructs) clean
            ProfileElement = null!;
            LayerPropertyGroup = null!;
            Entity = null!;
            PropertyDescription = null!;
            DataBinding = null!;
            Path = "";

            CurrentValue = default!;
            DefaultValue = default!;

            // We'll try our best...
            // TODO: Consider alternatives
            if (typeof(T).IsValueType)
                _baseValue = default!;
            else if (typeof(T).GetConstructor(Type.EmptyTypes) != null)
                _baseValue = Activator.CreateInstance<T>();
            else
                _baseValue = default!;

            _keyframes = new List<LayerPropertyKeyframe<T>>();
            Keyframes = new ReadOnlyCollection<LayerPropertyKeyframe<T>>(_keyframes);
        }

        /// <summary>
        ///     Releases the unmanaged resources used by the object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        ///     <see langword="true" /> to release both managed and unmanaged resources;
        ///     <see langword="false" /> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            _disposed = true;

            DataBinding.Dispose();
            Disposed?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc />
        public PropertyDescriptionAttribute PropertyDescription { get; internal set; }

        /// <inheritdoc />
        public string Path { get; private set; }

        /// <inheritdoc />
        public Type PropertyType => typeof(T);

        /// <inheritdoc />
        public void Update(Timeline timeline)
        {
            if (_disposed)
                throw new ObjectDisposedException("LayerProperty");

            CurrentValue = BaseValue;

            UpdateKeyframes(timeline);
            UpdateDataBinding();

            // UpdateDataBinding called OnUpdated()
        }

        /// <inheritdoc />
        public void UpdateDataBinding()
        {
            DataBinding.Update();
            DataBinding.Apply();

            OnUpdated();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #region Hierarchy

        private bool _isHidden;

        /// <inheritdoc />
        public bool IsHidden
        {
            get => _isHidden;
            set
            {
                _isHidden = value;
                OnVisibilityChanged();
            }
        }

        /// <inheritdoc />
        public RenderProfileElement ProfileElement { get; private set; }

        /// <inheritdoc />
        public LayerPropertyGroup LayerPropertyGroup { get; private set; }

        #endregion

        #region Value management

        private T _baseValue;

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
        ///     Gets or sets the base value of this layer property without any keyframes applied
        /// </summary>
        public T BaseValue
        {
            get => _baseValue;
            set
            {
                if (Equals(_baseValue, value))
                    return;

                _baseValue = value;
                ReapplyUpdate();
                OnPropertyChanged(nameof(BaseValue));
            }
        }

        /// <summary>
        ///     Gets the current value of this property as it is affected by it's keyframes, updated once every frame
        /// </summary>
        public T CurrentValue { get; set; }

        /// <summary>
        ///     Gets or sets the default value of this layer property. If set, this value is automatically applied if the property
        ///     has no  value in storage
        /// </summary>
        public T DefaultValue { get; set; }

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
            if (_disposed)
                throw new ObjectDisposedException("LayerProperty");

            if (time == null || !KeyframesEnabled || !KeyframesSupported)
            {
                BaseValue = value;
            }
            else
            {
                // If on a keyframe, update the keyframe
                LayerPropertyKeyframe<T>? currentKeyframe = Keyframes.FirstOrDefault(k => k.Position == time.Value);
                // Create a new keyframe if none found
                if (currentKeyframe == null)
                    AddKeyframe(new LayerPropertyKeyframe<T>(value, time.Value, Easings.Functions.Linear, this));
                else
                    currentKeyframe.Value = value;
            }

            // Force an update so that the base value is applied to the current value and
            // keyframes/data bindings are applied using the new base value
            ReapplyUpdate();
        }

        /// <inheritdoc />
        public void ApplyDefaultValue()
        {
            if (_disposed)
                throw new ObjectDisposedException("LayerProperty");

            string json = CoreJson.SerializeObject(DefaultValue, true);
            KeyframesEnabled = false;
            SetCurrentValue(CoreJson.DeserializeObject<T>(json)!, null);
        }

        private void ReapplyUpdate()
        {
            // Create a timeline with the same position but a delta of zero
            Timeline temporaryTimeline = new();
            temporaryTimeline.Override(ProfileElement.Timeline.Position, false);
            temporaryTimeline.ClearDelta();

            Update(temporaryTimeline);
            OnCurrentValueSet();
        }

        #endregion

        #region Keyframes

        private bool _keyframesEnabled;
        private readonly List<LayerPropertyKeyframe<T>> _keyframes;

        /// <summary>
        ///     Gets whether keyframes are supported on this type of property
        /// </summary>
        public bool KeyframesSupported { get; protected internal set; } = true;

        /// <summary>
        ///     Gets or sets whether keyframes are enabled on this property, has no effect if <see cref="KeyframesSupported" /> is
        ///     False
        /// </summary>
        public bool KeyframesEnabled
        {
            get => _keyframesEnabled;
            set
            {
                if (_keyframesEnabled == value) return;
                _keyframesEnabled = value;
                OnKeyframesToggled();
                OnPropertyChanged(nameof(KeyframesEnabled));
            }
        }


        /// <summary>
        ///     Gets a read-only list of all the keyframes on this layer property
        /// </summary>
        public ReadOnlyCollection<LayerPropertyKeyframe<T>> Keyframes { get; }

        /// <summary>
        ///     Gets the current keyframe in the timeline according to the current progress
        /// </summary>
        public LayerPropertyKeyframe<T>? CurrentKeyframe { get; protected set; }

        /// <summary>
        ///     Gets the next keyframe in the timeline according to the current progress
        /// </summary>
        public LayerPropertyKeyframe<T>? NextKeyframe { get; protected set; }

        /// <summary>
        ///     Adds a keyframe to the layer property
        /// </summary>
        /// <param name="keyframe">The keyframe to add</param>
        public void AddKeyframe(LayerPropertyKeyframe<T> keyframe)
        {
            if (_disposed)
                throw new ObjectDisposedException("LayerProperty");

            if (_keyframes.Contains(keyframe))
                return;

            keyframe.LayerProperty?.RemoveKeyframe(keyframe);
            keyframe.LayerProperty = this;
            _keyframes.Add(keyframe);

            if (!KeyframesEnabled)
                KeyframesEnabled = true;

            SortKeyframes();
            OnKeyframeAdded();
        }

        /// <inheritdoc />
        public ILayerPropertyKeyframe? AddKeyframeEntity(KeyframeEntity keyframeEntity)
        {
            if (keyframeEntity.Position > ProfileElement.Timeline.Length)
                return null;
            T? value = CoreJson.DeserializeObject<T>(keyframeEntity.Value);
            if (value == null)
                return null;

            LayerPropertyKeyframe<T> keyframe = new(
                CoreJson.DeserializeObject<T>(keyframeEntity.Value)!, keyframeEntity.Position, (Easings.Functions) keyframeEntity.EasingFunction, this
            );
            AddKeyframe(keyframe);
            return keyframe;
        }

        /// <summary>
        ///     Removes a keyframe from the layer property
        /// </summary>
        /// <param name="keyframe">The keyframe to remove</param>
        public void RemoveKeyframe(LayerPropertyKeyframe<T> keyframe)
        {
            if (_disposed)
                throw new ObjectDisposedException("LayerProperty");

            if (!_keyframes.Contains(keyframe))
                return;

            _keyframes.Remove(keyframe);
            SortKeyframes();
            OnKeyframeRemoved();
        }

        /// <summary>
        ///     Sorts the keyframes in ascending order by position
        /// </summary>
        internal void SortKeyframes()
        {
            _keyframes.Sort((a, b) => a.Position.CompareTo(b.Position));
        }

        private void UpdateKeyframes(Timeline timeline)
        {
            if (!KeyframesSupported || !KeyframesEnabled)
                return;

            // The current keyframe is the last keyframe before the current time
            CurrentKeyframe = _keyframes.LastOrDefault(k => k.Position <= timeline.Position);
            // Keyframes are sorted by position so we can safely assume the next keyframe's position is after the current 
            if (CurrentKeyframe != null)
            {
                int nextIndex = _keyframes.IndexOf(CurrentKeyframe) + 1;
                NextKeyframe = _keyframes.Count > nextIndex ? _keyframes[nextIndex] : null;
            }
            else
            {
                NextKeyframe = null;
            }

            // No need to update the current value if either of the keyframes are null
            if (CurrentKeyframe == null)
            {
                CurrentValue = _keyframes.Any() ? _keyframes[0].Value : BaseValue;
            }
            else if (NextKeyframe == null)
            {
                CurrentValue = CurrentKeyframe.Value;
            }
            // Only determine progress and current value if both keyframes are present
            else
            {
                TimeSpan timeDiff = NextKeyframe.Position - CurrentKeyframe.Position;
                float keyframeProgress = (float) ((timeline.Position - CurrentKeyframe.Position).TotalMilliseconds / timeDiff.TotalMilliseconds);
                float keyframeProgressEased = (float) Easings.Interpolate(keyframeProgress, CurrentKeyframe.EasingFunction);
                UpdateCurrentValue(keyframeProgress, keyframeProgressEased);
            }
        }

        #endregion

        #region Data bindings

        /// <summary>
        ///     Gets the data binding of this property
        /// </summary>
        public DataBinding<T> DataBinding { get; private set; }

        /// <inheritdoc />
        public bool DataBindingsSupported => DataBinding.Properties.Any();

        /// <inheritdoc />
        public IDataBinding BaseDataBinding => DataBinding;

        /// <inheritdoc />
        public bool HasDataBinding => DataBinding.IsEnabled;

        #endregion

        #region Visbility

        /// <summary>
        ///     Set up a condition to hide the provided layer property when the condition evaluates to <see langword="true" />
        ///     <para>Note: overrides previous calls to <c>IsHiddenWhen</c> and <c>IsVisibleWhen</c></para>
        /// </summary>
        /// <typeparam name="TP">The type of the target layer property</typeparam>
        /// <param name="layerProperty">The target layer property</param>
        /// <param name="condition">The condition to evaluate to determine whether to hide the current layer property</param>
        public void IsHiddenWhen<TP>(TP layerProperty, Func<TP, bool> condition) where TP : ILayerProperty
        {
            IsHiddenWhen(layerProperty, condition, false);
        }

        /// <summary>
        ///     Set up a condition to show the provided layer property when the condition evaluates to <see langword="true" />
        ///     <para>Note: overrides previous calls to <c>IsHiddenWhen</c> and <c>IsVisibleWhen</c></para>
        /// </summary>
        /// <typeparam name="TP">The type of the target layer property</typeparam>
        /// <param name="layerProperty">The target layer property</param>
        /// <param name="condition">The condition to evaluate to determine whether to hide the current layer property</param>
        public void IsVisibleWhen<TP>(TP layerProperty, Func<TP, bool> condition) where TP : ILayerProperty
        {
            IsHiddenWhen(layerProperty, condition, true);
        }

        private void IsHiddenWhen<TP>(TP layerProperty, Func<TP, bool> condition, bool inverse) where TP : ILayerProperty
        {
            layerProperty.VisibilityChanged += LayerPropertyChanged;
            layerProperty.CurrentValueSet += LayerPropertyChanged;
            layerProperty.Disposed += LayerPropertyOnDisposed;

            void LayerPropertyChanged(object? sender, LayerPropertyEventArgs e)
            {
                if (inverse)
                    IsHidden = !condition(layerProperty);
                else
                    IsHidden = condition(layerProperty);
            }

            void LayerPropertyOnDisposed(object? sender, EventArgs e)
            {
                layerProperty.VisibilityChanged -= LayerPropertyChanged;
                layerProperty.CurrentValueSet -= LayerPropertyChanged;
                layerProperty.Disposed -= LayerPropertyOnDisposed;
            }

            if (inverse)
                IsHidden = !condition(layerProperty);
            else
                IsHidden = condition(layerProperty);
        }

        #endregion

        #region Storage

        private bool _isInitialized;

        /// <summary>
        ///     Indicates whether the BaseValue was loaded from storage, useful to check whether a default value must be applied
        /// </summary>
        public bool IsLoadedFromStorage { get; internal set; }

        internal PropertyEntity Entity { get; set; }

        /// <inheritdoc />
        public void Initialize(RenderProfileElement profileElement, LayerPropertyGroup group, PropertyEntity entity, bool fromStorage, PropertyDescriptionAttribute description)
        {
            if (_disposed)
                throw new ObjectDisposedException("LayerProperty");

            if (description.Identifier == null)
                throw new ArtemisCoreException("Can't initialize a property group without an identifier");

            _isInitialized = true;

            ProfileElement = profileElement ?? throw new ArgumentNullException(nameof(profileElement));
            LayerPropertyGroup = group ?? throw new ArgumentNullException(nameof(group));
            Entity = entity ?? throw new ArgumentNullException(nameof(entity));
            PropertyDescription = description ?? throw new ArgumentNullException(nameof(description));
            IsLoadedFromStorage = fromStorage;
            DataBinding = Entity.DataBinding != null ? new DataBinding<T>(this, Entity.DataBinding) : new DataBinding<T>(this);

            if (PropertyDescription.DisableKeyframes)
                KeyframesSupported = false;

            // Create the path to this property by walking up the tree
            Path = LayerPropertyGroup.Path + "." + description.Identifier;
            
            OnInitialize();
        }

        /// <inheritdoc />
        public void Load()
        {
            if (_disposed)
                throw new ObjectDisposedException("LayerProperty");

            if (!_isInitialized)
                throw new ArtemisCoreException("Layer property is not yet initialized");

            if (!IsLoadedFromStorage)
                ApplyDefaultValue();
            else
                try
                {
                    if (Entity.Value != null)
                        BaseValue = CoreJson.DeserializeObject<T>(Entity.Value)!;
                }
                catch (JsonException)
                {
                    // ignored for now
                }

            CurrentValue = BaseValue;
            KeyframesEnabled = Entity.KeyframesEnabled;

            _keyframes.Clear();
            try
            {
                foreach (KeyframeEntity keyframeEntity in Entity.KeyframeEntities.Where(k => k.Position <= ProfileElement.Timeline.Length))
                    AddKeyframeEntity(keyframeEntity);
            }
            catch (JsonException)
            {
                // ignored for now
            }

            DataBinding.Load();
        }

        /// <summary>
        ///     Saves the property to the underlying property entity
        /// </summary>
        public void Save()
        {
            if (_disposed)
                throw new ObjectDisposedException("LayerProperty");

            if (!_isInitialized)
                throw new ArtemisCoreException("Layer property is not yet initialized");

            Entity.Value = CoreJson.SerializeObject(BaseValue);
            Entity.KeyframesEnabled = KeyframesEnabled;
            Entity.KeyframeEntities.Clear();
            Entity.KeyframeEntities.AddRange(Keyframes.Select(k => k.GetKeyframeEntity()));

            DataBinding.Save();
            Entity.DataBinding = DataBinding.Entity;
        }

        /// <summary>
        ///     Called when the layer property has been initialized
        /// </summary>
        protected virtual void OnInitialize()
        {
        }

        #endregion

        #region Events

        /// <inheritdoc />
        public event EventHandler? Disposed;

        /// <inheritdoc />
        public event EventHandler<LayerPropertyEventArgs>? Updated;

        /// <inheritdoc />
        public event EventHandler<LayerPropertyEventArgs>? CurrentValueSet;

        /// <inheritdoc />
        public event EventHandler<LayerPropertyEventArgs>? VisibilityChanged;

        /// <inheritdoc />
        public event EventHandler<LayerPropertyEventArgs>? KeyframesToggled;

        /// <inheritdoc />
        public event EventHandler<LayerPropertyEventArgs>? KeyframeAdded;

        /// <inheritdoc />
        public event EventHandler<LayerPropertyEventArgs>? KeyframeRemoved;

        /// <summary>
        ///     Invokes the <see cref="Updated" /> event
        /// </summary>
        protected virtual void OnUpdated()
        {
            Updated?.Invoke(this, new LayerPropertyEventArgs(this));
        }

        /// <summary>
        ///     Invokes the <see cref="CurrentValueSet" /> event
        /// </summary>
        protected virtual void OnCurrentValueSet()
        {
            CurrentValueSet?.Invoke(this, new LayerPropertyEventArgs(this));
            LayerPropertyGroup.OnLayerPropertyOnCurrentValueSet(new LayerPropertyEventArgs(this));
        }

        /// <summary>
        ///     Invokes the <see cref="VisibilityChanged" /> event
        /// </summary>
        protected virtual void OnVisibilityChanged()
        {
            VisibilityChanged?.Invoke(this, new LayerPropertyEventArgs(this));
        }

        /// <summary>
        ///     Invokes the <see cref="KeyframesToggled" /> event
        /// </summary>
        protected virtual void OnKeyframesToggled()
        {
            KeyframesToggled?.Invoke(this, new LayerPropertyEventArgs(this));
        }

        /// <summary>
        ///     Invokes the <see cref="KeyframeAdded" /> event
        /// </summary>
        protected virtual void OnKeyframeAdded()
        {
            KeyframeAdded?.Invoke(this, new LayerPropertyEventArgs(this));
        }

        /// <summary>
        ///     Invokes the <see cref="KeyframeRemoved" /> event
        /// </summary>
        protected virtual void OnKeyframeRemoved()
        {
            KeyframeRemoved?.Invoke(this, new LayerPropertyEventArgs(this));
        }

        #endregion
    }
}