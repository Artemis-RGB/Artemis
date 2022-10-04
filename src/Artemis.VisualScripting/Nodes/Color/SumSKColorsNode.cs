﻿using Artemis.Core;
using SkiaSharp;

namespace Artemis.VisualScripting.Nodes.Color;

[Node("Sum (Color)", "Sums the connected color values.", "Color", InputType = typeof(SKColor), OutputType = typeof(SKColor))]
public class SumSKColorsNode : Node
{
    #region Constructors

    public SumSKColorsNode()
    {
        Name = "Sum";
        Values = CreateInputPinCollection<SKColor>("Values", 2);
        Sum = CreateOutputPin<SKColor>("Sum");
    }

    #endregion

    #region Methods

    public override void Evaluate()
    {
        SKColor result = SKColor.Empty;

        bool first = true;
        foreach (SKColor current in Values.Values)
        {
            result = first ? current : result.Sum(current);
            first = false;
        }

        Sum.Value = result;
    }

    #endregion

    #region Properties & Fields

    public InputPinCollection<SKColor> Values { get; }

    public OutputPin<SKColor> Sum { get; }

    #endregion
}