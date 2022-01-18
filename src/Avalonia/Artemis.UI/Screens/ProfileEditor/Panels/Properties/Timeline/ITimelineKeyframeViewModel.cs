using System;
using Artemis.Core;

namespace Artemis.UI.Screens.ProfileEditor.Properties.Timeline;

public interface ITimelineKeyframeViewModel
{
    bool IsSelected { get; set; }
    TimeSpan Position { get; }
    ILayerPropertyKeyframe Keyframe { get; }

    #region Movement

    void SaveOffsetToKeyframe(ITimelineKeyframeViewModel source);
    void ApplyOffsetToKeyframe(ITimelineKeyframeViewModel source);
    void UpdatePosition(TimeSpan position);
    void ReleaseMovement();

    #endregion

    #region Context menu actions

    void PopulateEasingViewModels();
    void ClearEasingViewModels();
    void Delete(bool save = true);

    #endregion
}