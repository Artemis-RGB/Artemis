using System;
using System.Collections.Generic;
using Artemis.Storage.Entities.Profile;

namespace Artemis.Core.Models.Profile.LayerProperties
{
    /// <summary>
    /// For internal use only, to implement your own layer property type, extend <see cref="LayerProperty{T}"/> instead.
    /// </summary>
    public abstract class BaseLayerProperty
    {
        /// <summary>
        ///     Used to declare that this property doesn't belong to a plugin and should use the core plugin GUID
        /// </summary>
        internal bool IsCoreProperty { get; set; }
        internal PropertyEntity PropertyEntity { get; set; }
        internal LayerPropertyGroup LayerPropertyGroup { get; set; }

        /// <summary>
        ///     Gets whether keyframes are supported on this property
        /// </summary>
        public bool KeyframesSupported { get; protected set; }

        /// <summary>
        ///     Gets or sets whether keyframes are enabled on this property, has no effect if <see cref="KeyframesSupported" /> is
        ///     False
        /// </summary>
        public bool KeyframesEnabled { get; set; }

        /// <summary>
        ///     Gets or sets whether the property is hidden in the UI
        /// </summary>
        public bool IsHidden { get; set; }

        /// <summary>
        ///     Indicates whether the BaseValue was loaded from storage, useful to check whether a default value must be applied
        /// </summary>
        public bool IsLoadedFromStorage { get; internal set; }

        /// <summary>
        ///     Gets the total progress on the timeline
        /// </summary>
        public TimeSpan TimelineProgress { get; internal set; }

        /// <summary>
        ///     Applies the provided property entity to the layer property by deserializing the JSON base value and keyframe values
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="layerPropertyGroup"></param>
        internal abstract void ApplyToLayerProperty(PropertyEntity entity, LayerPropertyGroup layerPropertyGroup);

        /// <summary>
        ///     Saves the property to the underlying property entity that was configured when calling
        ///     <see cref="ApplyToLayerProperty" />
        /// </summary>
        internal abstract void ApplyToEntity();

        internal abstract List<TimeSpan> GetKeyframePositions();
        internal abstract TimeSpan GetLastKeyframePosition();
    }
}