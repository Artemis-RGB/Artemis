namespace Artemis.Storage.Migrator.Legacy.Entities.Profile;

public class TimelineEntity
{
    public TimeSpan StartSegmentLength { get; set; }
    public TimeSpan MainSegmentLength { get; set; }
    public TimeSpan EndSegmentLength { get; set; }
}