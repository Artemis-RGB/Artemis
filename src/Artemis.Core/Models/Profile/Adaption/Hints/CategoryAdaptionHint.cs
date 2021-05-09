namespace Artemis.Core
{
    public class CategoryAdaptionHint : IAdaptionHint
    {
        public AdaptionCategory Category { get; set; }

        public bool ApplyToAllMatches { get; set; }
        public int Skip { get; set; }
        public int Amount { get; set; }
    }

    public enum AdaptionCategory
    {
        Desk,
        Monitor,
        Case,
        Room,
        Peripherals
    }
}