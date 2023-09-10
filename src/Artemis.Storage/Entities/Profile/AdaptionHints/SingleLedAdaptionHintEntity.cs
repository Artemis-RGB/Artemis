namespace Artemis.Storage.Entities.Profile.AdaptionHints;

public class SingleLedAdaptionHintEntity : IAdaptionHintEntity
{
    public int LedId { get; set; }

    public bool LimitAmount { get; set; }
    public int Skip { get; set; }
    public int Amount { get; set; }
}