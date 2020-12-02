using System;
using Artemis.Storage.Entities.Profile;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a keyframe on a <see cref="ILayerProperty" /> containing a value and a timestamp
    /// </summary>
    public interface ILayerPropertyKeyframe
    {
        /// <summary>
        ///     Gets an untyped reference to the layer property of this keyframe
        /// </summary>
        ILayerProperty UntypedLayerProperty { get; }

        /// <summary>
        ///     The position of this keyframe in the timeline
        /// </summary>
        TimeSpan Position { get; set; }

        /// <summary>
        ///     The easing function applied on the value of the keyframe
        /// </summary>
        Easings.Functions EasingFunction { get; set; }

        /// <summary>
        ///     Gets the entity this keyframe uses for persistent storage
        /// </summary>
        KeyframeEntity GetKeyframeEntity();

        /// <summary>
        ///     Removes the keyframe from the layer property
        /// </summary>
        void Remove();
    }
}