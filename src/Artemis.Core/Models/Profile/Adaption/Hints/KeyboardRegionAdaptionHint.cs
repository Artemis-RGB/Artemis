namespace Artemis.Core
{
    public class KeyboardRegionAdaptionHint : IAdaptionHint
    {
        public AdaptionKeyboardRegion Region { get; set; }
    }

    public enum AdaptionKeyboardRegion
    {
        FKeys,
        NumPad,
        MacroKeys,
        MovementKeys,
        LedStrips
    }
}