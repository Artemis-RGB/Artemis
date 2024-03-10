namespace Artemis.Storage.Legacy.Entities.Profile;

internal class TimelineEntity
{
    public TimeSpan StartSegmentLength { get; set; }
    public TimeSpan MainSegmentLength { get; set; }
    public TimeSpan EndSegmentLength { get; set; }
}