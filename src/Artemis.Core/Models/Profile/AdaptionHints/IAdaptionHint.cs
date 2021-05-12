using System.Collections.Generic;
using Artemis.Storage.Entities.Profile.AdaptionHints;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents an adaption hint that's used to adapt a layer to a set of devices
    /// </summary>
    public interface IAdaptionHint
    {
        /// <summary>
        ///     Applies the adaptive action to the provided layer
        /// </summary>
        /// <param name="layer">The layer to adapt</param>
        /// <param name="devices">The devices to adapt the layer for</param>
        void Apply(Layer layer, List<ArtemisDevice> devices);

        /// <summary>
        ///     Returns an adaption hint entry for this adaption hint used for persistent storage
        /// </summary>
        IAdaptionHintEntity GetEntry();
    }
}