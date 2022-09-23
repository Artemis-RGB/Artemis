namespace Artemis.Core;

/// <summary>
///     Represents the position of a node's custom view model.
/// </summary>
public enum CustomNodeViewModelPosition
{
    /// <summary>
    ///     Puts the view model above the pins.
    /// </summary>
    AbovePins,

    /// <summary>
    ///     Puts the view model between the pins, vertically aligned to the top.
    /// </summary>
    BetweenPinsTop,

    /// <summary>
    ///     Puts the view model between the pins, vertically aligned to the center.
    /// </summary>
    BetweenPinsCenter,

    /// <summary>
    ///     Puts the view model between the pins, vertically aligned to the bottom.
    /// </summary>
    BetweenPinsBottom,

    /// <summary>
    ///     Puts the view model below the pins.
    /// </summary>
    BelowPins
}