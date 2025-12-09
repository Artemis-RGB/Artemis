using Artemis.Core;
using Artemis.UI.Shared;

namespace Artemis.UI.Screens.Profiles.ProfileEditor.DisplayCondition.ConditionTypes;

public class PlayOnceConditionViewModel : ViewModelBase
{
    private readonly PlayOnceCondition _playOnceCondition;

    public PlayOnceConditionViewModel(PlayOnceCondition playOnceCondition)
    {
        _playOnceCondition = playOnceCondition;
    }
}