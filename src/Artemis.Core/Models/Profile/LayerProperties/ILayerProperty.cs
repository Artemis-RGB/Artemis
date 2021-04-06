﻿using System;
using System.Collections.Generic;
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
        ///     The parent group of this layer property, set after construction
        /// </summary>
        LayerPropertyGroup LayerPropertyGroup { get; }

        /// <summary>
        ///     Gets the unique path of the property on the layer
        /// </summary>
        string Path { get; }

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
        void Initialize(RenderProfileElement profileElement, LayerPropertyGroup group, PropertyEntity entity, bool fromStorage, PropertyDescriptionAttribute description, string path);

        /// <summary>
        ///     Returns a list off all data binding registrations
        /// </summary>
        List<IDataBindingRegistration> GetAllDataBindingRegistrations();

        /// <summary>
        ///     Attempts to load and add the provided keyframe entity to the layer property
        /// </summary>
        /// <param name="keyframeEntity">The entity representing the keyframe to add</param>
        /// <returns>If succeeded the resulting keyframe, otherwise <see langword="null" /></returns>
        ILayerPropertyKeyframe? AddKeyframeEntity(KeyframeEntity keyframeEntity);

        /// <summary>
        ///     Updates the layer properties internal state
        /// </summary>
        /// <param name="timeline">The timeline to apply to the property</param>
        void Update(Timeline timeline);

        #region Events

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

        /// <summary>
        ///     Occurs when a data binding property has been added
        /// </summary>
        public event EventHandler<LayerPropertyEventArgs>? DataBindingPropertyRegistered;

        /// <summary>
        ///     Occurs when all data binding properties have been removed
        /// </summary>
        public event EventHandler<LayerPropertyEventArgs>? DataBindingPropertiesCleared;

        /// <summary>
        ///     Occurs when a data binding has been enabled
        /// </summary>
        public event EventHandler<LayerPropertyEventArgs>? DataBindingEnabled;

        /// <summary>
        ///     Occurs when a data binding has been disabled
        /// </summary>
        public event EventHandler<LayerPropertyEventArgs>? DataBindingDisabled;

        #endregion
    }
}