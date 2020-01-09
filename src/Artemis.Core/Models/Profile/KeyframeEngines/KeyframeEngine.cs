using System;
using System.Collections.Generic;
using System.Linq;
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
        public BaseLayerProperty LayerProperty { get; private set; }

        /// <summary>
        ///     The total progress
        /// </summary>
        public TimeSpan Progress { get; private set; }

        /// <summary>
        ///     The progress from the current keyframe to the next 0 to 1
        /// </summary>
        public float KeyframeProgress { get; private set; }

        /// <summary>
        ///     The current keyframe
        /// </summary>
        public BaseKeyframe CurrentKeyframe { get; private set; }

        /// <summary>
        ///     The next keyframe
        /// </summary>
        public BaseKeyframe NextKeyframe { get; private set; }

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
            LayerProperty.KeyframeEngine = this;
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

            Progress = Progress.Add(TimeSpan.FromMilliseconds(deltaTime));

            // TODO Keep them sorted somewhere else, iterating all keyframes multiple times sucks
            var sortedKeyframes = LayerProperty.UntypedKeyframes.ToList().OrderBy(k => k.Position).ToList();

            CurrentKeyframe = sortedKeyframes.LastOrDefault(k => k.Position <= Progress);
            NextKeyframe = sortedKeyframes.FirstOrDefault(k => k.Position > Progress);
            if (CurrentKeyframe == null)
                KeyframeProgress = 0;
            else if (NextKeyframe == null)
                KeyframeProgress = 1;
            else
            {
                var timeDiff = NextKeyframe.Position - CurrentKeyframe.Position;
                KeyframeProgress = (float) ((Progress - CurrentKeyframe.Position).TotalMilliseconds / timeDiff.TotalMilliseconds);
            }

            // TODO Apply easing and store it separately

            // LayerProperty determines what's next: reset, stop, continue
        }

        /// <summary>
        ///     Overrides the engine's progress to the provided value
        /// </summary>
        /// <param name="progress"></param>
        public void OverrideProgress(TimeSpan progress)
        {
            Progress = TimeSpan.Zero;
            Update(progress.TotalMilliseconds);
        }

        /// <summary>
        ///     Gets the current value, if the progress is in between two keyframes the value will be interpolated
        /// </summary>
        /// <returns></returns>
        public object GetCurrentValue()
        {
            if (CurrentKeyframe == null)
                return LayerProperty.BaseValue;
            if (NextKeyframe == null)
                return CurrentKeyframe.BaseValue;

            return GetInterpolatedValue();
        }

        protected abstract object GetInterpolatedValue();
    }
}