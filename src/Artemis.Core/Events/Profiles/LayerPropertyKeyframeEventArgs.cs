using System;

namespace Artemis.Core
{
    /// <summary>
    ///     Provides data for layer property events.
    /// </summary>
    public class LayerPropertyKeyframeEventArgs : EventArgs
    {
        internal LayerPropertyKeyframeEventArgs(ILayerPropertyKeyframe keyframe)
        {
            Keyframe = keyframe;
        }

        /// <summary>
        /// Gets the keyframe this event is related to
        /// </summary>
        public ILayerPropertyKeyframe Keyframe { get; }
    }
}