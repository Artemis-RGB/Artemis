using System.Collections.Generic;
using Artemis.Core;
using Artemis.UI.Shared;
using Avalonia;
using Humanizer;

namespace Artemis.UI.Screens.ProfileEditor.Properties.Timeline;

public class TimelineEasingViewModel : ViewModelBase
{
    private readonly ILayerPropertyKeyframe _keyframe;

    public TimelineEasingViewModel(Easings.Functions easingFunction, ILayerPropertyKeyframe keyframe)
    {
        _keyframe = keyframe;

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
    public bool IsEasingModeSelected => _keyframe.EasingFunction == EasingFunction;
}