using System;
using Artemis.Core;

namespace Artemis.UI.Shared.Services.ProfileEditor.Commands;

/// <summary>
///     Represents a profile editor command that can be used to change the length of a timeline segment.
/// </summary>
public class ResizeTimelineSegment : IProfileEditorCommand
{
    private readonly TimeSpan _length;
    private readonly TimeSpan _originalLength;
    private readonly RenderProfileElement _profileElement;
    private readonly SegmentType _segmentType;

    /// <summary>
    ///     Creates a new instance of the <see cref="ResizeTimelineSegment" /> class.
    /// </summary>
    /// <param name="segmentType">The type of segment to resize.</param>
    /// <param name="profileElement">The render profile element whose segment to resize.</param>
    /// <param name="length">The new length of the segment</param>
    public ResizeTimelineSegment(SegmentType segmentType, RenderProfileElement profileElement, TimeSpan length)
    {
        _segmentType = segmentType;
        _profileElement = profileElement;
        _length = length;
        _originalLength = _segmentType switch
        {
            SegmentType.Start => _profileElement.Timeline.StartSegmentLength,
            SegmentType.Main => _profileElement.Timeline.MainSegmentLength,
            SegmentType.End => _profileElement.Timeline.EndSegmentLength,
            _ => _originalLength
        };
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="ResizeTimelineSegment" /> class.
    /// </summary>
    /// <param name="segmentType">The type of segment to resize.</param>
    /// <param name="profileElement">The render profile element whose segment to resize.</param>
    /// <param name="length">The new length of the segment</param>
    /// <param name="originalLength">The original length of the segment</param>
    public ResizeTimelineSegment(SegmentType segmentType, RenderProfileElement profileElement, TimeSpan length, TimeSpan originalLength)
    {
        _segmentType = segmentType;
        _profileElement = profileElement;
        _length = length;
        _originalLength = originalLength;
    }

    /// <inheritdoc />
    public string DisplayName => $"Resize {_segmentType.ToString().ToLower()} segment";

    /// <inheritdoc />
    public void Execute()
    {
        switch (_segmentType)
        {
            case SegmentType.Start:
                _profileElement.Timeline.StartSegmentLength = _length;
                break;
            case SegmentType.Main:
                _profileElement.Timeline.MainSegmentLength = _length;
                break;
            case SegmentType.End:
                _profileElement.Timeline.EndSegmentLength = _length;
                break;
        }
    }

    /// <inheritdoc />
    public void Undo()
    {
        switch (_segmentType)
        {
            case SegmentType.Start:
                _profileElement.Timeline.StartSegmentLength = _originalLength;
                break;
            case SegmentType.Main:
                _profileElement.Timeline.MainSegmentLength = _originalLength;
                break;
            case SegmentType.End:
                _profileElement.Timeline.EndSegmentLength = _originalLength;
                break;
        }
    }

    /// <summary>
    ///     Represents a type of segment on a timeline.
    /// </summary>
    public enum SegmentType
    {
        /// <summary>
        ///     The start segment.
        /// </summary>
        Start,

        /// <summary>
        ///     The main segment.
        /// </summary>
        Main,

        /// <summary>
        ///     The end segment.
        /// </summary>
        End
    }
}