using System;
using Artemis.Core.Utilities;
using Stylet;

namespace Artemis.Core.Models.Profile.LayerProperties
{
    /// <summary>
    ///     For internal use only, use <see cref="LayerPropertyKeyframe{T}" /> instead.
    /// </summary>
    public abstract class BaseLayerPropertyKeyframe : PropertyChangedBase
    {
        internal BaseLayerPropertyKeyframe(BaseLayerProperty baseLayerProperty)
        {
            BaseLayerProperty = baseLayerProperty;
        }

        /// <summary>
        ///     The base class of the layer property this keyframe is applied to
        /// </summary>
        public BaseLayerProperty BaseLayerProperty { get; internal set; }

        /// <summary>
        ///     The position of this keyframe in the timeline
        /// </summary>
        public abstract TimeSpan Position { get; set; }

        /// <summary>
        ///     The easing function applied on the value of the keyframe
        /// </summary>
        public Easings.Functions EasingFunction { get; set; }

        /// <summary>
        ///     Removes the keyframe from the layer property
        /// </summary>
        public abstract void Remove();
    }
}