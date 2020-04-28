using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core.Utilities;

namespace Artemis.Core.Models.Profile.LayerProperties.Abstract
{
    public abstract class LayerProperty<T>
    {
        private List<LayerPropertyKeyFrame<T>> _keyframes;

        protected LayerProperty()
        {
            _keyframes = new List<LayerPropertyKeyFrame<T>>();
        }

        public T BaseValue { get; set; }
        public T CurrentValue { get; set; }
        public IReadOnlyList<LayerPropertyKeyFrame<T>> Keyframes => _keyframes.AsReadOnly();

        /// <summary>
        ///     The total progress on the timeline
        /// </summary>
        public TimeSpan TimelineProgress { get; private set; }

        /// <summary>
        ///     The current keyframe in the timeline
        /// </summary>
        public LayerPropertyKeyFrame<T> CurrentKeyframe { get; protected set; }

        /// <summary>
        ///     The next keyframe in the timeline
        /// </summary>
        public LayerPropertyKeyFrame<T> NextKeyframe { get; protected set; }

        public void Update(double deltaTime)
        {
            float keyframeProgress;
            float keyframeProgressEased;

            TimelineProgress = TimelineProgress.Add(TimeSpan.FromSeconds(deltaTime));
            // The current keyframe is the last keyframe before the current time
            CurrentKeyframe = _keyframes.LastOrDefault(k => k.Position <= TimelineProgress);
            // The next keyframe is the first keyframe that's after the current time
            NextKeyframe = _keyframes.FirstOrDefault(k => k.Position > TimelineProgress);

            if (CurrentKeyframe == null)
            {
                keyframeProgress = 0;
                keyframeProgressEased = 0;
            }
            else if (NextKeyframe == null)
            {
                keyframeProgress = 1;
                keyframeProgressEased = 1;
            }
            else
            {
                var timeDiff = NextKeyframe.Position - CurrentKeyframe.Position;
                keyframeProgress = (float) ((TimelineProgress - CurrentKeyframe.Position).TotalMilliseconds / timeDiff.TotalMilliseconds);
                keyframeProgressEased = (float) Easings.Interpolate(keyframeProgress, CurrentKeyframe.EasingFunction);
            }

            UpdateCurrentValue(keyframeProgress, keyframeProgressEased);
        }

        protected abstract void UpdateCurrentValue(float keyframeProgress, float keyframeProgressEased);

        public void OverrideProgress(TimeSpan progress)
        {
            TimelineProgress = TimeSpan.Zero;
            Update(progress.TotalSeconds);
        }

        internal void SortKeyframes()
        {
            _keyframes = _keyframes.OrderBy(k => k.Position).ToList();
        }
    }
}