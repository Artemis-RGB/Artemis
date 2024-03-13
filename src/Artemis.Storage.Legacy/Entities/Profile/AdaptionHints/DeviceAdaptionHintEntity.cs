namespace Artemis.Storage.Legacy.Entities.Profile.AdaptionHints;

internal class DeviceAdaptionHintEntity : IAdaptionHintEntity
{
    public int DeviceType { get; set; }

    public bool LimitAmount { get; set; }
    public int Skip { get; set; }
    public int Amount { get; set; }
}