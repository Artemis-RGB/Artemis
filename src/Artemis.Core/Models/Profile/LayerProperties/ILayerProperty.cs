using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Artemis.Storage.Entities.Profile;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a property on a layer. Properties are saved in storage and can optionally be modified from the UI.
    ///     <para>
    ///         Note: You cannot initialize layer properties yourself. If properly placed and annotated, the Artemis core will
    ///         initialize these for you.
    ///     </para>
    /// </summary>
    public interface ILayerProperty : IStorageModel, IDisposable
    {
        /// <summary>
        ///     Gets the description attribute applied to this property
        /// </summary>
        PropertyDescriptionAttribute PropertyDescription { get; }

        /// <summary>
        ///     Gets the profile element (such as layer or folder) this property is applied to
        /// </summary>
        RenderProfileElement ProfileElement { get; }

        /// <summary>
        ///     The parent group of this layer property, set after construction
        /// </summary>
        LayerPropertyGroup LayerPropertyGroup { get; }

        /// <summary>
        ///     Gets or sets whether the property is hidden in the UI
        /// </summary>
        public bool IsHidden { get; set; }

        /// <summary>
        ///     Gets the data binding of this property
        /// </summary>
        IDataBinding BaseDataBinding { get; }

        /// <summary>
        ///     Gets a boolean indicating whether the layer has any data binding properties
        /// </summary>
        public bool HasDataBinding { get; }

        /// <summary>
        ///     Gets a boolean indicating whether data bindings are supported on this type of property
        /// </summary>
        public bool DataBindingsSupported { get; }

        /// <summary>
        ///     Gets the unique path of the property on the render element
        /// </summary>
        string Path { get; }

        /// <summary>
        ///     Gets a read-only list of all the keyframes on this layer property
        /// </summary>
        ReadOnlyCollection<ILayerPropertyKeyframe> UntypedKeyframes { get; }

        /// <summary>
        ///     Gets the type of the property
        /// </summary>
        Type PropertyType { get; }

        /// <summary>
        ///     Indicates whether the BaseValue was loaded from storage, useful to check whether a default value must be applied
        /// </summary>
        bool IsLoadedFromStorage { get; }

        /// <summary>
        ///     Initializes the layer property
        ///     <para>
        ///         Note: This isn't done in the constructor to keep it parameterless which is easier for implementations of
        ///         <see cref="LayerProperty{T}" />
        ///     </para>
        /// </summary>
        void Initialize(RenderProfileElement profileElement, LayerPropertyGroup group, PropertyEntity entity, bool fromStorage, PropertyDescriptionAttribute description);

        /// <summary>
        ///     Attempts to load and add the provided keyframe entity to the layer property
        /// </summary>
        /// <param name="keyframeEntity">The entity representing the keyframe to add</param>
        /// <returns>If succeeded the resulting keyframe, otherwise <see langword="null" /></returns>
        ILayerPropertyKeyframe? AddKeyframeEntity(KeyframeEntity keyframeEntity);

        /// <summary>
        ///     Overrides the property value with the default value
        /// </summary>
        void ApplyDefaultValue();

        /// <summary>
        ///     Updates the layer properties internal state
        /// </summary>
        /// <param name="timeline">The timeline to apply to the property</param>
        void Update(Timeline timeline);


        /// <summary>
        /// Updates just the data binding instead of the entire layer
        /// </summary>
        void UpdateDataBinding();

        /// <summary>
        /// Removes a keyframe from the layer property without knowing it's type.
        /// <para>Prefer <see cref="LayerProperty{T}.RemoveKeyframe"/>.</para>
        /// </summary>
        /// <param name="keyframe"></param>
        void RemoveUntypedKeyframe(ILayerPropertyKeyframe keyframe);

        /// <summary>
        /// Adds a keyframe to the layer property without knowing it's type.
        /// <para>Prefer <see cref="LayerProperty{T}.AddKeyframe"/>.</para>
        /// </summary>
        /// <param name="keyframe"></param>
        void AddUntypedKeyframe(ILayerPropertyKeyframe keyframe);

        /// <summary>
        ///     Occurs when the layer property is disposed
        /// </summary>
        public event EventHandler Disposed;

        /// <summary>
        ///     Occurs once every frame when the layer property is updated
        /// </summary>
        public event EventHandler<LayerPropertyEventArgs>? Updated;

        /// <summary>
        ///     Occurs when the current value of the layer property was updated by some form of input
        /// </summary>
        public event EventHandler<LayerPropertyEventArgs>? CurrentValueSet;

        /// <summary>
        ///     Occurs when the visibility value of the layer property was updated
        /// </summary>
        public event EventHandler<LayerPropertyEventArgs>? VisibilityChanged;

        /// <summary>
        ///     Occurs when keyframes are enabled/disabled
        /// </summary>
        public event EventHandler<LayerPropertyEventArgs>? KeyframesToggled;

        /// <summary>
        ///     Occurs when a new keyframe was added to the layer property
        /// </summary>
        public event EventHandler<LayerPropertyEventArgs>? KeyframeAdded;

        /// <summary>
        ///     Occurs when a keyframe was removed from the layer property
        /// </summary>
        public event EventHandler<LayerPropertyEventArgs>? KeyframeRemoved;
    }
}