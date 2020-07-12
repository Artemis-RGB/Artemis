using System;
using Artemis.Core.Utilities;

namespace Artemis.Core.Models.Profile.LayerProperties
{
    /// <summary>
    ///     For internal use only, use <see cref="LayerPropertyKeyframe{T}" /> instead.
    /// </summary>
    public abstract class BaseLayerPropertyKeyframe
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
        ///     The timeline this keyframe is contained in
        /// </summary>
        public abstract Timeline Timeline { get; set; }

        /// <summary>
        ///     The position of this keyframe in the timeline
        /// </summary>
        public abstract TimeSpan Position { get; set; }

        /// <summary>
        ///     The easing function applied on the value of the keyframe
        /// </summary>
        public Easings.Functions EasingFunction { get; set; }
    }

    public enum Timeline
    {
        Start,
        Main,
        End
    }
}