using System;
using System.Linq;

namespace Artemis.Core;

public static class NodeExtension
{
    #region Methods

    public static double EstimateHeight(this INode node)
    {
        const double PIN_HEIGHT = 26;
        const double TITLE_HEIGHT = 46;

        int inputPinCount = node.Pins.Count(x => x.Direction == PinDirection.Input)
                          + node.PinCollections.Where(x => x.Direction == PinDirection.Input).Sum(x => x.Count() + 1);
        int outputPinCount = node.Pins.Count(x => x.Direction == PinDirection.Output)
                           + node.PinCollections.Where(x => x.Direction == PinDirection.Output).Sum(x => x.Count() + 1);

        return TITLE_HEIGHT + (Math.Max(inputPinCount, outputPinCount) * PIN_HEIGHT);
    }

    public static double EstimateWidth(this INode node) => 120; // DarthAffe 13.09.2022: For now just assume they are all the same size

    #endregion
}