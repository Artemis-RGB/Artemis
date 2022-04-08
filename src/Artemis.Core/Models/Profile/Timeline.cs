using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Storage.Entities.Profile;

namespace Artemis.Core;

/// <summary>
///     Represents a timeline used by profile elements
/// </summary>
public class Timeline : CorePropertyChanged, IStorageModel
{
    private readonly object _lock = new();

    /// <summary>
    ///     Creates a new instance of the <see cref="Timeline" /> class
    /// </summary>
    public Timeline()
    {
        Entity = new TimelineEntity();
        MainSegmentLength = TimeSpan.FromSeconds(5);

        Save();
    }

    internal Timeline(TimelineEntity entity)
    {
        Entity = entity;

        Load();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Progress: {Position}/{Length} - delta: {Delta}";
    }

    #region Properties

    private TimeSpan _position;
    private TimeSpan _lastDelta;

    private TimeSpan _startSegmentLength;
    private TimeSpan _mainSegmentLength;
    private TimeSpan _endSegmentLength;
    private TimeSpan _lastOverride;

    /// <summary>
    ///     Gets the current position of the timeline
    /// </summary>
    public TimeSpan Position
    {
        get => _position;
        private set => SetAndNotify(ref _position, value);
    }

    /// <summary>
    ///     Gets the cumulative delta of all calls to <see cref="Update" /> that took place after the last call to
    ///     <see cref="ClearDelta" />
    /// </summary>
    public TimeSpan Delta
    {
        get => _lastDelta;
        private set => SetAndNotify(ref _lastDelta, value);
    }



    /// <summary>
    ///     Gets a boolean indicating whether the timeline has finished its run
    /// </summary>
    public bool IsFinished => Position > Length;

    /// <summary>
    ///     Gets a boolean indicating whether the timeline progress has been overridden
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

        if (segment <= TimelineSegment.Start)
            OnPropertyChanged(nameof(StartSegmentEndPosition));

        OnPropertyChanged(nameof(Length));
        OnTimelineChanged();
    }

    /// <summary>
    ///     Occurs when changes have been made to any of the segments of the timeline.
    /// </summary>
    public event EventHandler? TimelineChanged;

    private void OnTimelineChanged()
    {
        TimelineChanged?.Invoke(this, EventArgs.Empty);
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
            if (IsOverridden)
                throw new ArtemisCoreException("Can't update an overridden timeline, call ClearOverride first.");

            Delta += delta;
            Position += delta;

            if (!stickToMainSegment || Position <= MainSegmentEndPosition)
                return;

            // If the main segment has no length, simply stick to the start of the segment
            if (MainSegmentLength == TimeSpan.Zero)
                Position = MainSegmentStartPosition;
            // Ensure wrapping back around retains the delta time
            else
                Position = MainSegmentStartPosition + TimeSpan.FromMilliseconds(delta.TotalMilliseconds % MainSegmentLength.TotalMilliseconds);
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
    ///     Overrides the <see cref="Position" /> to the specified time
    /// </summary>
    /// <param name="position">The position to set the timeline to</param>
    /// <param name="stickToMainSegment">Whether to stick to the main segment, wrapping around if needed</param>
    internal void Override(TimeSpan position, bool stickToMainSegment)
    {
        lock (_lock)
        {
            if (_lastOverride == TimeSpan.Zero)
                Delta = Position - position;
            else
                Delta = position - _lastOverride;

            Position = position;
            IsOverridden = true;
            _lastOverride = position;

            if (!stickToMainSegment || Position < MainSegmentStartPosition)
                return;

            bool atSegmentStart = Position >= MainSegmentStartPosition;
            if (MainSegmentLength > TimeSpan.Zero)
            {
                Position = MainSegmentStartPosition + TimeSpan.FromMilliseconds(Position.TotalMilliseconds % MainSegmentLength.TotalMilliseconds);
                // If the cursor is at the end of the timeline we don't want to wrap back around yet so only allow going to the start if the cursor
                // is actually at the start of the segment
                if (Position == MainSegmentStartPosition && !atSegmentStart)
                    Position = MainSegmentEndPosition;
            }
            else
            {
                Position = MainSegmentStartPosition;
            }
        }
    }

    internal void ClearOverride()
    {
        IsOverridden = false;
        _lastOverride = TimeSpan.Zero;
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

        JumpToEnd();
    }

    /// <inheritdoc />
    public void Save()
    {
        Entity.StartSegmentLength = StartSegmentLength;
        Entity.MainSegmentLength = MainSegmentLength;
        Entity.EndSegmentLength = EndSegmentLength;
    }

    #endregion
}

internal enum TimelineSegment
{
    Start,
    Main,
    End
}
