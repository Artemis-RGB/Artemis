using System;
using System.Reactive;
using Artemis.Core;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.Properties.Timeline.Keyframes;

public interface ITimelineKeyframeViewModel
{
    bool IsSelected { get; }
    TimeSpan Position { get; }
    ILayerPropertyKeyframe Keyframe { get; }

    #region Movement

    void Select(bool expand, bool toggle);
    void StartMovement(ITimelineKeyframeViewModel source);
    void UpdateMovement(TimeSpan position);
    void FinishMovement();
    TimeSpan GetTimeSpanAtPosition(double x);

    #endregion

    #region Context menu actions

    void PopulateEasingViewModels();

    ReactiveCommand<Unit, Unit> Duplicate { get; }
    ReactiveCommand<Unit, Unit> Copy { get; }
    ReactiveCommand<Unit, Unit> Paste { get; }
    ReactiveCommand<Unit, Unit> Delete { get; }

    #endregion
}