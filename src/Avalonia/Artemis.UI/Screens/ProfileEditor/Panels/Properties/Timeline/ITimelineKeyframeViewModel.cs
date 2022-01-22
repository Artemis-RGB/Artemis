using System;
using Artemis.Core;
using Artemis.UI.Shared.Services.ProfileEditor.Commands;

namespace Artemis.UI.Screens.ProfileEditor.Properties.Timeline;

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
    void Duplicate();
    void Copy();
    void Paste();
    void Delete();

    #endregion
}