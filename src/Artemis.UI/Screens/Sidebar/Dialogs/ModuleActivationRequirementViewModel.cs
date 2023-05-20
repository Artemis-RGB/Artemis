using System;
using System.Reactive.Disposables;
using System.Timers;
using Artemis.Core.Modules;
using Artemis.UI.Shared;
using Humanizer;
using ReactiveUI;

namespace Artemis.UI.Screens.Sidebar;

public class ModuleActivationRequirementViewModel : ActivatableViewModelBase
{
    private readonly IModuleActivationRequirement _activationRequirement;
    private string _requirementDescription;
    private bool _requirementMet;

    public ModuleActivationRequirementViewModel(IModuleActivationRequirement activationRequirement)
    {
        RequirementName = activationRequirement.GetType().Name.Humanize();
        _requirementDescription = activationRequirement.GetUserFriendlyDescription();
        _activationRequirement = activationRequirement;

        this.WhenActivated(d =>
        {
            Timer updateTimer = new(TimeSpan.FromMilliseconds(500));
            updateTimer.Elapsed += (_, _) => Update();
            updateTimer.Start();
            updateTimer.DisposeWith(d);
        });
    }

    public string RequirementName { get; }

    public string RequirementDescription
    {
        get => _requirementDescription;
        set => RaiseAndSetIfChanged(ref _requirementDescription, value);
    }

    public bool RequirementMet
    {
        get => _requirementMet;
        set => RaiseAndSetIfChanged(ref _requirementMet, value);
    }

    private void Update()
    {
        RequirementDescription = _activationRequirement.GetUserFriendlyDescription();
        RequirementMet = _activationRequirement.Evaluate();
    }
}