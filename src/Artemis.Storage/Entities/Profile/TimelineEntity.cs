using System;

namespace Artemis.Storage.Entities.Profile
{
    public class TimelineEntity
    {
        public TimeSpan StartSegmentLength { get; set; }
        public TimeSpan MainSegmentLength { get; set; }
        public TimeSpan EndSegmentLength { get; set; }

        public int PlayMode { get; set; }
        public int StopMode { get; set; }
        public int EventOverlapMode { get; set; }
    }
}