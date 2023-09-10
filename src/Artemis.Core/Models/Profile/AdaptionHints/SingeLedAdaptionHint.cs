using System.Collections.Generic;
using System.Linq;
using Artemis.Storage.Entities.Profile.AdaptionHints;
using RGB.NET.Core;

namespace Artemis.Core;

/// <summary>
///     Represents a hint that adapts layers to a single LED of one or more devices
/// </summary>
public class SingleLedAdaptionHint : CorePropertyChanged, IAdaptionHint
{
    private LedId _ledId;
    private int _skip;
    private bool _limitAmount;
    private int _amount;

    /// <summary>
    ///     Creates a new instance of the <see cref="SingleLedAdaptionHint" /> class
    /// </summary>
    public SingleLedAdaptionHint()
    {
    }

    internal SingleLedAdaptionHint(SingleLedAdaptionHintEntity entity)
    {
        LedId = (LedId) entity.LedId;
        Skip = entity.Skip;
        LimitAmount = entity.LimitAmount;
        Amount = entity.Amount;
    }

    /// <summary>
    /// Gets or sets the LED ID to apply to.
    /// </summary>
    public LedId LedId
    {
        get => _ledId;
        set => SetAndNotify(ref _ledId, value);
    }

    /// <summary>
    ///     Gets or sets the amount of devices to skip
    /// </summary>
    public int Skip
    {
        get => _skip;
        set => SetAndNotify(ref _skip, value);
    }

    /// <summary>
    ///     Gets or sets a boolean indicating whether a limited amount of devices should be used
    /// </summary>
    public bool LimitAmount
    {
        get => _limitAmount;
        set => SetAndNotify(ref _limitAmount, value);
    }

    /// <summary>
    ///     Gets or sets the amount of devices to limit to if <see cref="LimitAmount" /> is <see langword="true" />
    /// </summary>
    public int Amount
    {
        get => _amount;
        set => SetAndNotify(ref _amount, value);
    }

    #region Implementation of IAdaptionHint

    /// <inheritdoc />
    public void Apply(Layer layer, List<ArtemisDevice> devices)
    {
        IEnumerable<ArtemisDevice> matches = devices
            .Where(d => d.Leds.Any(l => l.RgbLed.Id == LedId))
            .OrderBy(d => d.Rectangle.Top)
            .ThenBy(d => d.Rectangle.Left)
            .Skip(Skip);
        if (LimitAmount)
            matches = matches.Take(Amount);

        foreach (ArtemisDevice artemisDevice in matches)
        {
            ArtemisLed led = artemisDevice.Leds.First(l => l.RgbLed.Id == LedId);
            layer.AddLed(led);
        }
    }

    /// <inheritdoc />
    public IAdaptionHintEntity GetEntry()
    {
        return new SingleLedAdaptionHintEntity {Amount = Amount, LimitAmount = LimitAmount, Skip = Skip, LedId = (int) LedId};
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Single LED adaption - {nameof(LedId)}: {LedId}, {nameof(Skip)}: {Skip}, {nameof(LimitAmount)}: {LimitAmount}, {nameof(Amount)}: {Amount}";
    }

    #endregion
}