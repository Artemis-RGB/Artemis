using System.Collections.Generic;
using System.Reactive;
using Artemis.Core;
using Artemis.UI.Shared;
using Avalonia;
using Humanizer;
using ReactiveUI;

namespace Artemis.UI.Screens.Profiles.ProfileEditor.Properties.Timeline.Keyframes;

public class TimelineEasingViewModel : ViewModelBase
{
    private readonly ILayerPropertyKeyframe _keyframe;

    public TimelineEasingViewModel(Easings.Functions easingFunction, ILayerPropertyKeyframe keyframe, ReactiveCommand<Easings.Functions, Unit> selectEasingFunction)
    {
        _keyframe = keyframe;

        EasingFunction = easingFunction;
        SelectEasingFunction = selectEasingFunction;
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
    public ReactiveCommand<Easings.Functions, Unit> SelectEasingFunction { get; }
    public List<Point> EasingPoints { get; }
    public string Description { get; }
    public bool IsEasingModeSelected => _keyframe.EasingFunction == EasingFunction;
}