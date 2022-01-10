using System;
using System.Collections.Generic;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.ProfileElementProperties.Timeline;

public interface ITimelinePropertyViewModel : IReactiveObject
{
    List<ITimelineKeyframeViewModel> GetAllKeyframeViewModels();
    void WipeKeyframes(TimeSpan? start, TimeSpan? end);
    void ShiftKeyframes(TimeSpan? start, TimeSpan? end, TimeSpan amount);
}