using Artemis.Core;
using Artemis.UI.Shared;

namespace Artemis.UI.Screens.Profile.ProfileEditor.DisplayCondition.ConditionTypes;

public class AlwaysOnConditionViewModel : ViewModelBase
{
    private readonly AlwaysOnCondition _alwaysOnCondition;

    public AlwaysOnConditionViewModel(AlwaysOnCondition alwaysOnCondition)
    {
        _alwaysOnCondition = alwaysOnCondition;
    }
}