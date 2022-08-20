using System;
using Artemis.UI.Shared;

namespace Artemis.UI.Screens.ProfileEditor.DisplayCondition;

public class ConditionTypeViewModel : ViewModelBase
{
    public ConditionTypeViewModel(string name, string description, Type conditionType)
    {
        Name = name;
        Description = description;
        ConditionType = conditionType;
    }

    public string Name { get; }
    public string Description { get; }
    public Type ConditionType { get; }
}