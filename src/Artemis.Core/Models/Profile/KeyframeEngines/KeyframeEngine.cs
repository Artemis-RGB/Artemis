using System;
using System.Collections.Generic;
using Artemis.Core.Exceptions;
using Artemis.Core.Models.Profile.LayerProperties;

namespace Artemis.Core.Models.Profile.KeyframeEngines
{
    public abstract class KeyframeEngine
    {
        /// <summary>
        ///     Indicates whether <see cref="Initialize" /> has been called.
        /// </summary>
        public bool Initialized { get; private set; }

        /// <summary>
        ///     The layer property this keyframe engine applies to.
        /// </summary>
        public BaseLayerProperty LayerProperty { get; set; }

        /// <summary>
        ///     The keyframe progress in milliseconds.
        /// </summary>
        public double Progress { get; set; }

        /// <summary>
        ///     The types this keyframe engine supports.
        /// </summary>
        public abstract List<Type> CompatibleTypes { get; }

        /// <summary>
        ///     Associates the keyframe engine with the provided layer property.
        /// </summary>
        /// <param name="layerProperty"></param>
        public void Initialize(BaseLayerProperty layerProperty)
        {
            if (Initialized)
                throw new ArtemisCoreException("Cannot initialize the same keyframe engine twice");
            if (!CompatibleTypes.Contains(layerProperty.Type))
                throw new ArtemisCoreException($"This property engine does not support the provided type {layerProperty.Type.Name}");

            LayerProperty = layerProperty;
            Initialized = true;
        }

        /// <summary>
        ///     Updates the engine's progress
        /// </summary>
        /// <param name="deltaTime"></param>
        public void Update(double deltaTime)
        {
            if (!Initialized)
                return;

            Progress += deltaTime;

            // LayerProperty determines what's next: reset, stop, continue
        }

        /// <summary>
        ///     Gets the current value, if the progress is in between two keyframes the value will be interpolated
        /// </summary>
        /// <returns></returns>
        public abstract object GetCurrentValue();
    }
}