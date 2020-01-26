using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Artemis.Core.Exceptions;
using Artemis.Core.Models.Profile.LayerProperties;
using Artemis.Core.Utilities;

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
        ///     The progress from the current keyframe to the next.
        ///     <para>Range 0.0 to 1.0.</para>
        /// </summary>
        public float KeyframeProgress { get; private set; }

        /// <summary>
        ///     The progress from the current keyframe to the next with the current keyframes easing function applied.
        ///     <para>Range 0.0 to 1.0 but can be higher than 1.0 depending on easing function.</para>
        /// </summary>
        public float KeyframeProgressEased { get; set; }

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

            var keyframes = LayerProperty.UntypedKeyframes.ToList();
            Progress = Progress.Add(TimeSpan.FromSeconds(deltaTime));
            // The current keyframe is the last keyframe before the current time
            CurrentKeyframe = keyframes.LastOrDefault(k => k.Position <= Progress);
            // The next keyframe is the first keyframe that's after the current time
            NextKeyframe = keyframes.FirstOrDefault(k => k.Position > Progress);

            if (CurrentKeyframe == null)
            {
                KeyframeProgress = 0;
                KeyframeProgressEased = 0;
            }
            else if (NextKeyframe == null)
            {
                KeyframeProgress = 1;
                KeyframeProgressEased = 1;
            }
            else
            {
                var timeDiff = NextKeyframe.Position - CurrentKeyframe.Position;
                KeyframeProgress = (float) ((Progress - CurrentKeyframe.Position).TotalMilliseconds / timeDiff.TotalMilliseconds);
                KeyframeProgressEased = (float) Easings.Interpolate(KeyframeProgress, CurrentKeyframe.EasingFunction);
            }

            // LayerProperty determines what's next: reset, stop, continue
        }


        /// <summary>
        ///     Overrides the engine's progress to the provided value
        /// </summary>
        /// <param name="progress"></param>
        public void OverrideProgress(TimeSpan progress)
        {
            Progress = TimeSpan.Zero;
            Update(progress.TotalSeconds);
        }

        /// <summary>
        ///     Gets the current value, if the progress is in between two keyframes the value will be interpolated
        /// </summary>
        /// <returns></returns>
        public object GetCurrentValue()
        {
            if (CurrentKeyframe == null && LayerProperty.UntypedKeyframes.Any())
                return LayerProperty.UntypedKeyframes.First().BaseValue;
            if (CurrentKeyframe == null)
                return LayerProperty.BaseValue;
            if (NextKeyframe == null)
                return CurrentKeyframe.BaseValue;

            return GetInterpolatedValue();
        }

        protected abstract object GetInterpolatedValue();
    }
}