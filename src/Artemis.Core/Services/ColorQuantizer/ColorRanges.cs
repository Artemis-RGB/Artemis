namespace Artemis.Core.Services;

internal readonly struct ColorRanges
{
    #region Properties & Fields

    public readonly byte RedRange;
    public readonly byte GreenRange;
    public readonly byte BlueRange;

    #endregion

    #region Constructors

    public ColorRanges(byte redRange, byte greenRange, byte blueRange)
    {
        this.RedRange = redRange;
        this.GreenRange = greenRange;
        this.BlueRange = blueRange;
    }

    #endregion
}