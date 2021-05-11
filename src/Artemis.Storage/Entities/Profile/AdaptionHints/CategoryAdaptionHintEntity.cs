﻿namespace Artemis.Storage.Entities.Profile.AdaptionHints
{
    public class CategoryAdaptionHintEntity : IAdaptionHintEntity
    {
        public int Category { get; set; }

        public bool ApplyToAllMatches { get; set; }
        public int Skip { get; set; }
        public int Amount { get; set; }
    }


}