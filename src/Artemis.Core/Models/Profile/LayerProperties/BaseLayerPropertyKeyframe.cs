using System;
using Artemis.Core.Utilities;

namespace Artemis.Core.Models.Profile.LayerProperties
{
    /// <summary>
    ///     For internal use only, use <see cref="LayerPropertyKeyframe{T}" /> instead.
    /// </summary>
    public abstract class BaseLayerPropertyKeyframe
    {
        internal BaseLayerPropertyKeyframe()
        {
        }

        /// <summary>
        ///     The position of this keyframe in the timeline
        /// </summary>
        public abstract TimeSpan Position { get; set; }

        /// <summary>
        ///     The easing function applied on the value of the keyframe
        /// </summary>
        public abstract Easings.Functions EasingFunction { get; set; }

        internal abstract BaseLayerProperty BaseLayerProperty { get; }
    }
}