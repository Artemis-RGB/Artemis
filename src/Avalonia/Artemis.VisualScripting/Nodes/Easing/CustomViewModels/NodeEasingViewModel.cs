﻿using Artemis.Core;
using Artemis.UI.Shared;
using Avalonia;
using Humanizer;

namespace Artemis.VisualScripting.Nodes.Easing.CustomViewModels;

public class NodeEasingViewModel : ViewModelBase
{
    public NodeEasingViewModel(Easings.Functions easingFunction)
    {
        EasingFunction = easingFunction;
        Description = easingFunction.Humanize();

        EasingPoints = new List<Point>();
        for (int i = 1; i <= 10; i++)
        {
            int x = i;
            double y = Easings.Interpolate(i / 10.0, EasingFunction) * 10;
            EasingPoints.Add(new Point(x, y));
        }
    }

    public Easings.Functions EasingFunction { get; }
    public List<Point> EasingPoints { get; }
    public string Description { get; }
}