using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Artemis.Storage.Entities.Profile;
using Stylet;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a timeline used by profile elements
    /// </summary>
    public class Timeline : PropertyChangedBase, IStorageModel
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="Timeline" /> class
        /// </summary>
        public Timeline()
        {
            Entity = new TimelineEntity();
            _extraTimelines = new List<Timeline>();
            MainSegmentLength = TimeSpan.FromSeconds(5);

            Save();
        }

        internal Timeline(TimelineEntity entity)
        {
            Entity = entity;
            _extraTimelines = new List<Timeline>();

            Load();
        }

        private Timeline(Timeline parent)
        {
            Parent = parent;
        }

        #region Extra timelines

        /// <summary>
        ///     Adds an extra timeline to this timeline
        /// </summary>
        public void AddExtraTimeline()
        {
            _extraTimelines.Add(new Timeline(this));
        }

        /// <summary>
        ///     Removes all extra timelines from this timeline
        /// </summary>
        public void ClearExtraTimelines()
        {
            _extraTimelines.Clear();
        }

        #endregion

        #region Properties

        private TimeSpan _position;
        private TimeSpan _lastDelta;
        private TimeLineEventOverlapMode _eventOverlapMode;
        private TimelinePlayMode _playMode;
        private TimelineStopMode _stopMode;
        private readonly List<Timeline> _extraTimelines;

        /// <summary>
        ///     Gets the parent this timeline is an extra timeline of
        /// </summary>
        public Timeline Parent { get; }

        /// <summary>
        ///     Gets the current position of the timeline
        /// </summary>
        public TimeSpan Position
        {
            get => _position;
            private set => SetAndNotify(ref _position, value);
        }

        /// <summary>
        ///     Gets the delta that was applied during the last call to <see cref="Update" />
        /// </summary>
        public TimeSpan LastDelta
        {
            get => _lastDelta;
            private set => SetAndNotify(ref _lastDelta, value);
        }

        /// <summary>
        ///     Gets or sets the mode in which the render element starts its timeline when display conditions are met
        /// </summary>
        public TimelinePlayMode PlayMode
        {
            get => _playMode;
            set => SetAndNotify(ref _playMode, value);
        }

        /// <summary>
        ///     Gets or sets the mode in which the render element stops its timeline when display conditions are no longer met
        /// </summary>
        public TimelineStopMode StopMode
        {
            get => _stopMode;
            set => SetAndNotify(ref _stopMode, value);
        }

        /// <summary>
        ///     Gets or sets the mode in which the render element responds to display condition events being fired before the
        ///     timeline finished
        /// </summary>
        public TimeLineEventOverlapMode EventOverlapMode
        {
            get => _eventOverlapMode;
            set => SetAndNotify(ref _eventOverlapMode, value);
        }

        /// <summary>
        ///     Gets a list of extra copies of the timeline applied to this timeline
        /// </summary>
        public ReadOnlyCollection<Timeline> ExtraTimelines => _extraTimelines.AsReadOnly();

        /// <summary>
        ///     Gets a boolean indicating whether the timeline has finished its run
        /// </summary>
        public bool IsFinished => Position > Length;

        #region Segments

        /// <summary>
        ///     Gets the total length of this timeline
        /// </summary>
        public TimeSpan Length => StartSegmentLength + MainSegmentLength + EndSegmentLength;

        /// <summary>
        ///     Gets or sets the length of the start segment
        /// </summary>
        public TimeSpan StartSegmentLength { get; set; }

        /// <summary>
        ///     Gets or sets the length of the main segment
        /// </summary>
        public TimeSpan MainSegmentLength { get; set; }

        /// <summary>
        ///     Gets or sets the length of the end segment
        /// </summary>
        public TimeSpan EndSegmentLength { get; set; }

        /// <summary>
        ///     Gets or sets the start position of the main segment
        /// </summary>
        public TimeSpan MainSegmentStartPosition
        {
            get => StartSegmentEndPosition;
            set => StartSegmentEndPosition = value;
        }

        /// <summary>
        ///     Gets or sets the end position of the end segment
        /// </summary>
        public TimeSpan EndSegmentStartPosition
        {
            get => MainSegmentEndPosition;
            set => MainSegmentEndPosition = value;
        }

        /// <summary>
        ///     Gets or sets the end position of the start segment
        /// </summary>
        public TimeSpan StartSegmentEndPosition
        {
            get => StartSegmentLength;
            set => StartSegmentLength = value;
        }

        /// <summary>
        ///     Gets or sets the end position of the main segment
        /// </summary>
        public TimeSpan MainSegmentEndPosition
        {
            get => StartSegmentEndPosition + MainSegmentLength;
            set => MainSegmentLength = value - StartSegmentEndPosition >= TimeSpan.Zero ? value - StartSegmentEndPosition : TimeSpan.Zero;
        }

        /// <summary>
        ///     Gets or sets the end position of the end segment
        /// </summary>
        public TimeSpan EndSegmentEndPosition
        {
            get => MainSegmentEndPosition + EndSegmentLength;
            set => EndSegmentLength = value - MainSegmentEndPosition >= TimeSpan.Zero ? value - MainSegmentEndPosition : TimeSpan.Zero;
        }

        internal TimelineEntity Entity { get; set; }

        #endregion

        #endregion

        #region Updating

        /// <summary>
        ///     Updates the timeline, applying the provided <paramref name="delta" /> to the <see cref="Position" />
        /// </summary>
        /// <param name="delta">The amount of time to apply to the position</param>
        /// <param name="stickToMainSegment">Whether to stick to the main segment, wrapping around if needed</param>
        public void Update(TimeSpan delta, bool stickToMainSegment)
        {
            LastDelta = delta;
            Position += delta;

            if (stickToMainSegment && Position >= MainSegmentStartPosition)
                Position = MainSegmentStartPosition + TimeSpan.FromMilliseconds(Position.TotalMilliseconds % MainSegmentLength.TotalMilliseconds);
        }

        /// <summary>
        ///     Moves the position of the timeline backwards to the very start of the timeline
        /// </summary>
        public void JumpToStart()
        {
            if (Position == TimeSpan.Zero)
                return;

            LastDelta = TimeSpan.Zero - Position;
            Position = TimeSpan.Zero;
        }

        /// <summary>
        ///     Moves the position of the timeline forwards to the beginning of the end segment
        /// </summary>
        public void JumpToEndSegment()
        {
            if (Position >= EndSegmentStartPosition)
                return;

            LastDelta = EndSegmentStartPosition - Position;
            Position = EndSegmentStartPosition;
        }

        /// <summary>
        ///     Moves the position of the timeline forwards to the very end of the timeline
        /// </summary>
        public void JumpToEnd()
        {
            if (Position >= EndSegmentEndPosition)
                return;

            LastDelta = EndSegmentEndPosition - Position;
            Position = EndSegmentEndPosition;
        }

        /// <summary>
        ///     Overrides the <see cref="Position" /> to the specified time and clears any extra time lines
        /// </summary>
        /// <param name="position">The position to set the timeline to</param>
        /// <param name="stickToMainSegment">Whether to stick to the main segment, wrapping around if needed</param>
        public void Override(TimeSpan position, bool stickToMainSegment)
        {
            _extraTimelines.Clear();

            LastDelta = position - Position;
            Position = position;
            if (stickToMainSegment && Position >= MainSegmentStartPosition)
                Position = MainSegmentStartPosition + TimeSpan.FromMilliseconds(Position.TotalMilliseconds % MainSegmentLength.TotalMilliseconds);
        }

        #endregion

        #region Storage

        /// <inheritdoc />
        public void Load()
        {
            StartSegmentLength = Entity.StartSegmentLength;
            MainSegmentLength = Entity.MainSegmentLength;
            EndSegmentLength = Entity.EndSegmentLength;
            PlayMode = (TimelinePlayMode) Entity.PlayMode;
            StopMode = (TimelineStopMode) Entity.StopMode;
            EventOverlapMode = (TimeLineEventOverlapMode) Entity.EventOverlapMode;
        }

        /// <inheritdoc />
        public void Save()
        {
            Entity.StartSegmentLength = StartSegmentLength;
            Entity.MainSegmentLength = MainSegmentLength;
            Entity.EndSegmentLength = EndSegmentLength;
            Entity.PlayMode = (int) PlayMode;
            Entity.StopMode = (int) StopMode;
            Entity.EventOverlapMode = (int) EventOverlapMode;
        }

        #endregion
    }

    /// <summary>
    ///     Represents a mode for render elements to start their timeline when display conditions are met
    /// </summary>
    public enum TimelinePlayMode
    {
        /// <summary>
        ///     Continue repeating the main segment of the timeline while the condition is met
        /// </summary>
        Repeat,

        /// <summary>
        ///     Only play the timeline once when the condition is met
        /// </summary>
        Once
    }

    /// <summary>
    ///     Represents a mode for render elements to stop their timeline when display conditions are no longer met
    /// </summary>
    public enum TimelineStopMode
    {
        /// <summary>
        ///     When conditions are no longer met, finish the the current run of the main timeline
        /// </summary>
        Finish,

        /// <summary>
        ///     When conditions are no longer met, skip to the end segment of the timeline
        /// </summary>
        SkipToEnd
    }

    /// <summary>
    ///     Represents a mode for render elements to start their timeline when display conditions events are fired
    /// </summary>
    public enum TimeLineEventOverlapMode
    {
        /// <summary>
        ///     Stop the current run and restart the timeline
        /// </summary>
        Restart,

        /// <summary>
        ///     Ignore subsequent event fires until the timeline finishes
        /// </summary>
        Ignore,

        /// <summary>
        ///     Play another copy of the timeline on top of the current run
        /// </summary>
        Copy
    }
}