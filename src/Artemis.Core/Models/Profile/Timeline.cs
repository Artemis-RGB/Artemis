using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Storage.Entities.Profile;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a timeline used by profile elements
    /// </summary>
    public class Timeline : CorePropertyChanged, IStorageModel
    {
        private const int MaxExtraTimelines = 15;
        private readonly object _lock = new object();

        /// <summary>
        ///     Creates a new instance of the <see cref="Timeline" /> class
        /// </summary>
        public Timeline()
        {
            Entity = new TimelineEntity();
            MainSegmentLength = TimeSpan.FromSeconds(5);

            _extraTimelines = new List<Timeline>();

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
            Entity = new TimelineEntity();
            Parent = parent;
            StartSegmentLength = Parent.StartSegmentLength;
            MainSegmentLength = Parent.MainSegmentLength;
            EndSegmentLength = Parent.EndSegmentLength;

            _extraTimelines = new List<Timeline>();
        }

        #region Extra timelines

        /// <summary>
        ///     Adds an extra timeline to this timeline
        /// </summary>
        public void AddExtraTimeline()
        {
            _extraTimelines.Add(new Timeline(this));
            if (_extraTimelines.Count > MaxExtraTimelines)
                _extraTimelines.RemoveAt(0);
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
        private TimeSpan _startSegmentLength;
        private TimeSpan _mainSegmentLength;
        private TimeSpan _endSegmentLength;

        /// <summary>
        ///     Gets the parent this timeline is an extra timeline of
        /// </summary>
        public Timeline? Parent { get; }

        /// <summary>
        ///     Gets the current position of the timeline
        /// </summary>
        public TimeSpan Position
        {
            get => _position;
            private set => SetAndNotify(ref _position, value);
        }

        /// <summary>
        ///     Gets the cumulative delta of all calls to <see cref="Update" /> that took place after the last call to <see cref="ClearDelta"/>
        ///     <para>
        ///         Note: If this is an extra timeline <see cref="Delta" /> is always equal to <see cref="DeltaToParent" />
        ///     </para>
        /// </summary>
        public TimeSpan Delta
        {
            get => Parent == null ? _lastDelta : DeltaToParent;
            private set => SetAndNotify(ref _lastDelta, value);
        }

        /// <summary>
        ///     Gets the delta to this timeline's <see cref="Parent" />
        /// </summary>
        public TimeSpan DeltaToParent => Parent != null ? Position - Parent.Position : TimeSpan.Zero;

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
        public bool IsFinished => (Position > Length || Length == TimeSpan.Zero) && !ExtraTimelines.Any();

        /// <summary>
        /// Gets a boolean indicating whether the timeline progress has been overridden
        /// </summary>
        public bool IsOverridden { get; private set; }

        #region Segments

        /// <summary>
        ///     Gets the total length of this timeline
        /// </summary>
        public TimeSpan Length => StartSegmentLength + MainSegmentLength + EndSegmentLength;

        /// <summary>
        ///     Gets or sets the length of the start segment
        /// </summary>
        public TimeSpan StartSegmentLength
        {
            get => _startSegmentLength;
            set
            {
                if (SetAndNotify(ref _startSegmentLength, value))
                    NotifySegmentShiftAt(TimelineSegment.Start, false);
            }
        }

        /// <summary>
        ///     Gets or sets the length of the main segment
        /// </summary>
        public TimeSpan MainSegmentLength
        {
            get => _mainSegmentLength;
            set
            {
                if (SetAndNotify(ref _mainSegmentLength, value))
                    NotifySegmentShiftAt(TimelineSegment.Main, false);
            }
        }

        /// <summary>
        ///     Gets or sets the length of the end segment
        /// </summary>
        public TimeSpan EndSegmentLength
        {
            get => _endSegmentLength;
            set
            {
                if (SetAndNotify(ref _endSegmentLength, value))
                    NotifySegmentShiftAt(TimelineSegment.End, false);
            }
        }

        /// <summary>
        ///     Gets or sets the start position of the main segment
        /// </summary>
        public TimeSpan MainSegmentStartPosition
        {
            get => StartSegmentEndPosition;
            set
            {
                StartSegmentEndPosition = value;
                NotifySegmentShiftAt(TimelineSegment.Main, true);
            }
        }

        /// <summary>
        ///     Gets or sets the end position of the end segment
        /// </summary>
        public TimeSpan EndSegmentStartPosition
        {
            get => MainSegmentEndPosition;
            set
            {
                MainSegmentEndPosition = value;
                NotifySegmentShiftAt(TimelineSegment.End, true);
            }
        }

        /// <summary>
        ///     Gets or sets the end position of the start segment
        /// </summary>
        public TimeSpan StartSegmentEndPosition
        {
            get => StartSegmentLength;
            set
            {
                StartSegmentLength = value;
                NotifySegmentShiftAt(TimelineSegment.Start, false);
            }
        }

        /// <summary>
        ///     Gets or sets the end position of the main segment
        /// </summary>
        public TimeSpan MainSegmentEndPosition
        {
            get => StartSegmentEndPosition + MainSegmentLength;
            set
            {
                MainSegmentLength = value - StartSegmentEndPosition >= TimeSpan.Zero ? value - StartSegmentEndPosition : TimeSpan.Zero;
                NotifySegmentShiftAt(TimelineSegment.Main, false);
            }
        }

        /// <summary>
        ///     Gets or sets the end position of the end segment
        /// </summary>
        public TimeSpan EndSegmentEndPosition
        {
            get => MainSegmentEndPosition + EndSegmentLength;
            set
            {
                EndSegmentLength = value - MainSegmentEndPosition >= TimeSpan.Zero ? value - MainSegmentEndPosition : TimeSpan.Zero;
                NotifySegmentShiftAt(TimelineSegment.End, false);
            }
        }

        internal TimelineEntity Entity { get; set; }

        /// <summary>
        ///     Notifies the right segments in a way that I don't have to think about it
        /// </summary>
        /// <param name="segment">The segment that was updated</param>
        /// <param name="startUpdated">Whether the start point of the <paramref name="segment" /> was updated</param>
        private void NotifySegmentShiftAt(TimelineSegment segment, bool startUpdated)
        {
            if (segment <= TimelineSegment.End)
            {
                if (startUpdated || segment < TimelineSegment.End)
                    OnPropertyChanged(nameof(EndSegmentStartPosition));
                OnPropertyChanged(nameof(EndSegmentEndPosition));
            }

            if (segment <= TimelineSegment.Main)
            {
                if (startUpdated || segment < TimelineSegment.Main)
                    OnPropertyChanged(nameof(MainSegmentStartPosition));
                OnPropertyChanged(nameof(MainSegmentEndPosition));
            }

            if (segment <= TimelineSegment.Start) OnPropertyChanged(nameof(StartSegmentEndPosition));

            OnPropertyChanged(nameof(Length));
        }

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
            lock (_lock)
            {
                Delta += delta;
                Position += delta;
                IsOverridden = false;

                if (stickToMainSegment && Position >= MainSegmentStartPosition)
                {
                    // If the main segment has no length, simply stick to the start of the segment
                    if (MainSegmentLength == TimeSpan.Zero)
                        Position = MainSegmentStartPosition;
                    // Ensure wrapping back around retains the delta time
                    else
                        Position = MainSegmentStartPosition + TimeSpan.FromMilliseconds(Position.TotalMilliseconds % MainSegmentLength.TotalMilliseconds);
                }

                _extraTimelines.RemoveAll(t => t.IsFinished);
                foreach (Timeline extraTimeline in _extraTimelines)
                    extraTimeline.Update(delta, false);
            }
        }

        /// <summary>
        ///     Moves the position of the timeline backwards to the very start of the timeline
        /// </summary>
        public void JumpToStart()
        {
            lock (_lock)
            {
                if (Position == TimeSpan.Zero)
                    return;

                Delta = TimeSpan.Zero - Position;
                Position = TimeSpan.Zero;
            }
        }

        /// <summary>
        ///     Moves the position of the timeline forwards to the beginning of the end segment
        /// </summary>
        public void JumpToEndSegment()
        {
            lock (_lock)
            {
                if (Position >= EndSegmentStartPosition)
                    return;

                Delta = EndSegmentStartPosition - Position;
                Position = EndSegmentStartPosition;
            }
        }

        /// <summary>
        ///     Moves the position of the timeline forwards to the very end of the timeline
        /// </summary>
        public void JumpToEnd()
        {
            lock (_lock)
            {
                if (Position >= EndSegmentEndPosition)
                    return;

                Delta = EndSegmentEndPosition - Position;
                Position = EndSegmentEndPosition;
            }
        }

        /// <summary>
        ///     Overrides the <see cref="Position" /> to the specified time and clears any extra time lines
        /// </summary>
        /// <param name="position">The position to set the timeline to</param>
        /// <param name="stickToMainSegment">Whether to stick to the main segment, wrapping around if needed</param>
        public void Override(TimeSpan position, bool stickToMainSegment)
        {
            lock (_lock)
            {
                Delta += position - Position;
                Position = position;
                IsOverridden = true;

                if (stickToMainSegment && Position >= MainSegmentStartPosition)
                    Position = MainSegmentStartPosition + TimeSpan.FromMilliseconds(Position.TotalMilliseconds % MainSegmentLength.TotalMilliseconds);

                _extraTimelines.Clear();
            }
        }

        /// <summary>
        ///     Sets the <see cref="Delta" /> to <see cref="TimeSpan.Zero" />
        /// </summary>
        public void ClearDelta()
        {
            lock (_lock)
            {
                Delta = TimeSpan.Zero;
            }
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

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Progress: {Position}/{Length} - delta: {Delta}";
        }
    }

    internal enum TimelineSegment
    {
        Start,
        Main,
        End
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